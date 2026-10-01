using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public ReceptorClock ReceptorClock;
    public InputJudge InputJudge;
    public float ActivationWindow = 0.4f;
    public event Action<NoteData, InputType, InputJudge.Judgement> OnJudgement;

    private double ExpiryWindow => InputJudge.WidestThreshold + 0.01;
    private List<NoteData> Notes;
    private List<ActiveNote> ActiveNotes = new List<ActiveNote>();

    private struct ActiveNote
    {
        public NoteData Note;
        public List<InputType> PendingInputs;
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
            if (receptor >= ActiveNotes[i].Note.Time + ExpiryWindow)
            {
                foreach (InputType missed in ActiveNotes[i].PendingInputs)
                    OnJudgement?.Invoke(ActiveNotes[i].Note, missed, InputJudge.Judgement.Miss);

                ActiveNotes.RemoveAt(i);
            }
        }
    }

    public void HandleInput(InputType pressed, double receptor)
    {
        foreach (ActiveNote active in ActiveNotes)
        {
            if (active.PendingInputs.Contains(pressed))
            {
                InputJudge.Judgement judgement = InputJudge.Evaluate(receptor, active.Note.Time);
                OnJudgement?.Invoke(active.Note, pressed, judgement);
                active.PendingInputs.Remove(pressed);
                return;
            }
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
        ActiveNotes.Add(new ActiveNote
        {
            Note = note,
            PendingInputs = new List<InputType>(note.InputTypes)
        });
    }
}