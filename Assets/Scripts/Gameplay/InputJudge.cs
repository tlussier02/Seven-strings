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

    public Judgement Evaluate(double receptor, double noteTime)
    {
        double offset = Math.Abs(receptor - noteTime);
        
        Debug.Log($"offset: {offset}");

        foreach ((double threshold, Judgement judgement) in JudgementThresholds)
        {
            if (offset <= threshold)
                return judgement;
        }

        return Judgement.Miss;
    }
}