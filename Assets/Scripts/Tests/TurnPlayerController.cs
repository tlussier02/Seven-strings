using UnityEngine;
using DG.Tweening;

public class TurnPlayerController : MonoBehaviour
{
    public TestingInputHandler TestingInputHandler;
    public Transform PathSegment;
    public float PlayerSpeed;
    public float TurnDuration = 0.15f;
    
    private float LaneWidth;
    private float CenterX;
    private float TargetYaw;
    private Tween TurnTween;
    
    void Awake()
    {
        CenterX = transform.position.x;
        LaneWidth = PathSegment.localScale.x;
        TargetYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        transform.position += transform.forward * PlayerSpeed * Time.deltaTime;
    }

    void OnDestroy()
    {
        TurnTween?.Kill();
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
        TargetYaw += 90f * direction;
        TurnTween?.Kill();
        TurnTween = transform.DORotate(new Vector3(0f, TargetYaw, 0f), TurnDuration).SetEase(Ease.OutQuad);
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