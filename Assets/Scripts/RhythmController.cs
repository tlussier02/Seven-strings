using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RhythmController : MonoBehaviour
{
    public ScheduledAudioPlayer ScheduledAudioPlayer;
    public ReceptorClock ReceptorClock;
    public InputJudge InputJudge;
    public BeatVisual BeatVisual;
    public ChartLoader ChartLoader;
    public InputHandler InputHandler;
    

    void Awake()
    {
        ReceptorClock.ScheduledAudioPlayer = ScheduledAudioPlayer;
        ReceptorClock.SetNotes(ChartLoader.LoadChart());
        ReceptorClock.OnBeat += HandleBeat;
        InputHandler.OnInputPressed += HandleInput;
    }
    void Update()
    {
        ScheduledAudioPlayer.Tick();
        ReceptorClock.Tick();
        InputHandler.Tick();
    }

    public void HandleBeat(NoteData note)
    {
        BeatVisual.FlashBeat();
    }

    public void HandleInput(InputType input)
    {
        BeatVisual.FlashInput();
        
        InputJudge.Judgement judgement = InputJudge.Evaluate(input, ReceptorClock.Receptor, ReceptorClock.GetNextNote());
        switch (judgement)
        {
            case InputJudge.Judgement.Perfect:
                BeatVisual.DisplayAccuracy("Perfect", Color.dodgerBlue);
                break;
            case InputJudge.Judgement.Great:
                BeatVisual.DisplayAccuracy("Great", Color.forestGreen);
                break;
            case InputJudge.Judgement.Good:
                BeatVisual.DisplayAccuracy("Good", Color.darkGreen);
                break;
            case InputJudge.Judgement.Ok:
                BeatVisual.DisplayAccuracy("Ok", Color.softYellow);
                break;
            default:
                BeatVisual.DisplayAccuracy("Miss", Color.softRed);
                break;
        }
    }

    void OnDestroy()
    {
        ReceptorClock.OnBeat -= HandleBeat;
        InputHandler.OnInputPressed -= HandleInput;
    }

    void OnEnable()
    {
        ReceptorClock.OnBeat += HandleBeat;
        InputHandler.OnInputPressed += HandleInput;
    }

    void OnDisable()
    {
        ReceptorClock.OnBeat -= HandleBeat;
        InputHandler.OnInputPressed -= HandleInput;
    }
}
