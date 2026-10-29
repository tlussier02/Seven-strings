using System;
using UnityEngine;

[Serializable]
public struct InputSfx
{
    public InputType inputType;
    public AudioClip clip; 
}

public class SfxPlayer : MonoBehaviour
{
    [SerializeField] private InputSfx[] sounds;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("Perfect Jump Flourish")]
    [Tooltip("Layered on top of the normal jump sound when a jump is judged Perfect (e.g. jump_v1).")]
    [SerializeField] private AudioClip perfectJumpFlourish;
    [SerializeField, Range(0f, 1f)] private float flourishVolume = 1f;
    
    private AudioSource audioSource;

    void Awake()
    {
        // Separate source from ScheduledAudioPlayer so SFX never touch the music track
        GameObject child = new GameObject("SfxSource");
        child.transform.parent = transform;
        audioSource = child.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    // Signature matches Action<InputType>, so it can subscribe directly to InputHandler.OnInputPressed
    public void PlayFor(InputType input)
    {
        foreach (InputSfx sound in sounds)
        {
            
            if (sound.inputType == input && sound.clip != null)
            {
                audioSource.PlayOneShot(sound.clip, volume);
                return; 
            }
        }
    }

    // Signature matches NoteManager.OnJudgement. Fires on the same frame as the key press,
    // so the flourish lands together with the base jump sound from PlayFor.
    public void HandleJudgement(NoteData note, InputType input, InputJudge.Judgement judgement)
    {
        if (input == InputType.Jump && judgement == InputJudge.Judgement.Perfect && perfectJumpFlourish != null)
            audioSource.PlayOneShot(perfectJumpFlourish, flourishVolume);
    }
}
