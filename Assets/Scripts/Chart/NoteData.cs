using System.Collections.Generic;

public enum InputType
{
    Jump,
    Roll,
    Left,
    Right,
    Hold, // need to know what specific inputs are being held down (or just have separate key for holding down)
    QTE
}
    
public struct NoteData
{
    public string Id;
    public double Time;
    public double EndTime; // only for held notes
    public InputType Input;
    public int Lane;
    public bool IsHold => Input == InputType.Hold;
}

public struct QteRegion
{
    public double StartTime; 
    public double EndTime; 
    // public int Lane;
}