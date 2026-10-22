using UnityEngine;

public class TurnPlayerController : MonoBehaviour
{
    public TestingInputHandler TestingInputHandler;
    public Transform PathSegment;
    public float PlayerSpeed;
    public float LaneChangeDuration = 0.15f;
    
    private int CurrentLane = 1;
    private float LaneWidth;
    private float CenterX;
    
    void Awake()
    {
        CenterX =  transform.position.x;
        LaneWidth = PathSegment.localScale.x;
    }

    void Update()
    {
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
                TurnRight();
                break;
            case InputType.Left:
                TurnLeft();
                break;
        }
    }

    private void Turn(int direction)
    {
        
    }

    private void TurnRight()
    {
        Turn(1);
    }
    
    private void TurnLeft()
    {
        Turn(-1);
    }
}