using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class ControlsPageController : MonoBehaviour
{
    [Serializable]
    public class RebindRow
    {
        public InputType action;
        public Button keyButton;
        [NonSerialized] public Graphic keyLabel;
    }

    [SerializeField] private RebindRow[] rows = new RebindRow[0];
    [SerializeField] private Button resetDefaultButton;
    [SerializeField] private Button resetWasdButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Graphic statusLabel; //provides user instructions

    public event Action BackRequested;
    public bool IsListening => listeningRow != null;

    private RebindRow listeningRow;
    private bool previousNavigationSetting = true;

    void Awake()
    {
        foreach (RebindRow row in rows)
        {
            if (row.keyButton == null)
            {
                continue;
            }
            RebindRow captured = row;
            row.keyLabel = FindLabel(row.keyButton);
            row.keyButton.onClick.AddListener(() => StartListening(captured));
        }

        if (resetDefaultButton != null)
            resetDefaultButton.onClick.AddListener(() => ResetTo(ControlPreset.Default));
        if (resetWasdButton != null)
            resetWasdButton.onClick.AddListener(() => ResetTo(ControlPreset.WASD));
        if (backButton != null)
            backButton.onClick.AddListener(() => BackRequested?.Invoke());
    }

    void OnEnable()
    {
        KeyBindingSettings.OnBindingsChanged += Refresh;
        SetStatus("");
        Refresh();
    }

    void OnDisable()
    {
        CancelListening();
        KeyBindingSettings.OnBindingsChanged -= Refresh;
    }

    void Update()
    {
        if (!IsListening)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            CancelListening();
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            CancelListening();
            SetStatus("");
            return;
        }

        foreach (KeyControl key in keyboard.allKeys)
        {
            if (key == null || !key.wasPressedThisFrame)
                continue;

            Key pressed = key.keyCode;
            if (!KeyBindingSettings.IsAssignable(pressed))
            {
                SetStatus(KeyBindingSettings.GetKeyName(pressed) + " is unavailable. Pick another key (Esc to cancel).");
                return; // keep listening
            }

            RebindRow row = listeningRow;
            StopListening();
            KeyBindingSettings.TrySetKey(row.action, pressed, out InputType? swappedWith);
            SetStatus(swappedWith.HasValue
                ? KeyBindingSettings.GetActionName(row.action) + " and " + KeyBindingSettings.GetActionName(swappedWith.Value) + " swapped keys."
                : "");
            Refresh();
            return;
        }
    }

    public void CancelListening()
    {
        if (!IsListening)
            return;
        StopListening();
        Refresh();
    }

    public void Refresh()
    {
        foreach (RebindRow row in rows)
        {
            if (row.keyLabel == null)
                continue;
            SetText(row.keyLabel, row == listeningRow
                ? "..."
                : KeyBindingSettings.GetKeyName(KeyBindingSettings.GetKey(row.action)));
        }
    }

    private void StartListening(RebindRow row)
    {
        if (IsListening)
            StopListening();

        listeningRow = row;
        // A clicked button stays selected, so Space/Enter would "click" it again and arrow
        // keys would move the selection. Deselect it and pause UI navigation while listening
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            previousNavigationSetting = EventSystem.current.sendNavigationEvents;
            EventSystem.current.sendNavigationEvents = false;
        }
        SetStatus("Press a key for " + KeyBindingSettings.GetActionName(row.action) + " (Esc to cancel)");
        Refresh();
    }

    private void StopListening()
    {
        listeningRow = null;
        if (EventSystem.current != null)
            EventSystem.current.sendNavigationEvents = previousNavigationSetting;
    }

    private void ResetTo(ControlPreset preset)
    {
        CancelListening();
        KeyBindingSettings.ApplyPreset(preset);
        SetStatus(preset == ControlPreset.WASD ? "Reset to WASD controls." : "Reset to default controls.");
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
            SetText(statusLabel, message);
    }

    private static Graphic FindLabel(Button button)
    {
        Graphic label = button.GetComponentInChildren<TMP_Text>(true);
        return label != null ? label : button.GetComponentInChildren<Text>(true);
    }

    private static void SetText(Graphic target, string value)
    {
        if (target is TMP_Text tmp)
            tmp.text = value;
        else if (target is Text text)
            text.text = value;
    }
}
