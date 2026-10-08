using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct KeyBinding
{
    public InputType InputType;
    public Key Key;
}

public class InputHandler : MonoBehaviour
{
    public event Action<InputType> OnInputPressed;
    [SerializeField] private KeyBinding[] bindings;

    public void Tick()
    {
        foreach (var binding in bindings)
        {
            if (Keyboard.current[binding.Key].wasPressedThisFrame)
            {
                OnInputPressed?.Invoke(binding.InputType);
            }
        }
    }
}
