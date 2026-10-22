using System;
using System.Collections.Generic;
using UnityEngine;

public class ReceptorClock : MonoBehaviour
{
    public ScheduledAudioPlayer ScheduledAudioPlayer;
    public double Receptor { get; private set; }
    public double LastNoteTime { get; private set; }
    public event Action<NoteData> OnBeat;
    public NoteData? NextNote => CurrentNoteIndex < Notes.Count ? Notes[CurrentNoteIndex] : null;
    public bool FoundOvertime { get; private set; }
    
    private List<NoteData> Notes;
    private double NextNoteTime;
    private int CurrentNoteIndex;
    private double LatencyCompensation;

    void Awake()
    {
        LatencyCompensation = PlayerPrefs.GetFloat("LatencyCompensation", 0f);
    }

    public void Tick()
    {
        double time = AudioSettings.dspTime;
        Receptor +=  Time.deltaTime;
        
        if (!FoundOvertime && time >= ScheduledAudioPlayer.ScheduledStartTime + LatencyCompensation)
        {
            double overtime = time - (ScheduledAudioPlayer.ScheduledStartTime + LatencyCompensation);
            Receptor = overtime;
            FoundOvertime = true;
        }
        
        if (FoundOvertime && Receptor >= NextNoteTime && CurrentNoteIndex < Notes.Count)
        {
            OnBeat?.Invoke(Notes[CurrentNoteIndex]);
            LastNoteTime = NextNoteTime;
            CurrentNoteIndex++;

            if (CurrentNoteIndex < Notes.Count)
                NextNoteTime = Notes[CurrentNoteIndex].Time;
            else
                NextNoteTime = double.PositiveInfinity;
        }
    }
    
    public void SetNotes(List<NoteData> notes)
    {
        Notes = notes;
        CurrentNoteIndex = 0;
        
        NextNoteTime = Notes[CurrentNoteIndex].Time;
        // might have to define LastBeatTime
    }
}
