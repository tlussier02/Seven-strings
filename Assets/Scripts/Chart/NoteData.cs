using System.Collections.Generic;

public enum InputType
{
    Jump,
    Roll,
    Left,
    Right,
    Hold,
    QTE
}
    
public struct NoteData
{
    public string Id;
    public double Beat;
    public double Time;
    public double EndTime; // only for held notes and qte
    public InputType Input;
    public int Lane;
    public bool IsCheckpoint;
    public bool IsTurn;
    public bool IsHold => Input == InputType.Hold;
}

public struct QteRegion
{
    public double StartTime; 
    public double EndTime; 
    // public int Lane;
}