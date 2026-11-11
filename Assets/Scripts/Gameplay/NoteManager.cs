using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public ReceptorClock ReceptorClock { get; set; }
    public InputJudge InputJudge { get; set; }
    public float ActivationWindow = 0.4f;
    public event Action<NoteData, InputType, InputJudge.Judgement> OnJudgement;

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
    
    public void SetNotes(List<NoteData> chartNotes)
    {
        Notes = chartNotes;
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

    public void Tick(double receptor)
    {
        while (NextActivateIndex < Notes.Count && receptor >= Notes[NextActivateIndex].Time - NoteWindows[Notes[NextActivateIndex].Id].early)
        {
            Activate(Notes[NextActivateIndex]);
            NextActivateIndex++;
        }
        
        // expired note
        for (int i = ActiveNotes.Count - 1; i >= 0; i--)
        {
            ActiveNote active = ActiveNotes[i];
            double expireAt = (active.Note.IsHold && active.Pressed ? active.Note.EndTime : active.Note.Time)
                              + NoteWindows[active.Note.Id].late;            
            if (receptor < expireAt)
                continue;

            // checkpoint
            if (active.Note.IsCheckpoint && active.Note.Time >= receptor)
            {
                
            }
            
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

    private void Activate(NoteData note)
    {
        ActiveNotes.Add(new ActiveNote { Note = note});
    }
}