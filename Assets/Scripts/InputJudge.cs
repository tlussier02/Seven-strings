using System;
using UnityEngine;

public class InputJudge : MonoBehaviour
{
    public enum Judgement
    {
        Perfect,
        Great,
        Good,
        Ok,
        Miss
    }

    private static readonly (double threshold, Judgement judgement)[] JudgementThresholds =
    {
        (0.025, Judgement.Perfect),
        (0.05, Judgement.Great),
        (0.0875, Judgement.Good),
        (0.125, Judgement.Ok)
    };
    
    public Judgement Evaluate(InputType input, double receptor, NoteData note)
    {
        if (!note.InputTypes.Contains(input))
            return Judgement.Miss;
        
        double offset = Math.Min(receptor, note.Time);

        foreach (var (threshold, judgement) in JudgementThresholds)
        {
            if (offset <= threshold)
                return judgement;
        }

        return Judgement.Miss;
    }
}
