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
    private int suppressedFrame = -1;

    // A menu key must not also become a gameplay press on the resume frame.
    public void SuppressCurrentFrame()
    {
        suppressedFrame = Time.frameCount;
    }

    public void Tick()
    {
        if (Time.timeScale == 0f || AudioListener.pause || suppressedFrame == Time.frameCount || Keyboard.current == null)
            return;

        foreach (var binding in bindings)
        {
            if (Keyboard.current[binding.Key].wasPressedThisFrame)
            {
                OnInputPressed?.Invoke(binding.InputType);
            }
        }
    }
}
