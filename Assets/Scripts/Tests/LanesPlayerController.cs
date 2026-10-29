using UnityEngine;
using DG.Tweening;

public class LanesPlayerController : MonoBehaviour
{
    public InputHandler InputHandler;
    public ReceptorClock ReceptorClock;
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
        if (Time.timeScale == 0f || AudioListener.pause)
            return;
        if (!ReceptorClock.FoundOvertime) 
            return;
        transform.position += transform.forward * PlayerSpeed * Time.deltaTime;
    }

    void OnEnable()
    {
        InputHandler.OnInputPressed += HandleInput;
    }

    void OnDisable()
    {
        InputHandler.OnInputPressed -= HandleInput;
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
        int target = Mathf.Clamp(CurrentLane + direction, 0, 4); // increased lanes from 3 to 5 so player can "fall off" of track
        if (target == CurrentLane) 
            return;
        CurrentLane = target;

        float targetX = CenterX + (CurrentLane - 1) * LaneWidth;
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