using System;
using UnityEngine;

/// <summary>
/// Data for one prompted key press tied to a beat from DSPBeatClock.
/// Beat 0 is the first beat at the clock start time.
/// </summary>
[Serializable]
public class BeatPrompt
{
    public int beatIndex;
    public KeyCode requiredKey = KeyCode.Space;
    public bool wasHit;

    public BeatPrompt(int beatIndex, KeyCode requiredKey)
    {
        this.beatIndex = Mathf.Max(0, beatIndex);
        this.requiredKey = requiredKey;
    }

    public void MarkHit()
    {
        wasHit = true;
    }

    public void Reset()
    {
        wasHit = false;
    }
}
