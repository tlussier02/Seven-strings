using System.Collections.Generic;
using UnityEngine;

public class RhythmController : MonoBehaviour
{
    private ScheduledAudioPlayer ScheduledAudioPlayer;
    private ReceptorClock ReceptorClock;
    private InputJudge InputJudge;
    private BeatVisual BeatVisual;
    private ChartLoader ChartLoader;
    private InputHandler InputHandler;
    private SfxPlayer SfxPlayer;
    private NoteManager NoteManager;
    // private AnimationManager AnimationManager;
    private HarmonyMeter HarmonyMeter;

    void Awake()
    {
        ScheduledAudioPlayer = GetComponent<ScheduledAudioPlayer>();
        ReceptorClock = GetComponent<ReceptorClock>();
        InputJudge = GetComponent<InputJudge>();
        BeatVisual = GetComponent<BeatVisual>();
        ChartLoader = GetComponent<ChartLoader>();
        InputHandler = GetComponent<InputHandler>();
        SfxPlayer = GetComponent<SfxPlayer>();
        NoteManager = GetComponent<NoteManager>();
        HarmonyMeter = GetComponent<HarmonyMeter>();
        
        ReceptorClock.ScheduledAudioPlayer = ScheduledAudioPlayer;
        NoteManager.ReceptorClock = ReceptorClock;
        NoteManager.InputJudge = InputJudge;
        
        List<NoteData> notes = ChartLoader.LoadChart();
        ReceptorClock.SetNotes(notes);
        NoteManager.SetNotes(notes);
        // AnimationManager.SetNotes(notes);
    }

    void OnEnable()
    {
        ReceptorClock.OnBeat += HandleBeat;
        InputHandler.OnInputPressed += HandleInput;
        InputHandler.OnInputPressed += SfxPlayer.PlayFor;
        NoteManager.OnJudgement += HandleJudgement;
        // NoteManager.OnJudgement += AnimationManager.HandleJudgement;
        NoteManager.OnJudgement += HarmonyMeter.HandleJudgement;
        NoteManager.OnJudgement += SfxPlayer.HandleJudgement;
    }

    void OnDisable()
    {
        ReceptorClock.OnBeat -= HandleBeat;
        InputHandler.OnInputPressed -= HandleInput;
        InputHandler.OnInputPressed -= SfxPlayer.PlayFor;
        NoteManager.OnJudgement -= HandleJudgement;
        // NoteManager.OnJudgement -= AnimationManager.HandleJudgement;
        NoteManager.OnJudgement -= HarmonyMeter.HandleJudgement;
        NoteManager.OnJudgement -= SfxPlayer.HandleJudgement;
    }

    void Update()
    {
        // Update still runs at timeScale zero. Freeze chart processing as well as input.
        if (Time.timeScale == 0f || AudioListener.pause)
            return;

        ScheduledAudioPlayer.Tick();
        ReceptorClock.Tick();
        // Before sync, Receptor contains accumulated lead-in time, not song time.
        // Do not activate/judge notes or consume input until the song clock is valid.
        if (!ReceptorClock.FoundOvertime)
            return;

        NoteManager.Tick(ReceptorClock.Receptor, ReceptorClock.NextNote);
        InputHandler.Tick();
        // double preSyncTime = AudioSettings.dspTime - (ScheduledAudioPlayer.ScheduledStartTime + ReceptorClock.LatencyCompensation);
        // AnimationManager.Tick(ReceptorClock.Receptor, ReceptorClock.FoundOvertime, preSyncTime);
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
