using UnityEngine;

public class ScheduledAudioPlayer : MonoBehaviour
{
    public AudioClip[] Stems;
    public AudioSource[] StemSources { get; private set; }
    public int StemCount => StemSources != null ? StemSources.Length : 0;

    public double ScheduledStartTime { get; private set; }
    public bool IsScheduled { get; private set; }
    public bool IsRunning { get; private set; }

    private float musicVolume = 1f;
    private float[] stemGains;

    void Awake()
    {
        ScheduledStartTime = AudioSettings.dspTime + 2f;
        IsScheduled = true;

        AudioClip[] clips = Stems;
        StemSources = new AudioSource[clips.Length];
        stemGains = new float[clips.Length];

        for (int i = 0; i < clips.Length; i++)
        {
            GameObject child = new GameObject($"Stem{i}");
            child.transform.parent = transform;
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.ignoreListenerPause = false;
            source.clip = clips[i];
            StemSources[i] = source;

            // Base layer audible by default; StemLayerMixer takes over from here.
            stemGains[i] = i == 0 ? 1f : 0f;
        }
        
        SetMusicVolume(AudioVolumeSettings.MusicVolume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        for (int i = 0; i < StemCount; i++)
            ApplyVolume(i);
    }

    //Fade in
    public void SetStemGain(int stemIndex, float gain)
    {
        if (stemIndex < 0 || stemIndex >= StemCount)
            return;
        stemGains[stemIndex] = Mathf.Clamp01(gain);
        ApplyVolume(stemIndex);
    }

    public float GetStemGain(int stemIndex) =>
        stemIndex >= 0 && stemIndex < StemCount ? stemGains[stemIndex] : 0f;

    private void ApplyVolume(int i)
    {
        if (StemSources[i] != null)
            StemSources[i].volume = musicVolume * stemGains[i];
    }

    public void Tick()
    {
        double time = AudioSettings.dspTime;
        if (!IsRunning && time + 1.0f > ScheduledStartTime)
        {
            // Same DSP timestamp for every stem = sample-locked layers.
            foreach (AudioSource source in StemSources)
                source.PlayScheduled(ScheduledStartTime);
            IsRunning = true;

            Debug.Log($"Scheduled {StemCount} stem(s) to start at dsp time {ScheduledStartTime} | DSP time: {time}");
        }
    }
}
