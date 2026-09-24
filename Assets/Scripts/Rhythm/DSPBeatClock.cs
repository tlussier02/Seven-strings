using UnityEngine;

/// <summary>
/// Simple DSP-based beat clock for rhythm prototypes.
/// Call StartClock with the same DSP time used by AudioSource.PlayScheduled.
/// </summary>
public class DSPBeatClock : MonoBehaviour
{
    [Min(1f)]
    public float bpm = 120f;

    public bool IsRunning { get; private set; }
    public double StartDspTime { get; private set; }

    public double CurrentDspTime => AudioSettings.dspTime;
    public double SecondsPerBeat => 60.0 / Mathf.Max(1f, bpm);

    public double SongPositionSeconds
    {
        get
        {
            if (!IsRunning)
            {
                return 0.0;
            }

            return CurrentDspTime - StartDspTime;
        }
    }

    public void StartClock(double startDspTime)
    {
        StartDspTime = startDspTime;
        IsRunning = true;
    }

    public void StartClockNow(float delaySeconds = 0f)
    {
        StartClock(AudioSettings.dspTime + Mathf.Max(0f, delaySeconds));
    }

    public void StopClock()
    {
        IsRunning = false;
    }

    public double GetBeatDspTime(int beatIndex)
    {
        return StartDspTime + Mathf.Max(0, beatIndex) * SecondsPerBeat;
    }

    public int GetCurrentBeatIndex()
    {
        if (!IsRunning || SongPositionSeconds < 0.0)
        {
            return -1;
        }

        return Mathf.FloorToInt((float)(SongPositionSeconds / SecondsPerBeat));
    }

    public int GetNearestBeatIndex()
    {
        if (!IsRunning || SongPositionSeconds < 0.0)
        {
            return -1;
        }

        return Mathf.RoundToInt((float)(SongPositionSeconds / SecondsPerBeat));
    }

    public double GetOffsetFromBeat(int beatIndex)
    {
        return CurrentDspTime - GetBeatDspTime(beatIndex);
    }

    private void OnValidate()
    {
        bpm = Mathf.Max(1f, bpm);
    }
}
