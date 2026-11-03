using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Note
{
    public string uuid;
    public int songPos;
    public int beat;
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
        LoadChart();
    }
    
    public List<NoteData> LoadChart()
    {
        BeatMap beatMapJson = JsonUtility.FromJson<BeatMap>(ChartFile.text);
        List<NoteData> notes  = new List<NoteData>();
        
        

        return notes;
    }
}
