using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct TestKeyBinding
{
    public InputType InputType;
    public Key Key;
}

public class TestingInputHandler : MonoBehaviour
{
    public event Action<InputType> OnInputPressed;
    [SerializeField] private TestKeyBinding[] bindings;

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