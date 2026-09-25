using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Text.RegularExpressions;

[RequireComponent(typeof(AudioSource))]
public class DSPReceptorSync : MonoBehaviour
{
    public AudioClip clip;
    public BeatVisual beatVisual;
    public float bpm = 120.0f;
    public int numBeatsPerSegment = 64;
    public int totalLoops = 1;
    public float latencyCompensation = 0.19f;

    private AudioSource audioSource;
    private double nextEventTime;
    private double lastBeatTime;
    private double nextBeatTime;
    private double receptor;
    private bool running = false;
    private bool foundOvertime = false;
    private int loopCount = 0;
    private double beatInterval => 60.0 / bpm;

    void Start()
    {
        GameObject child = new GameObject("Player");
        child.transform.parent = gameObject.transform;
        audioSource = child.AddComponent<AudioSource>();
        
        // audio scheduled to play 2 seconds after game start
        nextEventTime = AudioSettings.dspTime + 2.0f;
        running = true;
    }

    void Update()
    {
        if (!running)
            return;

        double time = AudioSettings.dspTime;
        double overtime = time - (nextEventTime + latencyCompensation);
        receptor += Time.deltaTime;

        // finding and adjusting for frame overshoot from scheduled playback
        if (!foundOvertime && time >= nextEventTime + latencyCompensation)
        {
            print("overtime: " + overtime + "ms");
            receptor -= overtime;
            foundOvertime = true;
            lastBeatTime = receptor;
            nextBeatTime = receptor + beatInterval;
            beatVisual.FlashBeat();
        }

        // 1 second before time scheduled to play audio
        if (time + 1.0f > nextEventTime && loopCount < totalLoops)
        {
            audioSource.clip = clip;
            audioSource.PlayScheduled(nextEventTime);

            Debug.Log("Scheduled audio to start at dsp time " + nextEventTime);

            // Place the next event 32 beats from here at a rate of 120 beats per minute
            // calling PlayScheduled on audio that is still running terminates it 
            // would need a second audio clip and source to be able to seamlessly loop the audio
            //nextEventTime += 60.0f / bpm * numBeatsPerSegment;
            loopCount++;
        }
        
        if (foundOvertime && receptor >= nextBeatTime)
        {
            beatVisual.FlashBeat();
            lastBeatTime = nextBeatTime;
            nextBeatTime += beatInterval;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            double offsetToNext = Math.Abs(receptor - nextBeatTime);
            double offsetToLast = Math.Abs(receptor - lastBeatTime);
            double timingOffset = Math.Min(offsetToNext, offsetToLast);

            if (timingOffset <= 0.025)
                beatVisual.DisplayAccuracy("Perfect", Color.dodgerBlue);
            else if (timingOffset <= 0.050)
                beatVisual.DisplayAccuracy("Great", Color.forestGreen);
            else if (timingOffset <= 0.0875)
                beatVisual.DisplayAccuracy("Good", Color.darkGreen);
            else if (timingOffset <= 0.125)
                beatVisual.DisplayAccuracy("Ok", Color.softYellow);
            else
                beatVisual.DisplayAccuracy("Miss", Color.red);

            beatVisual.FlashInput();
            
            print(timingOffset);
        }
    }
}
