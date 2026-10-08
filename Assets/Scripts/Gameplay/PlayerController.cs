using UnityEngine;

public class PlayerController : MonoBehaviour
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
        if (!ReceptorClock.FoundOvertime) 
            return;
        transform.position += transform.forward * PlayerSpeed * Time.deltaTime;
    }
}
