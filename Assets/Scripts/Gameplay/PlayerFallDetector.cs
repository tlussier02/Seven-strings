using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(GameplayReset))]
public class PlayerFallDetector : MonoBehaviour
{
    [SerializeField] private float fallThresholdY = -5f;

    private bool resetTriggered;

    void LateUpdate()
    {
        // Leave a paused fall pending until gameplay resumes.
        if (Time.timeScale == 0f)
            return;

        if (transform.position.y >= fallThresholdY || resetTriggered)
            return;

        // Latch until scene unload; a fall can request only one restart.
        resetTriggered = true;
        GameplayReset.RequestRestart();
    }
}
