using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Run before gameplay Update so Escape cannot leak into the rhythm input poll.
[DefaultExecutionOrder(-1000)]
public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private InputHandler gameplayInput;

    public bool IsOpen { get; private set; }

    private GameObject uiRoot;
    private GameObject overlay;
    private Button settingsButton;
    private Button resumeButton;
    private Font font;
    private float previousTimeScale;
    private bool previousAudioPause;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private GameObject previousSelection;
    private bool preferencesDirty;

    void Awake()
    {
        AudioListener.volume = AudioVolumeSettings.MasterVolume;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
    }

    void OnEnable()
    {
        if (uiRoot != null)
            uiRoot.SetActive(true);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsOpen)
                Close();
            else
                Open();
        }
    }

    public void Open()
    {
        if (IsOpen || !isActiveAndEnabled)
            return;

        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        IsOpen = true;
        AudioListener.pause = true; // Freezes DSP time and pending scheduled playback too.
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        settingsButton.gameObject.SetActive(false);
        overlay.SetActive(true);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        if (overlay != null)
            overlay.SetActive(false);
        if (settingsButton != null)
            settingsButton.gameObject.SetActive(true);
        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        if (gameplayInput != null)
            gameplayInput.SuppressCurrentFrame();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy ? previousSelection : null);
        SavePreferences();
    }

    void OnDisable()
    {
        // Scene unload or disabling the HUD must not leave global pause behind.
        Close();
        SavePreferences();
        if (uiRoot != null)
            uiRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (uiRoot != null)
            Destroy(uiRoot);
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
            SavePreferences();
    }

    void OnApplicationQuit()
    {
        SavePreferences();
    }

    private void SavePreferences()
    {
        if (!preferencesDirty)
            return;
        PlayerPrefs.Save();
        preferencesDirty = false;
    }

    private void BuildUI()
    {
        uiRoot = new GameObject("Settings UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = uiRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = uiRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 540f);
        // Fit the reference area in either dimension, including narrow windows.
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        settingsButton = CreateButton("Settings (Esc)", uiRoot.transform, new Vector2(160f, 42f), Open);
        RectTransform settingsRect = (RectTransform)settingsButton.transform;
        settingsRect.anchorMin = settingsRect.anchorMax = settingsRect.pivot = Vector2.one;
        settingsRect.anchoredPosition = new Vector2(-20f, -20f);

        RectTransform backdrop = CreateRect("Settings Overlay", uiRoot.transform, Vector2.zero, Vector2.zero);
        Stretch(backdrop);
        backdrop.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        overlay = backdrop.gameObject;

        RectTransform panel = CreateRect("Settings Panel", backdrop, new Vector2(440f, 330f), Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = new Color(0.09f, 0.11f, 0.16f, 1f);
        CreateText("SETTINGS", panel, new Vector2(380f, 44f), new Vector2(0f, 120f), 28);
        CreateVolumeControl(panel, "Master Volume", 60f, AudioVolumeSettings.MasterVolume, true);
        CreateVolumeControl(panel, "Music Volume", -30f, AudioVolumeSettings.MusicVolume, false);
        resumeButton = CreateButton("Resume", panel, new Vector2(180f, 44f), Close);
        ((RectTransform)resumeButton.transform).anchoredPosition = new Vector2(0f, -115f);
        overlay.SetActive(false);
    }

    private void CreateVolumeControl(Transform parent, string label, float y, float initialValue, bool master)
    {
        Text title = CreateText(label, parent, new Vector2(260f, 30f), new Vector2(-60f, y), 20);
        title.alignment = TextAnchor.MiddleLeft;
        Text percentage = CreateText("", parent, new Vector2(80f, 30f), new Vector2(150f, y - 32f), 20);
        RectTransform sliderRect = CreateRect(label + " Slider", parent, new Vector2(290f, 32f), new Vector2(-45f, y - 32f));
        sliderRect.gameObject.AddComponent<Image>().color = Color.clear;
        Slider slider = sliderRect.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.wholeNumbers = true;

        RectTransform track = CreateRect("Track", sliderRect, Vector2.zero, Vector2.zero);
        Stretch(track);
        track.offsetMin = new Vector2(0f, 12f);
        track.offsetMax = new Vector2(0f, -12f);
        track.gameObject.AddComponent<Image>().color = new Color(0.25f, 0.28f, 0.34f);
        RectTransform fill = CreateRect("Fill", track, Vector2.zero, Vector2.zero);
        Stretch(fill);
        fill.gameObject.AddComponent<Image>().color = new Color(0.35f, 0.75f, 0.95f);
        RectTransform handleArea = CreateRect("Handle Area", sliderRect, Vector2.zero, Vector2.zero);
        Stretch(handleArea);
        handleArea.offsetMin = new Vector2(9f, 0f);
        handleArea.offsetMax = new Vector2(-9f, 0f);
        RectTransform handle = CreateRect("Handle", handleArea, new Vector2(18f, 28f), Vector2.zero);
        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = Color.white;
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.SetValueWithoutNotify(Mathf.RoundToInt(initialValue * 100f));
        percentage.text = Mathf.RoundToInt(initialValue * 100f) + "%";
        slider.onValueChanged.AddListener(value =>
        {
            percentage.text = Mathf.RoundToInt(value) + "%";
            float volume = value / 100f;
            if (master)
                AudioListener.volume = volume;
            else
            {
                foreach (ScheduledAudioPlayer player in FindObjectsByType<ScheduledAudioPlayer>())
                    player.SetMusicVolume(volume);
            }
            PlayerPrefs.SetFloat(master ? AudioVolumeSettings.MasterVolumeKey : AudioVolumeSettings.MusicVolumeKey, volume);
            preferencesDirty = true;
        });
    }

    private Button CreateButton(string label, Transform parent, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = CreateRect(label, parent, size, Vector2.zero);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.35f, 0.48f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        Text text = CreateText(label, rect, Vector2.zero, Vector2.zero, 20);
        Stretch((RectTransform)text.transform);
        return button;
    }

    private Text CreateText(string value, Transform parent, Vector2 size, Vector2 position, int fontSize)
    {
        RectTransform rect = CreateRect(value, parent, size, position);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.layer = 5;
        RectTransform rect = (RectTransform)child.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
