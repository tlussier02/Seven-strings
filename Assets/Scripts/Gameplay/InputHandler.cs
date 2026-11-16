using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct KeyBinding
{
    public InputType InputType;
    public Key Key;
}

public enum ControlScheme { CurrentControls, WASDControls }

public class InputHandler : MonoBehaviour
{
    public const string ControlSchemePreferenceKey = "ControlScheme";
    public static ControlScheme SelectedControlScheme =>
        PlayerPrefs.GetInt(ControlSchemePreferenceKey, 0) == 1
            ? ControlScheme.WASDControls : ControlScheme.CurrentControls;
    public static string ControlSchemeName => SelectedControlScheme == ControlScheme.WASDControls
        ? "WASD Controls" : "Current Controls";
    public static string ControlInstructions => "A / D - Move\nS - Roll\n"
        + (SelectedControlScheme == ControlScheme.WASDControls
            ? "W - Jump\nSPACE - Chord" : "SPACE - Jump\nSHIFT - Chord");

    public static void SetControlScheme(ControlScheme scheme)
    {
        PlayerPrefs.SetInt(ControlSchemePreferenceKey, scheme == ControlScheme.WASDControls ? 1 : 0);
    }

    public Key GetBindingKey(KeyBinding binding)
    {
        if (binding.InputType == InputType.Jump)
            return SelectedControlScheme == ControlScheme.WASDControls ? Key.W : Key.Space;
        if (binding.InputType == InputType.Hold)
            return SelectedControlScheme == ControlScheme.WASDControls ? Key.Space : Key.LeftShift;
        return binding.Key;
    }

    public event Action<InputType> OnInputPressed;
    public event Action<InputType> OnInputReleased;
    [SerializeField] private KeyBinding[] bindings;
    private int suppressedFrame = -1;
    public bool IsGameplayInputBlocked { get; private set; }

    // Countdown blocking is independent of the menu's time/audio pause protections.
    public void SetGameplayInputBlocked(bool blocked)
    {
        IsGameplayInputBlocked = blocked;
    }

    // A menu key must not also become a gameplay press on the resume frame.
    public void SuppressCurrentFrame()
    {
        suppressedFrame = Time.frameCount;
    }

    public void Tick()
    {
        if (IsGameplayInputBlocked || Time.timeScale == 0f || AudioListener.pause || suppressedFrame == Time.frameCount || Keyboard.current == null)
            return;

        foreach (var binding in bindings)
        {
            Key key = GetBindingKey(binding);
            if (Keyboard.current[key].wasPressedThisFrame)
                OnInputPressed?.Invoke(binding.InputType);
            
            if (Keyboard.current[key].wasReleasedThisFrame)
                OnInputReleased?.Invoke(binding.InputType);
        }
    }
}
