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

    public Judgement Evaluate(double receptor, double nextBeatTime, double lastBeatTime)
    {
        double offsetToNext = Math.Abs(receptor - nextBeatTime);
        double offsetToLast = Math.Abs(receptor - lastBeatTime);
        double offset = Math.Min(offsetToNext, offsetToLast);

        foreach (var (threshold, judgement) in JudgementThresholds)
        {
            if (offset <= threshold)
                return judgement;
        }

        return Judgement.Miss;
    }
}
