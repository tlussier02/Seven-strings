using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public ReceptorClock ReceptorClock { get; set; }
    public InputJudge InputJudge { get; set; }
    public float ActivationWindow = 0.4f;
    
    public event Action<NoteData, InputType, InputJudge.Judgement> OnJudgement;
    
    // for input hold mechanic
    public event Action<NoteData, InputType, InputJudge.Judgement> OnReleaseJudgement;
    public event Action<NoteData> OnHoldStarted;
    public event Action<NoteData> OnHoldEnded;

    private List<NoteData> Notes;
    private int NextActivateIndex;
    private List<ActiveNote> ActiveNotes = new List<ActiveNote>();
    private readonly Dictionary<string, (double early, double late)> NoteWindows = new Dictionary<string, (double early, double late)>();
    private double ExpiryWindow => InputJudge.WidestThreshold + 0.01;
    // How long after a hold's EndTime a release can still be judged.
    // Clamped so it never reaches halfway to the next note of the same input.
    private readonly Dictionary<string, double> ReleaseLateWindows = new Dictionary<string, double>();

    private struct ActiveNote
    {
        public NoteData Note;
        public bool Pressed;
        
        // An input hold can be caught up until its release window opens
        public bool PressMissed;
    }

    private struct HeldNote
    {
        public NoteData Note;
        
        // True once the late "held too long" Miss has fired
        public bool ReleaseJudged;
    }

    // Hold notes whose key is still down are separate from ActiveNotes so the player can hold past EndTime
    private readonly List<HeldNote> Sustains = new List<HeldNote>();
    
    public void SetNotes(List<NoteData> chartNotes)
    {
        Notes = chartNotes.FindAll(note => !note.IsCheckpoint);
        NextActivateIndex = 0;
        ActiveNotes.Clear();
        NoteWindows.Clear();
        ReleaseLateWindows.Clear();

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

            // A release can't be judged past halfway to the next note of the same input
            if (Notes[i].IsHold)
            {
                double releaseLate = ExpiryWindow;
                for (int j = i + 1; j < Notes.Count; j++)
                    if (Notes[j].Input == Notes[i].Input)
                    {
                        releaseLate = Math.Min(releaseLate, (Notes[j].Time - Notes[i].EndTime) / 2.0);
                        break;
                    }
                ReleaseLateWindows[Notes[i].Id] = Math.Max(0.0, releaseLate);
            }
        }
    }

    public void Seek(double songTime)
    {
        // stop any hold sustain before dropping the notes, so the sound never keeps playing after a restore
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
        
        
        for (int i = 0; i < Sustains.Count; i++)
        {
            HeldNote held = Sustains[i];
            if (held.ReleaseJudged || receptor < held.Note.EndTime + ReleaseLateWindows[held.Note.Id])
                continue;

            held.ReleaseJudged = true;
            Sustains[i] = held;
            OnReleaseJudgement?.Invoke(held.Note, held.Note.Input, InputJudge.Judgement.Miss);
        }

        // expired note
        for (int i = ActiveNotes.Count - 1; i >= 0; i--)
        {
            ActiveNote active = ActiveNotes[i];
            if (receptor < active.Note.Time + NoteWindows[active.Note.Id].late)
                continue;

            if (!active.Pressed && !active.PressMissed)
            {
                OnJudgement?.Invoke(active.Note, active.Note.Input, InputJudge.Judgement.Miss);
                if (active.Note.IsHold)
                {
                    active.PressMissed = true;
                    ActiveNotes[i] = active;
                }
            }
            
            // a late press hold can still start the sound and earn a release judgement
            if (active.Note.IsHold && !active.Pressed && receptor < CatchDeadline(active.Note))
                continue;

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

            // A late catch already got its press Miss from Tick, so it isn't judged again
            if (!active.PressMissed)
            {
                InputJudge.Judgement judgement = InputJudge.Evaluate(receptor, active.Note.Time);
                OnJudgement?.Invoke(active.Note, pressed, judgement);
            }
            
            active.Pressed = true;
            ActiveNotes[i] = active;

            // Every hold press starts the sustain, even a Miss; the release is judged separately
            if (active.Note.IsHold)
            {
                Sustains.Add(new HeldNote { Note = active.Note });
                OnHoldStarted?.Invoke(active.Note);
            }
            return;
        }
    }
    
    
    public void HandleRelease(InputType released, double receptor)
    {
        for (int i = Sustains.Count - 1; i >= 0; i--)
        {
            HeldNote held = Sustains[i];
            if (held.Note.Input != released)
                continue;

            if (!held.ReleaseJudged)
            {
                InputJudge.Judgement judgement = InputJudge.Evaluate(receptor, held.Note.EndTime);
                OnReleaseJudgement?.Invoke(held.Note, released, judgement);
            }
            EndSustain(i);
        }
    }

    private void EndSustain(int index)
    {
        NoteData note = Sustains[index].Note;
        Sustains.RemoveAt(index);
        OnHoldEnded?.Invoke(note);
    }

    // Last moment a missed hold can still be caught
    private double CatchDeadline(NoteData note) =>
        Math.Max(note.Time + NoteWindows[note.Id].late, note.EndTime - InputJudge.WidestThreshold);

    private void Activate(NoteData note)
    {
        ActiveNotes.Add(new ActiveNote { Note = note});
    }
}