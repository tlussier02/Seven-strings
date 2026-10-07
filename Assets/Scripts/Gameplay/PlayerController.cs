using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody Player;
    public float PlayerSpeed;

    void Awake()
    {
        Player = GetComponent<Rigidbody>();
        
    }

    void Update()
    {
        transform.position += transform.forward * PlayerSpeed * Time.deltaTime;
    }
}
