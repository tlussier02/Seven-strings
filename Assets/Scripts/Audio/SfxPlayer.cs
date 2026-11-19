using System;
using UnityEngine;
using UnityEngine.Serialization;

// A class so the volume default of 1 applies to all wav files.
[Serializable]
public class InputSfx
{
    public InputType inputType;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f; // individual file; 1 = unchanged
}

public class SfxPlayer : MonoBehaviour
{
    [SerializeField] private InputSfx[] sounds;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("Perfect Flourish")]
    [FormerlySerializedAs("perfectJumpFlourish")] // prevents scenes from breaking after the name change
    [SerializeField] private AudioClip perfectFlourish;
    [SerializeField, Range(0f, 1f)] private float flourishVolume = 1f;

    [Header("Hold Sustain")]
    [SerializeField] private AudioClip holdSustainClip;
    [SerializeField, Range(0f, 1f)] private float holdSustainVolume = 0.2f;
    [SerializeField, Min(0f)] private float holdReleaseFade = 0.25f;// Seconds to fade out on release

    [Header("Checkpoint")]
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField, Range(0f, 1f)] private float checkpointVolume = 1f;
    [SerializeField] private CheckpointManager checkpointManager;
    
    private AudioSource audioSource;
    private AudioSource sustainSource; // separate source so stopping the sustain never cuts off one-shots
    private float sfxVolume = 1f;
    private float sustainFadeTimer = -1f; // < 0 means not fading

    void Awake()
    {
        // Separate source from ScheduledAudioPlayer so SFX never touch the music track
        GameObject child = new GameObject("SfxSource");
        child.transform.parent = transform;
        audioSource = child.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        SetSfxVolume(AudioVolumeSettings.SfxVolume);
        audioSource.spatialBlend = 0f; // 2D sound

        GameObject sustainChild = new GameObject("SfxSustainSource");
        sustainChild.transform.parent = transform;
        sustainSource = sustainChild.AddComponent<AudioSource>();
        sustainSource.playOnAwake = false;
        sustainSource.spatialBlend = 0f;
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        audioSource.volume = sfxVolume;
        if (sustainSource != null && sustainFadeTimer < 0f)
            sustainSource.volume = sfxVolume * holdSustainVolume;
    }

    // Subscribed to NoteManager.OnHoldStarted.
    public void StartHoldSustain(NoteData note)
    {
        if (holdSustainClip == null) return;
        sustainFadeTimer = -1f;
        sustainSource.clip = holdSustainClip;
        sustainSource.volume = sfxVolume * holdSustainVolume;
        sustainSource.Play();
    }

    // Subscribed to NoteManager.OnHoldEnded
    public void StopHoldSustain(NoteData note)
    {
        if (!sustainSource.isPlaying) return;
        if (holdReleaseFade <= 0f) { sustainSource.Stop(); return; }
        sustainFadeTimer = holdReleaseFade;
    }

    void Update()
    {
        if (sustainFadeTimer < 0f) return;
        // unscaled so the fade still finishes if the game is paused mid-release
        sustainFadeTimer -= Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(sustainFadeTimer / holdReleaseFade);
        sustainSource.volume = sfxVolume * holdSustainVolume * t;
        if (sustainFadeTimer <= 0f)
        {
            sustainSource.Stop();
            sustainFadeTimer = -1f;
        }
    }

    void Start()
    {
        if (checkpointManager == null) checkpointManager = GetComponent<CheckpointManager>();
        if (checkpointManager == null) checkpointManager = FindFirstObjectByType<CheckpointManager>();
        if (checkpointManager != null) checkpointManager.OnCheckpointActivated.AddListener(PlayCheckpoint);
    }

    void OnDestroy()
    {
        if (checkpointManager != null) checkpointManager.OnCheckpointActivated.RemoveListener(PlayCheckpoint);
    }

    // Subscribed to CheckpointManager.OnCheckpointActivated in Start()
    public void PlayCheckpoint() => PlayEffect(checkpointSound, checkpointVolume);

    // volumeScale lets the caller set a per-sound volume on top of the shared Volume.
    public void PlayEffect(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null) audioSource.PlayOneShot(clip, volume * volumeScale);
    }
    
    public void PlayFor(InputType input)
    {
        foreach (InputSfx sound in sounds)
        {
            
            if (sound != null && sound.inputType == input && sound.clip != null)
            {
                audioSource.PlayOneShot(sound.clip, volume * sound.volume);
                return; 
            }
        }
    }

    // matches NoteManager.OnJudgement
    public void HandleJudgement(NoteData note, InputType input, InputJudge.Judgement judgement)
    {
        if (judgement != InputJudge.Judgement.Perfect || perfectFlourish == null)
            return;

        audioSource.PlayOneShot(perfectFlourish, flourishVolume);
    }
}
