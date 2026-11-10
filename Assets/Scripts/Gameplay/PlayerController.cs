using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    public InputHandler InputHandler;
    public ReceptorClock ReceptorClock;
    public LevelTools LevelTools { get; private set; }
    public float PlayerSpeed;
    public float LaneChangeDuration = 0.15f;
    public float JumpHeight = 2f;
    
    private Transform LaneSegment;
    private int CurrentLane = 1;
    private float LaneWidth;
    private float CenterX;
    private float GroundY;
    private Tween LaneTween;
    private Tween JumpTween;
    
    void Awake()
    {
        LevelTools = FindObjectOfType<LevelTools>();
        LaneSegment = LevelTools.LaneSegmentPrefab.transform;
        CenterX =  transform.position.x;
        LaneWidth = LaneSegment.localScale.x;
        GroundY = transform.position.y;
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
            case InputType.Jump:
                Jump();
                break;
            case InputType.Roll:
                Roll();
                break;
        }
    }

    private void Jump()
    {
        if (JumpTween != null && JumpTween.IsActive() && JumpTween.IsPlaying())
            return;
        
        float jumpDuration = LevelTools.JumpLengthBeats * (60f / LevelTools.ChartLoader.Bpm);

        JumpTween?.Kill();
        transform.position = new Vector3(transform.position.x, GroundY, transform.position.z);
        JumpTween = transform.DOMoveY(GroundY + JumpHeight, jumpDuration / 2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
    }

    private void Roll()
    {
        
    }

    private void ChangeLane(int direction)
    {
        if (LaneTween != null && LaneTween.IsActive() && LaneTween.IsPlaying())
            return;
        
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