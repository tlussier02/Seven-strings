using System;
using UnityEngine;

public class CalibrationMetronome : MonoBehaviour
{
    public AudioClip AudioBeat;
    public float Bpm = 120f;
    public double Lookahead = 0.2;
    public double VisualDelay;
    
    public bool IsRunning { get; private set; }
    public double FirstBeatTime { get; private set; }
    public double Interval => 60.0 / Bpm;
    public event Action OnBeat;

    private AudioSource[] sources = new AudioSource[2];
    private int sourceIndex;
    private double nextScheduledTime;
    private double nextVisualTime;

    void Awake()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            GameObject beat = new GameObject("Beat" + i);
            beat.transform.SetParent(transform);
            sources[i] = beat.AddComponent<AudioSource>();
            sources[i].clip = AudioBeat;
            sources[i].playOnAwake = false;
        }
    }
    
    void Update()
    {
        if (!IsRunning) return;
        double now = AudioSettings.dspTime;

        while (nextScheduledTime < now + Lookahead)
        {
            sources[sourceIndex].PlayScheduled(nextScheduledTime);
            sourceIndex = (sourceIndex + 1) % sources.Length;
            nextScheduledTime += Interval;
        }

        while (now >= nextVisualTime + VisualDelay)
        {
            OnBeat?.Invoke();
            nextVisualTime += Interval;
        }
    }

    public void StartMetronome()
    {
        FirstBeatTime = AudioSettings.dspTime + 1.0;
        nextScheduledTime = FirstBeatTime;
        nextVisualTime = FirstBeatTime;
        IsRunning = true;
    }

    public void StopMetronome()
    {
        IsRunning = false;
        foreach (AudioSource source in sources) 
            source.Stop();
    }
}