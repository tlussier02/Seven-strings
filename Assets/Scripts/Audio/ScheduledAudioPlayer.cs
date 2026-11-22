using System.Linq;
using UnityEngine;

public class ScheduledAudioPlayer : MonoBehaviour
{
    public AudioClip[] Stems;
    [Range(0f, 1f)] public float[] StemVolumes = new float[0];
    public AudioSource[] StemSources { get; private set; }
    public int StemCount => StemSources != null ? StemSources.Length : 0;

    public double SongOffset { get; private set; }
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

            // StemLayerMixer takes over from here
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

    // Default set to 1f
    private float StemVolume(int i) =>
        StemVolumes != null && i < StemVolumes.Length ? StemVolumes[i] : 1f;

    private void ApplyVolume(int i)
    {
        if (StemSources[i] != null)
            StemSources[i].volume = musicVolume * stemGains[i] * StemVolume(i);
    }

    void OnValidate()
    {
        // Added one volume slider per stem
        int count = Stems != null ? Stems.Length : 0;
        if (StemVolumes == null || StemVolumes.Length != count)
        {
            float[] resized = new float[count];
            for (int i = 0; i < count; i++)
                resized[i] = StemVolumes != null && i < StemVolumes.Length ? StemVolumes[i] : 1f;
            StemVolumes = resized;
        }
    }

    public void ScheduleFrom(double songTime)
    {
        SongOffset = songTime;
        ScheduledStartTime = AudioSettings.dspTime + 2d;
        IsScheduled = true;
        IsRunning = false;
        foreach (AudioSource source in StemSources)
        {
            source.Stop();
            if (source.clip == null) continue;
            source.timeSamples = (int)System.Math.Min(source.clip.samples - 1,
                System.Math.Round(songTime * source.clip.frequency));
        }
    }

    public void Tick()
    {
        double time = AudioSettings.dspTime;
        if (!IsRunning && time + 1.0f > ScheduledStartTime)
        {
            // Same DSP timestamp for every stem
            foreach (AudioSource source in StemSources)
                source.PlayScheduled(ScheduledStartTime);
            IsRunning = true;
        }
    }

    public float GetMaxLength()
    {
        return Stems != null && Stems.Length > 0 ? Stems.Max(x => x != null ? x.length : 0f) : 0f;
    }
}
