using System;
using System.Collections.Generic;
using UnityEngine;

public class ReceptorClock : MonoBehaviour
{
    public ScheduledAudioPlayer ScheduledAudioPlayer;
    public double Receptor { get; private set; }
    public double NextBeatTime { get; private set; }
    public double LastBeatTime { get; private set; }
    public double LatencyCompensation; // need to create a way to manually calibrate and adjust
    public Action<NoteData> OnBeat; //jumping between Note and Beat naming. pick one

    private bool FoundOvertime;
    private List<NoteData> Notes;
    private int CurrentNoteIndex;

    public void Tick()
    {
        double time = AudioSettings.dspTime;
        Receptor +=  Time.deltaTime;
        
        if (!FoundOvertime && time >= ScheduledAudioPlayer.ScheduledStartTime + LatencyCompensation)
        {
            double overtime = time - (ScheduledAudioPlayer.ScheduledStartTime + LatencyCompensation);
            Receptor -= overtime;
            FoundOvertime = true;
        }
        
        if (Receptor >= NextBeatTime)
        {
            OnBeat?.Invoke(Notes[CurrentNoteIndex]);
            LastBeatTime = NextBeatTime;
            NextBeatTime = Notes[++CurrentNoteIndex].Time;
        }
    }
    
    public void SetNotes(List<NoteData> notes)
    {
        Notes = notes;
        CurrentNoteIndex = 0;
        
        NextBeatTime = Notes[CurrentNoteIndex].Time;
        // might have to define LastBeatTime
    }
}
