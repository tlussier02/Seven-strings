using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public ReceptorClock ReceptorClock { get; set; }
    public InputJudge InputJudge { get; set; }
    public float ActivationWindow = 0.4f;
    public event Action<NoteData, InputType, InputJudge.Judgement> OnJudgement;

    private double ExpiryWindow => InputJudge.WidestThreshold + 0.01;
    private List<NoteData> Notes;
    private List<ActiveNote> ActiveNotes = new List<ActiveNote>();

    private struct ActiveNote
    {
        public NoteData Note;
        public bool Pressed;
    }

    public void SetNotes(List<NoteData> chartNotes)
    {
        Notes = chartNotes;
        ActiveNotes.Clear();
    }

    public void Tick(double receptor, NoteData? upcomingNote)
    {
        if (upcomingNote.HasValue && !IsActive(upcomingNote.Value) && receptor >= upcomingNote.Value.Time - ActivationWindow)
        {
            Activate(upcomingNote.Value);
        }

        // expired note
        for (int i = ActiveNotes.Count - 1; i >= 0; i--)
        {
            ActiveNote active = ActiveNotes[i];
            double expireAt = (active.Note.IsHold && active.Pressed ? active.Note.EndTime : active.Note.Time) + ExpiryWindow;
            
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
            return;
        }
    }

    private bool IsActive(NoteData note)
    {
        foreach (ActiveNote active in ActiveNotes)
        {
            if (active.Note.Time == note.Time)
                return true;
        }
        return false;
    }

    private void Activate(NoteData note)
    {
        ActiveNotes.Add(new ActiveNote { Note = note});
    }
}