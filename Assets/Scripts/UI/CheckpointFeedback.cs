using UnityEngine;
using UnityEngine.UI;

// Presentation only: replace the label or wire final effects to OnCheckpointActivated.
public class CheckpointFeedback : MonoBehaviour
{
    [SerializeField] private CheckpointManager checkpoint;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private float duration = 3f;
    private GameObject ui;
    private float remaining;

    void Start()
    {
        checkpoint.OnCheckpointActivated.AddListener(Show);
        ui = new GameObject("Checkpoint Feedback", typeof(Canvas), typeof(CanvasScaler));
        var canvas = ui.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 75;
        var scaler = ui.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 540);
        var label = new GameObject("Checkpoint Activated", typeof(RectTransform), typeof(Text), typeof(Shadow));
        var rect = label.GetComponent<RectTransform>();
        rect.SetParent(ui.transform, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -85);
        rect.sizeDelta = new Vector2(440, 44);
        var text = label.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = "CHECKPOINT ACTIVATED";
        text.fontSize = 26;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.4f, 1f, 0.75f);
        text.raycastTarget = false;
        ui.SetActive(false);
    }

    private void Show()
    {
        remaining = duration;
        ui.SetActive(true);
        GetComponent<SfxPlayer>()?.PlayEffect(activationSound);
    }

    void Update()
    {
        if (remaining <= 0) return;
        remaining -= Time.deltaTime;
        if (remaining <= 0) ui.SetActive(false);
    }

    void OnDestroy()
    {
        if (checkpoint != null) checkpoint.OnCheckpointActivated.RemoveListener(Show);
        if (ui != null) Destroy(ui);
    }
}
