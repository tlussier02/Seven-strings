using System.Collections;
using UnityEngine;
using DG.Tweening;

public class TestingPlayerController : MonoBehaviour
{
    public TestingInputHandler TestingInputHandler;
    public Transform PathSegment;
    public float PlayerSpeed;
    public float LaneChangeDuration = 0.15f;
    
    private int CurrentLane = 1;
    private float LaneWidth;
    private float CenterX;
    private Tween LaneTween;
    
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
                MoveRight();
                break;
            case InputType.Left:
                MoveLeft();
                break;
        }
    }

    private void ChangeLane(int direction)
    {
        int target = Mathf.Clamp(CurrentLane + direction, 0, 2);
        if (target == CurrentLane) 
            return;
        CurrentLane = target;

        float targetX = CenterX + (CurrentLane - 1) * (LaneWidth / 4);
        LaneTween?.Kill();
        LaneTween = transform.DOMoveX(targetX, LaneChangeDuration).SetEase(Ease.OutQuad);
    }

    private void MoveRight()
    {
        ChangeLane(1);
    }
    
    private void MoveLeft()
    {
        ChangeLane(-1);
    }
}