using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ControlPreset { Default, WASD }
public static class KeyBindingSettings
{
    private const string KeyPrefix = "SevenStrings.Binding.";
    // Written by the old two-scheme toggle; only used to pick first-run defaults.
    private const string LegacyControlSchemeKey = "ControlScheme";

    private static readonly InputType[] RebindableActions =
        { InputType.Left, InputType.Right, InputType.Jump, InputType.Roll, InputType.Hold, InputType.QTE };

    // Prevents these keys from being assigned
    private static readonly Key[] ReservedKeys = { Key.Escape, Key.Backquote };
    
    public static event Action OnBindingsChanged; // so UI labels can update

    public static Key GetKey(InputType action)
    {
        Key saved = (Key)PlayerPrefs.GetInt(KeyPrefix + action, (int)Key.None);
        return IsAssignable(saved) ? saved : GetDefaultKey(action, LegacyPreset);
    }

    public static bool IsReserved(Key key) => Array.IndexOf(ReservedKeys, key) >= 0;

    public static bool IsAssignable(Key key) =>
        key != Key.None && Enum.IsDefined(typeof(Key), key) && !IsReserved(key);

    // If another action already uses the newly assigned key, the two swap
    // so no action is ever left unbound or sharing a key
    public static bool TrySetKey(InputType action, Key newKey, out InputType? swappedWith)
    {
        swappedWith = null;
        if (!IsAssignable(newKey))
            return false;

        Key oldKey = GetKey(action);
        if (oldKey == newKey)
            return true;

        foreach (InputType other in RebindableActions)
        {
            if (other != action && GetKey(other) == newKey)
            {
                Save(other, oldKey);
                swappedWith = other;
                break;
            }
        }

        Save(action, newKey);
        OnBindingsChanged?.Invoke();
        return true;
    }

    public static void ApplyPreset(ControlPreset preset)
    {
        foreach (InputType action in RebindableActions)
            Save(action, GetDefaultKey(action, preset));
        OnBindingsChanged?.Invoke();
    }

    public static Key GetDefaultKey(InputType action, ControlPreset preset)
    {
        switch (action)
        {
            case InputType.Left: return Key.A;
            case InputType.Right: return Key.D;
            case InputType.Roll: return Key.S;
            case InputType.QTE: return Key.Q;
            case InputType.Jump: return preset == ControlPreset.WASD ? Key.W : Key.Space;
            case InputType.Hold: return preset == ControlPreset.WASD ? Key.Space : Key.LeftShift;
            default: return Key.None;
        }
    }
    
    public static string GetActionName(InputType action)
    {
        switch (action)
        {
            case InputType.Left: return "Move Left";
            case InputType.Right: return "Move Right";
            case InputType.Hold: return "Chord";
            default: return action.ToString();
        }
    }

    // Uses the keyboard layout's display name when available (i.e. "Left Shift" or "Down Arrow")
    public static string GetKeyName(Key key)
    {
        if (key == Key.None)
            return "Unbound";

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            try
            {
                string name = keyboard[key].displayName;
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            catch (ArgumentOutOfRangeException) { }
        }
        return key.ToString();
    }

    // Shown on the start countdown panel
    public static string ControlInstructions =>
        Upper(InputType.Left) + " / " + Upper(InputType.Right) + " - Move\n"
        + Upper(InputType.Roll) + " - Roll\n"
        + Upper(InputType.Jump) + " - Jump\n"
        + Upper(InputType.Hold) + " - Chord";

    private static string Upper(InputType action) => GetKeyName(GetKey(action)).ToUpperInvariant();

    private static ControlPreset LegacyPreset =>
        PlayerPrefs.GetInt(LegacyControlSchemeKey, 0) == 1 ? ControlPreset.WASD : ControlPreset.Default;

    // called by PlayerPrefs.Save() when the menu closes in SettingsMenuController
    private static void Save(InputType action, Key key) => PlayerPrefs.SetInt(KeyPrefix + action, (int)key);
}
