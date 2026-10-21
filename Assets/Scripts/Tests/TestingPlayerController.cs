using UnityEngine;

public class TestingPlayerController : MonoBehaviour
{
    public ReceptorClock ReceptorClock;
    public float PlayerSpeed;
    
    private Rigidbody Player;

    void Awake()
    {
        Player = GetComponent<Rigidbody>();

    }

    void Update()
    {
        // deltaTime can still contain the pre-pause value on the opening frame.
        if (Time.timeScale == 0f || AudioListener.pause)
            return;

        if (!ReceptorClock.FoundOvertime) 
            return;
        transform.position += transform.forward * PlayerSpeed * Time.deltaTime;
    }
}