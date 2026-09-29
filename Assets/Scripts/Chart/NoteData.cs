using System.Collections.Generic;

public enum InputType
{
    Jump,
    Roll,
    Left,
    Right,
    QTE
}
    
public struct NoteData
{
    public double Time;
    public List<InputType> InputTypes;
}