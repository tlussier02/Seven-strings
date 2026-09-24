using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DSPReceptorSync : MonoBehaviour
{
    public AudioClip clip;
    public float bpm = 120.0f;
    public int numBeatsPerSegment = 32;
    public int totalLoops = 1;
    public float beatMargin = .20f;

    private AudioSource audioSource;
    private double nextEventTime;
    private double nextBeatTime;
    private double receptor;
    private bool running = false;
    private bool foundOvertime = false;
    private int loopCount = 0;
    private int beatCount = 0;

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
        double overtime = time - nextEventTime;
        receptor += Time.deltaTime;

        // finding frame overshoot from scheduled time
        if (!foundOvertime && time >= nextEventTime)
        {
            print("overtime: " + overtime + "ms");
            receptor -= overtime;
            foundOvertime = true;
            nextBeatTime = receptor + (1 / (bpm / 60));
        }

        // 1 second before time scheduled to play audio
        if (time + 1.0f > nextEventTime && loopCount < totalLoops)
        {
            audioSource.clip = clip;
            audioSource.PlayScheduled(nextEventTime);

            Debug.Log("Scheduled audio to start at time " + nextEventTime);

            // Place the next event 32 beats from here at a rate of 120 beats per minute
            // calling PlayScheduled on audio that is still running terminates it 
            // would need a second audio clip and source to be able to seamlessly loop the audio
            //nextEventTime += 60.0f / bpm * numBeatsPerSegment;
            loopCount++;
        }
        
        // print on every beat
        if (receptor >= nextBeatTime)
        {
            print("mismatch: " + (receptor - nextBeatTime));
            nextBeatTime += 1 / (bpm / 60); 
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (receptor >= nextBeatTime - (beatMargin / 2) && receptor < nextBeatTime + (beatMargin / 2))
            {
                print("input received " + (receptor - nextBeatTime));
            } else 
                print("miss");
        }
    }
}
