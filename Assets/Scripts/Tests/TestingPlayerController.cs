using System.Collections;
using UnityEngine;
using DG.Tweening;

public class TestingPlayerController : MonoBehaviour
{
    public ReceptorClock ReceptorClock;
    public TestingInputHandler TestingInputHandler;
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

    void OnEnable()
    {
        TestingInputHandler.OnInputPressed += HandleInput;
    }

    void OnDisable()
    {
        TestingInputHandler.OnInputPressed -= HandleInput;
    }

    private void HandleInput(InputType inputType)
    {
        switch (inputType)
        {
            case InputType.Right:
                MoveRight();
                break;
            case InputType.Left:
                MoveLeft();
                break;
        }
    }

    private void MoveRight()
    {
        
    }
    
    private void MoveLeft()
    {
        
    }
}