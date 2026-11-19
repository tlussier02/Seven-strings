using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public ReceptorClock ReceptorClock { get; set; }
    public InputJudge InputJudge { get; set; }
    public float ActivationWindow = 0.4f;
    public event Action<NoteData, InputType, InputJudge.Judgement> OnJudgement;
    // Hold sustain: started when a hold note is hit (any judgement except Miss),
    // ended when the key is released (or on Seek). Optionally also cut at the note's EndTime.
    public event Action<NoteData> OnHoldStarted;
    public event Action<NoteData> OnHoldEnded;
    [Tooltip("If on, the hold sustain stops at the hold note's end even if the key is still down. Off = sound lasts exactly as long as the key is held.")]
    public bool EndSustainAtNoteEnd = false;

    private List<NoteData> Notes;
    private int NextActivateIndex;
    private List<ActiveNote> ActiveNotes = new List<ActiveNote>();
    private readonly Dictionary<string, (double early, double late)> NoteWindows = new Dictionary<string, (double early, double late)>();
    private double ExpiryWindow => InputJudge.WidestThreshold + 0.01;

    private struct ActiveNote
    {
        public NoteData Note;
        public bool Pressed;
    }

    // Hold notes whose key is still down. Kept separate from ActiveNotes so a sustain
    // can outlive the note's judgement window (the player keeps holding past EndTime).
    private readonly List<NoteData> Sustains = new List<NoteData>();
    
    public void SetNotes(List<NoteData> chartNotes)
    {
        Notes = chartNotes.FindAll(note => !note.IsCheckpoint);
        NextActivateIndex = 0;
        ActiveNotes.Clear();
        NoteWindows.Clear();

        for (int i = 0; i < Notes.Count; i++)
        {
            double early = ActivationWindow;
            double late = ExpiryWindow;

            for (int j = i - 1; j >= 0; j--)
                if (Notes[j].Input == Notes[i].Input)
                {
                    early = Math.Min(early, (Notes[i].Time - Notes[j].Time) / 2.0);
                    break;
                }

            for (int j = i + 1; j < Notes.Count; j++)
                if (Notes[j].Input == Notes[i].Input)
                {
                    late = Math.Min(late, (Notes[j].Time - Notes[i].Time) / 2.0);
                    break;
                }

            NoteWindows[Notes[i].Id] = (early, late);
        }
    }

    public void Seek(double songTime)
    {
        // Stop any hold sustain before dropping the notes, so the sound never keeps playing after a restore.
        for (int i = Sustains.Count - 1; i >= 0; i--)
            EndSustain(i);
        ActiveNotes.Clear();
        NextActivateIndex = Notes.FindIndex(note => note.Time >= songTime);
        if (NextActivateIndex < 0) NextActivateIndex = Notes.Count;
    }

    public void Tick(double receptor)
    {
        while (NextActivateIndex < Notes.Count && receptor >= Notes[NextActivateIndex].Time - NoteWindows[Notes[NextActivateIndex].Id].early)
        {
            Activate(Notes[NextActivateIndex]);
            NextActivateIndex++;
        }
        
        // Optional: cut the hold sustain at the note's EndTime even if the key is still down.
        if (EndSustainAtNoteEnd)
            for (int i = Sustains.Count - 1; i >= 0; i--)
                if (receptor >= Sustains[i].EndTime)
                    EndSustain(i);

        // expired note
        for (int i = ActiveNotes.Count - 1; i >= 0; i--)
        {
            ActiveNote active = ActiveNotes[i];
            double expireAt = (active.Note.IsHold && active.Pressed ? active.Note.EndTime : active.Note.Time)
                              + NoteWindows[active.Note.Id].late;            
            if (receptor < expireAt)
                continue;

            if (!active.Pressed)
                OnJudgement?.Invoke(active.Note, active.Note.Input, InputJudge.Judgement.Miss);
           
            ActiveNotes.RemoveAt(i);
        }
    }

    public void HandleInput(InputType pressed, double receptor)
    {
        for (int i = 0; i < ActiveNotes.Count; i++)
        {
            ActiveNote active = ActiveNotes[i];
            
            if (active.Pressed || active.Note.Input != pressed)
                continue;

            InputJudge.Judgement judgement = InputJudge.Evaluate(receptor, active.Note.Time);
            OnJudgement?.Invoke(active.Note, pressed, judgement);
            
            active.Pressed = true;
            ActiveNotes[i] = active;
            if (active.Note.IsHold && judgement != InputJudge.Judgement.Miss)
            {
                Sustains.Add(active.Note);
                OnHoldStarted?.Invoke(active.Note);
            }
            return;
        }
    }

    // Subscribed to InputHandler.OnInputReleased. Letting go of the key ends that input's sustain.
    public void HandleRelease(InputType released)
    {
        for (int i = Sustains.Count - 1; i >= 0; i--)
            if (Sustains[i].Input == released)
                EndSustain(i);
    }

    private void EndSustain(int index)
    {
        NoteData note = Sustains[index];
        Sustains.RemoveAt(index);
        OnHoldEnded?.Invoke(note);
    }

    private void Activate(NoteData note)
    {
        ActiveNotes.Add(new ActiveNote { Note = note});
    }
}