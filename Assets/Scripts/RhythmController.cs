using System;
using UnityEngine;

public class RhythmController : MonoBehaviour
{
    public ScheduledAudioPlayer ScheduledAudioPlayer;
    public ReceptorClock ReceptorClock;
    public InputJudge InputJudge;
    public BeatVisual BeatVisual;
    public ChartLoader ChartLoader;
    public InputHandler InputHandler;
    
    // store notes? 

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
        //eval input
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
