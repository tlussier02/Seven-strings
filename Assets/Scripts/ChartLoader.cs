using System;
using System.Collections.Generic;
using UnityEngine;

public class ChartLoader : MonoBehaviour
{
    public TextAsset ChartFile;
    public float Bpm { get; private set; }
    public List<NoteData> Notes { get; private set; }
    
    public List<NoteData> LoadChart()
    {
        Notes = new List<NoteData>();
        string[] lines = ChartFile.text.Split('\n');

        string[] bpmRow = lines[0].Split('\t');
        Bpm = float.Parse(bpmRow[1].Trim());
        double secondsPerBeat = 60.0 / Bpm;

        for (int i = 0; i < lines.Length - 1; i++)
        {
            if (!lines[i].StartsWith("Beat"))
                continue;

            string[] beatCells = lines[i].TrimEnd('\r').Split('\t');
            string[] inputCells = lines[i + 1].TrimEnd('\r').Split('\t');

            for (int j = 1; j < beatCells.Length && j < inputCells.Length; j++)
            {
                string inputCell = inputCells[j].Trim();
                
                // no input
                if (inputCell.Length == 0)
                    continue;

                List<InputType> parsedInputTypes = new List<InputType>();
                foreach (string rawInput in inputCell.Split(','))
                {
                    string inputText = rawInput.Trim();
                    if (!Enum.TryParse(inputText, true, out InputType parsedInput))
                    {
                        Debug.LogWarning($"Unrecognized input '{inputText}' at row {i}, column {j}");
                        continue;
                    }
                    parsedInputTypes.Add(parsedInput);
                }

                if (parsedInputTypes.Count == 0)
                    continue;

                double beat = double.Parse(beatCells[j].Trim());
                double time = (beat - 1) * secondsPerBeat;
                Notes.Add(new NoteData { Time = time, InputTypes = parsedInputTypes });
            }
        }

        return Notes;
    }
}
