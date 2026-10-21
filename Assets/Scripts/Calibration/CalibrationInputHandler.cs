using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CalibrationInputHandler : MonoBehaviour
{
    public bool CanHandleInput { get; set; }
    public event Action OnInputPressed;

    public void Tick()
    {
        if (CanHandleInput){
            if (Keyboard.current[Key.Space].wasPressedThisFrame)
            {
                OnInputPressed?.Invoke();
            }
        }
    }
}
