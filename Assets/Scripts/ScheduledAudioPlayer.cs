using UnityEngine;

public class ScheduledAudioPlayer : MonoBehaviour
{
    public AudioSource AudioSource;
    public AudioClip AudioClip;
    public double ScheduledStartTime { get; private set; }
    public bool IsScheduled { get; private set; }
    

    void Start()
    {
        ScheduledStartTime = AudioSettings.dspTime + 2f;
        IsScheduled = true;
        
        GameObject child = new GameObject("AudioPlayer");
        child.transform.parent = gameObject.transform;
        AudioSource = child.AddComponent<AudioSource>();
    }

    public void Tick()
    {
        double time = AudioSettings.dspTime;
        if (time + 1.0f > ScheduledStartTime)
        {
            AudioSource.clip = AudioClip;
            AudioSource.PlayScheduled(ScheduledStartTime);

            Debug.Log("Scheduled audio to start at dsp time " + ScheduledStartTime);
        }
    }
}
