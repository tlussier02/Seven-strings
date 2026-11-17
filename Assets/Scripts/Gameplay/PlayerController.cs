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
    
    [SerializeField] private float fallbackLaneWidth = 10f;
    [SerializeField] private float fallbackJumpDuration = 0.5f;
    private int CurrentLane = 1;
    private float LaneWidth;
    private float CenterX;
    private float GroundY;
    private Transform LaneSegment;
    private Tween LaneTween;
    private Tween JumpTween;
    private InputHandler subscribedInput;
    
    void Awake()
    {
        LevelTools = FindAnyObjectByType<LevelTools>();

        if (InputHandler == null)
            InputHandler = ReceptorClock != null
                ? ReceptorClock.GetComponent<InputHandler>()
                : FindFirstObjectByType<InputHandler>();

        if (ReceptorClock == null && InputHandler != null)
            ReceptorClock = InputHandler.GetComponent<ReceptorClock>();

        LaneSegment = LevelTools != null && LevelTools.LaneSegmentPrefab != null
            ? LevelTools.LaneSegmentPrefab.transform
            : null;

        CenterX = transform.position.x;

        LaneWidth = LaneSegment != null
            ? LaneSegment.localScale.x
            : fallbackLaneWidth;

        GroundY = transform.position.y;
    }

    void Update()
    {
        if (Time.timeScale == 0f || AudioListener.pause)
            return;
        if (ReceptorClock == null || !ReceptorClock.FoundOvertime)
            return;
        transform.position += transform.forward * PlayerSpeed * Time.deltaTime;
    }

    void OnEnable()
    {
        if (InputHandler == null)
        {
            Debug.LogError("PlayerController requires the gameplay InputHandler.", this);
            return;
        }
        subscribedInput = InputHandler;
        subscribedInput.OnInputPressed += HandleInput;
    }

    void OnDisable()
    {
        if (subscribedInput != null)
            subscribedInput.OnInputPressed -= HandleInput;
        subscribedInput = null;
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

    void OnDestroy()
    {
        LaneTween?.Kill();
        JumpTween?.Kill();
    }

    public void RestoreCheckpoint(Transform spawn, int lane)
    {
        LaneTween?.Kill();
        JumpTween?.Kill();
        CurrentLane = lane;
        CenterX = spawn.position.x - (lane - 1) * LaneWidth;
        GroundY = spawn.position.y;
        transform.SetPositionAndRotation(spawn.position, spawn.rotation);
        if (TryGetComponent<Rigidbody>(out var body))
        {
            body.position = spawn.position;
            body.rotation = spawn.rotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        Physics.SyncTransforms();
    }

    private void Jump()
    {
        if (JumpTween != null && JumpTween.IsActive() && JumpTween.IsPlaying())
            return;
        
        float jumpDuration = LevelTools != null && LevelTools.ChartLoader != null
            ? LevelTools.JumpLengthBeats * (60f / LevelTools.ChartLoader.Bpm) : fallbackJumpDuration;

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
