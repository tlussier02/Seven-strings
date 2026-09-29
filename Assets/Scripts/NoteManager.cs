using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public ReceptorClock ReceptorClock;
    public InputJudge InputJudge;
    public float MaxWindow = 0.125f; // shouldn't be manually changed. Just same value as Judgement.Ok
    public event Action<InputType, InputJudge.Judgement> OnJudgement;

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
        if (upcomingNote.HasValue && !IsActive(upcomingNote.Value) && receptor >= upcomingNote.Value.Time - MaxWindow)
        {
            Activate(upcomingNote.Value);
        }

        // expired note
        for (int i = ActiveNotes.Count - 1; i >= 0; i--)
        {
            if (receptor >= ActiveNotes[i].Note.Time + MaxWindow)
            {
                foreach (InputType missed in ActiveNotes[i].PendingInputs)
                    OnJudgement?.Invoke(missed, InputJudge.Judgement.Miss);

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
                OnJudgement?.Invoke(pressed, judgement);
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