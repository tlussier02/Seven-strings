using System.Collections.Generic;
using UnityEngine;

public class RhythmController : MonoBehaviour
{
    public ScheduledAudioPlayer ScheduledAudioPlayer;
    public ReceptorClock ReceptorClock;
    public InputJudge InputJudge;
    public BeatVisual BeatVisual;
    public ChartLoader ChartLoader;
    public InputHandler InputHandler;
    public SfxPlayer SfxPlayer;
    public NoteManager NoteManager;

    void Awake()
    {
        ReceptorClock.ScheduledAudioPlayer = ScheduledAudioPlayer;
        List<NoteData> notes = ChartLoader.LoadChart();
        ReceptorClock.SetNotes(notes);
        NoteManager.SetNotes(notes);
        NoteManager.ReceptorClock = ReceptorClock;
    }

    void OnEnable()
    {
        ReceptorClock.OnBeat += HandleBeat;
        InputHandler.OnInputPressed += HandleInput;
        InputHandler.OnInputPressed += SfxPlayer.PlayFor;
        NoteManager.OnJudgement += HandleJudgement;
    }

    void OnDisable()
    {
        ReceptorClock.OnBeat -= HandleBeat;
        InputHandler.OnInputPressed -= HandleInput;
        InputHandler.OnInputPressed -= SfxPlayer.PlayFor;
        NoteManager.OnJudgement -= HandleJudgement;
    }

    void Update()
    {
        ScheduledAudioPlayer.Tick();
        ReceptorClock.Tick();
        NoteManager.Tick(ReceptorClock.Receptor, ReceptorClock.NextNote);
        InputHandler.Tick();
        double preSyncTime = AudioSettings.dspTime - (ScheduledAudioPlayer.ScheduledStartTime + ReceptorClock.LatencyCompensation);
    }

    public void HandleBeat(NoteData note)
    {
        
    }

    public void HandleInput(InputType input)
    {
        NoteManager.HandleInput(input, ReceptorClock.Receptor);
    }

    public void HandleJudgement(NoteData note, InputType input, InputJudge.Judgement judgement)
    {
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
}