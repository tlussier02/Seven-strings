using System;
using UnityEngine;

public class HarmonyMeter : MonoBehaviour
{
    [Header("Range")]
    [SerializeField, Min(1)] private int maxHarmony = 100;
    [SerializeField, Min(0)] private int startingHarmony = 10;

    [Header("Points per judgement")]
    [SerializeField] private int perfectPoints = 20;
    [SerializeField] private int greatPoints = 15;
    [SerializeField] private int goodPoints = 10;
    [SerializeField] private int okPoints = 5;
    [SerializeField] private int missPoints = -10;

    public int Current { get; private set; }
    public int Max => maxHarmony;
    public float Normalized => (float)Current / maxHarmony;
    
    public event Action<int, int> OnHarmonyChanged;
    public event Action OnHarmonyEmpty;  // hook for a fail state later
    public event Action OnHarmonyFull;   // hook for a "full harmony" bonus later

    void Awake()
    {
        ResetMeter();
    }

    public void ResetMeter()
    {
        Current = Mathf.Clamp(startingHarmony, 0, maxHarmony);
        OnHarmonyChanged?.Invoke(Current, maxHarmony);
    }

    // Signature matches NoteManager.OnJudgement, so it can subscribe directly.
    public void HandleJudgement(NoteData note, InputType input, InputJudge.Judgement judgement)
    {
        Apply(PointsFor(judgement));
    }

    public int PointsFor(InputJudge.Judgement judgement) => judgement switch
    {
        InputJudge.Judgement.Perfect => perfectPoints,
        InputJudge.Judgement.Great   => greatPoints,
        InputJudge.Judgement.Good    => goodPoints,
        InputJudge.Judgement.Ok      => okPoints,
        _                            => missPoints
    };

    private void Apply(int delta)
    {
        int previous = Current;
        Current = Mathf.Clamp(Current + delta, 0, maxHarmony);

        if (Current == previous)
            return; // already pinned at 0 or max, nothing changed

        OnHarmonyChanged?.Invoke(Current, maxHarmony);

        if (Current == 0)
            OnHarmonyEmpty?.Invoke();
        else if (Current == maxHarmony)
            OnHarmonyFull?.Invoke();
    }

    void OnValidate()
    {
        startingHarmony = Mathf.Clamp(startingHarmony, 0, maxHarmony);
    }
}
