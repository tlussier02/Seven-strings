using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
    public TextAsset ChartFile;
    public int Bpm { get;  private set; }

    // delete after testing
    void Start()
    {
        PrintNotes(LoadChart());
    }
    
    public List<NoteData> LoadChart()
    {
        BeatMap beatMapJson = JsonUtility.FromJson<BeatMap>(ChartFile.text);
        List<NoteData> notes  = new List<NoteData>();
        Bpm = int.Parse(beatMapJson.bpm);
        
        foreach (Chart chart in beatMapJson.charts)
        {
            foreach (Note note in chart.notes)
            {
                NoteData noteData = new NoteData();
                noteData.Id = note.uuid;
                noteData.Time = note.songPos / 1000.0;
                noteData.EndTime = noteData.Time;
                noteData.Lane = note.lane;
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
                notes.Add(noteData);
            }
        }

        return notes;
    }

    [CanBeNull]
    private Note GetNote(Note[] notes, string uuid)
    {
        foreach (Note note in notes)
        {
            if (note.uuid == uuid)
                return note;
        }
        return null;
    }

    private void PrintNotes(List<NoteData> notes)
    {
        foreach (NoteData note in notes)
        {
            print($"Id: {note.Id}\nTime: {note.Time}\tEnd Time: {note.EndTime}\nInput: {note.Input}\nLane: {note.Lane}\n\n");
        }
    }
}
