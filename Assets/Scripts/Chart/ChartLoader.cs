using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Note
{
    public string uuid;
    public double songPos;
    public double beat;
    public int lane;
}

[System.Serializable]
public class Link
{
    public string uuid;
    public string startNote;
    public string endNote;
}

[System.Serializable]
public class Chart
{
    public string uuid;
    public string name;
    public int laneCount;
    public Note[] notes;
    public Link[] links;
}

[System.Serializable]
public class BeatMap
{
    public string bgm;
    public string bpm;
    public string offset;
    public Chart[] charts;
}

public class ChartLoader : MonoBehaviour
{
    public TextAsset ChartFile; // should ChartLoader hold the chart or should all references be passed to RhythmController
    public int Bpm { get;  private set; }
    
    public List<NoteData> LoadChart()
    {
        BeatMap beatMapJson = JsonUtility.FromJson<BeatMap>(ChartFile.text);
        List<NoteData> notes  = new List<NoteData>();
        Bpm = int.Parse(beatMapJson.bpm);
        Chart chart = beatMapJson.charts[0]; // assuming there is only one chart per song
    
        foreach (Note note in chart.notes)
        {
            NoteData noteData = new NoteData();
            noteData.Id = note.uuid;
            noteData.Time = note.songPos / 1000.0;
            noteData.EndTime = noteData.Time;
            noteData.Lane = note.lane - 1;
            noteData.Input = InputType.Jump;

            bool isEnd = false;
            foreach (Link link in chart.links)
            {
                if (note.uuid == link.endNote)
                    isEnd = true;
                if (link.startNote != note.uuid)
                    continue;

                Note startNote = GetNote(chart.notes, link.startNote);
                Note endNote = GetNote(chart.notes, link.endNote);
                
                if (startNote == null || endNote == null)
                    continue;
                
                // lane change
                if (startNote.beat == endNote.beat)
                    noteData.Input = startNote.lane < endNote.lane ? InputType.Right : InputType.Left;
                // roll
                else if (endNote.beat - startNote.beat <= 0.25)
                    noteData.Input = InputType.Roll;
                // hold
                else if (endNote.songPos - startNote.songPos < 5000)
                {
                    noteData.Input = InputType.Hold;
                    noteData.EndTime = endNote.songPos / 1000.0;
                }
                // QTE
                else
                {
                    noteData.Input = InputType.QTE;
                    noteData.EndTime = endNote.songPos / 1000.0;
                }
            }

            if (isEnd)
                continue;

            if (noteData.Input == InputType.QTE)
                continue;
            notes.Add(noteData);
        }
        
        notes.Sort((x, y) => x.Time.CompareTo(y.Time));
        return notes;
    }
    
    private Note GetNote(Note[] notes, string uuid)
    {
        foreach (Note note in notes)
        {
            if (note.uuid == uuid)
                return note;
        }
        return null;
    }
}
