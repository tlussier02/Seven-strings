using System;
using UnityEngine;

[Serializable]
public struct InputSfx
{
    public InputType inputType;
    public AudioClip clip; // turn into an array to randomly pick a sfx variation in PlayFor
}

public class SfxPlayer : MonoBehaviour
{
    [SerializeField] private InputSfx[] sounds;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    
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
            // pick an AudioClip[] Clips at random
            if (sound.inputType == input && sound.clip != null)
            {
                audioSource.PlayOneShot(sound.clip, volume);
                return; // first matching entry wins
            }
        }
    }
}