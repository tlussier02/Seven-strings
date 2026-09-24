using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DSPReceptorSync : MonoBehaviour
{
    public AudioClip clip;
    public float bpm = 120.0f;
    public int numBeatsPerSegment = 32;
    public int totalLoops = 1;

    private AudioSource audioSource;
    private double nextEventTime;
    private bool running = false;
    private bool foundOvertime = false;
    private int loopCount = 0;

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
        
        // finding frame overshoot from scheduled time
        if (!foundOvertime && time >= nextEventTime)
        {
            print("overtime: " + overtime + "ms");
            foundOvertime = true;
        }
        
        // 1 second before time scheduled to play audio
        if (time + 1.0f > nextEventTime &&  loopCount < totalLoops)
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
    }
}
