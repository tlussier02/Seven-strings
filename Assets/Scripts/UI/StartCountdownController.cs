using System;
using UnityEngine;
using UnityEngine.UI;

// Settings runs first, then countdown/input gating, then the existing rhythm tick.
[DefaultExecutionOrder(-900)]
public class StartCountdownController : MonoBehaviour
{
    [SerializeField] private ScheduledAudioPlayer scheduledAudioPlayer;
    [SerializeField] private InputHandler gameplayInput;

    private GameObject uiRoot;
    private Text countdownText;
    private Text controlsText;
    private double leadInDuration;
    private bool finished;

    void Awake()
    {
        if (scheduledAudioPlayer == null || gameplayInput == null)
        {
            Debug.LogError("Start countdown requires a scheduled audio player and gameplay input.", this);
            enabled = false;
            return;
        }

        BuildUI();
    }

    void OnEnable()
    {
        if (uiRoot != null)
            uiRoot.SetActive(!finished);
        if (gameplayInput != null)
            gameplayInput.SetGameplayInputBlocked(!finished);
    }

    void Update()
    {
        if (finished || !scheduledAudioPlayer.IsScheduled)
            return;

        controlsText.text = "Controls\n\n" + InputHandler.ControlInstructions;

        double remainingTime = scheduledAudioPlayer.ScheduledStartTime - AudioSettings.dspTime;
        if (remainingTime <= 0d)
        {
            finished = true;
            uiRoot.SetActive(false);
            ReleaseInput();
            return;
        }

        // Observe the existing lead-in; never extend or reschedule it for the UI.
        if (leadInDuration <= 0d)
            leadInDuration = remainingTime;

        int number = Mathf.Clamp((int)Math.Ceiling(3d * remainingTime / leadInDuration), 1, 3);
        countdownText.text = number.ToString();
    }

    void OnDisable()
    {
        ReleaseInput();
        if (uiRoot != null)
            uiRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (uiRoot != null)
            Destroy(uiRoot);
    }

    private void ReleaseInput()
    {
        if (gameplayInput == null)
            return;

        gameplayInput.SetGameplayInputBlocked(false);
        gameplayInput.SuppressCurrentFrame();
    }

    private void BuildUI()
    {
        // No GraphicRaycaster: the visual countdown cannot intercept menu clicks.
        uiRoot = new GameObject("Start Countdown UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = uiRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // Below the Settings canvas (100), above gameplay HUD.
        CanvasScaler scaler = uiRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 540f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        GameObject label = new GameObject("Countdown", typeof(RectTransform), typeof(Text), typeof(Shadow));
        label.layer = 5;
        RectTransform rect = (RectTransform)label.transform;
        rect.SetParent(uiRoot.transform, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400f, 240f);
        rect.anchoredPosition = Vector2.zero;

        countdownText = label.GetComponent<Text>();
        countdownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countdownText.fontSize = 144;
        countdownText.fontStyle = FontStyle.Bold;
        countdownText.alignment = TextAnchor.MiddleCenter;
        countdownText.color = Color.white;
        countdownText.raycastTarget = false;
        countdownText.text = "3";
        GameObject panel = new GameObject("Countdown Controls", typeof(RectTransform), typeof(Image));
        RectTransform panelRect = (RectTransform)panel.transform;
        panelRect.SetParent(uiRoot.transform, false);
        panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0f, 0.5f);
        panelRect.anchoredPosition = new Vector2(20f, 0f);
        panelRect.sizeDelta = new Vector2(250f, 230f);
        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.1f, 0.85f);
        background.raycastTarget = false;
        GameObject instructions = new GameObject("Selected Controls", typeof(RectTransform), typeof(Text));
        RectTransform instructionsRect = (RectTransform)instructions.transform;
        instructionsRect.SetParent(panelRect, false);
        instructionsRect.anchorMin = Vector2.zero;
        instructionsRect.anchorMax = Vector2.one;
        instructionsRect.offsetMin = new Vector2(16f, 12f);
        instructionsRect.offsetMax = new Vector2(-16f, -12f);
        controlsText = instructions.GetComponent<Text>();
        controlsText.font = countdownText.font;
        controlsText.fontSize = 22;
        controlsText.color = Color.white;
        controlsText.alignment = TextAnchor.MiddleLeft;
        controlsText.raycastTarget = false;
        controlsText.text = "Controls\n\n" + InputHandler.ControlInstructions;

        Shadow shadow = label.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(3f, -3f);
    }
}
