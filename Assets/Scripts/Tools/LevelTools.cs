using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelTools : MonoBehaviour
{
    public ChartLoader ChartLoader;
    public PlayerController Player;
    public ScheduledAudioPlayer ScheduledAudioPlayer;
    public GameObject BeatPrefab;
    public GameObject LaneSegmentPrefab;
    public GameObject RollObstaclePrefab;
    public GameObject CheckpointPrefab;
    public Transform TrackParent;
    public Transform BeatParent;
    public Material LaneMaterialA;
    public Material LaneMaterialB;
    public Vector3 LaneOffset;

    public int SegmentsPerBeat = 8;
    public int ExtraStartSegments = 2;
    public int ExtraEndSegments = 5;
    public int LaneChangeBuffer = 4;
    public float JumpLengthBeats = 1f;
    public int JumpGapInsetSteps = 1;
    public float RollLengthBeats = 1f;
    public float ObstacleHeight = 1.5f;
    
    private float BeatsPerSegment => 1 / SegmentsPerBeat;

    [ContextMenu("Clear All")]
    public void ClearTracks()
    {
        ClearChildren(TrackParent);
        ClearChildren(BeatParent);
    }
    
    [ContextMenu("Place Beats")]
    public void PlaceBeats()
    {
        ClearChildren(BeatParent);
        Vector3 start = Player.transform.position;
        Vector3 forward = Player.transform.forward;
        Vector3 right = Player.transform.right;
        float laneWidth = LaneSegmentPrefab.transform.localScale.x;

        foreach (NoteData note in ChartLoader.LoadChart())
        {
            float length = (float)((note.EndTime - note.Time) * Player.PlayerSpeed);
            Vector3 pos = start + forward * ((float)(note.Time * Player.PlayerSpeed) + length / 2f)
                                + right * ((note.Lane - 1) * laneWidth);
            pos.y = BeatPrefab.transform.position.y;
            GameObject beat = Instantiate(BeatPrefab, pos, BeatPrefab.transform.rotation, BeatParent);
            beat.name = $"{note.Input}_{note.Time}";
            if (note.IsHold)
                beat.transform.localScale = new Vector3(beat.transform.localScale.x, beat.transform.localScale.y, length);
        }
    }
    
    [ContextMenu("Place Track")]
    public void PlaceTrack()
    {
        ClearChildren(TrackParent);
        List<NoteData> notes = ChartLoader.LoadChart();
        if (notes.Count == 0) return;

        double secondsPerStep = 60.0 / ChartLoader.Bpm / SegmentsPerBeat;
        float segmentLength = (float)(secondsPerStep * Player.PlayerSpeed);

        TrackLayout layout = BuildLayout(notes, secondsPerStep);
        SpawnTiles(layout, segmentLength);
        SpawnObstacles(layout, segmentLength);
        SpawnCheckpoints();
    }
    
    private struct RollObstacle
    {
        public int StartStep;
        public int LengthSteps;
        public int Lane;
    }

    private class TrackLayout
    {
        public int TotalSteps;
        public bool[,] HasTrack;
        public bool[] IsGap;
        public List<RollObstacle> Obstacles = new List<RollObstacle>();

        public TrackLayout(int totalSteps)
        {
            TotalSteps = totalSteps;
            HasTrack = new bool[totalSteps, 3];
            IsGap = new bool[totalSteps];
        }

        public void MarkTrack(int step, int lane)
        {
            if (step >= 0 && step < TotalSteps)
                HasTrack[step, lane] = true;
        }

        public void MarkGap(int step)
        {
            if (step >= 0 && step < TotalSteps)
                IsGap[step] = true;
        }
    }

    private TrackLayout BuildLayout(List<NoteData> notes, double secondsPerStep)
    {
        TrackLayout layout = new TrackLayout(CalculateTotalSteps(notes, secondsPerStep));
        int currentLane = 1;
        int fillStep = 0;

        foreach (NoteData note in notes)
        {
            int noteStep = TimeToStep(note.Time, secondsPerStep);

            for (; fillStep < noteStep; fillStep++)
                layout.MarkTrack(fillStep, currentLane);
            
            if (!note.IsCheckpoint && note.Lane != currentLane)
                Debug.LogWarning($"Note {note.Id} ({note.Input}) at {note.Time:F2}s is in lane {note.Lane} but the path is in lane {currentLane}");
           
            switch (note.Input)
            {
                case InputType.Left:
                case InputType.Right:
                    currentLane = AddLaneChange(layout, note, noteStep, currentLane);
                    break;
                case InputType.Jump:
                    AddJumpGap(layout, noteStep);
                    break;
                case InputType.Roll:
                    AddRollObstacle(layout, noteStep, currentLane);
                    break;
            }
        }

        for (; fillStep < layout.TotalSteps; fillStep++)
            layout.MarkTrack(fillStep, currentLane);

        return layout;
    }

    private int CalculateTotalSteps(List<NoteData> notes, double secondsPerStep)
    {
        int lastNoteStep = TimeToStep(notes[notes.Count - 1].Time, secondsPerStep);
        int audioLengthSteps = 0;
        
        if (ScheduledAudioPlayer != null)
            audioLengthSteps = (int)System.Math.Ceiling(ScheduledAudioPlayer.GetMaxLength() / secondsPerStep);
        
        return Mathf.Max(lastNoteStep, audioLengthSteps) + ExtraEndSegments;
    }

    private int TimeToStep(double time, double secondsPerStep)
    {
        return (int)System.Math.Round(time / secondsPerStep);
    }

    private int AddLaneChange(TrackLayout layout, NoteData note, int noteStep, int currentLane)
    {
        int targetLane = Mathf.Clamp(currentLane + (note.Input == InputType.Right ? 1 : -1), 0, 2);

        if (targetLane == currentLane)
            Debug.LogWarning($"{note.Input} at {note.Time:F2}s has no lane to move into");

        for (int step = noteStep - LaneChangeBuffer; step <= noteStep + LaneChangeBuffer; step++)
        {
            layout.MarkTrack(step, currentLane);
            layout.MarkTrack(step, targetLane);
        }
        return targetLane;
    }

    private void AddJumpGap(TrackLayout layout, int noteStep)
    {
        int jumpLengthSteps = Mathf.RoundToInt(JumpLengthBeats * SegmentsPerBeat);
        for (int step = noteStep + JumpGapInsetSteps; step < noteStep + jumpLengthSteps - JumpGapInsetSteps; step++)
            layout.MarkGap(step);
    }

    private void AddRollObstacle(TrackLayout layout, int noteStep, int lane)
    {
        layout.Obstacles.Add(new RollObstacle
        {
            StartStep = noteStep,
            LengthSteps = Mathf.RoundToInt(RollLengthBeats * SegmentsPerBeat),
            Lane = lane
        });
    }

    private void SpawnTiles(TrackLayout layout, float segmentLength)
    {
        Vector3 prefabScale = LaneSegmentPrefab.transform.localScale;

        for (int step = -ExtraStartSegments; step < layout.TotalSteps; step++)
        {
            if (step >= 0 && layout.IsGap[step])
                continue;

            for (int lane = 0; lane < 3; lane++)
            {
                bool placeTile = step < 0 ? lane == 1 : layout.HasTrack[step, lane];
                
                if (!placeTile) 
                    continue;

                Vector3 pos = TrackPosition(step * segmentLength + segmentLength / 2f, lane);
                GameObject segment = Instantiate(LaneSegmentPrefab, pos, Player.transform.rotation, TrackParent);
                segment.transform.localScale = new Vector3(prefabScale.x, prefabScale.y, segmentLength);
                segment.name = $"Track_{step}_Lane{lane}";
                segment.GetComponentInChildren<Renderer>().sharedMaterial =
                    ((step + lane) & 1) == 0 ? LaneMaterialA : LaneMaterialB;
            }
        }
    }

    private void SpawnObstacles(TrackLayout layout, float segmentLength)
    {
        float laneWidth = LaneSegmentPrefab.transform.localScale.x;

        foreach (RollObstacle obstacle in layout.Obstacles)
        {
            float length = obstacle.LengthSteps * segmentLength;
            Vector3 pos = TrackPosition(obstacle.StartStep * segmentLength + length / 2f, obstacle.Lane)
                          + Vector3.up * ObstacleHeight;
            GameObject obstacleObject = Instantiate(RollObstaclePrefab, pos, Player.transform.rotation, TrackParent);
            obstacleObject.transform.localScale = new Vector3(laneWidth, obstacleObject.transform.localScale.y, length);
        }
    }
    
    private void SpawnCheckpoints()
    {
        float laneWidth = LaneSegmentPrefab.transform.localScale.x;

        foreach (double time in ChartLoader.Checkpoints)
        {
            Vector3 pos = TrackPosition((float)(time * Player.PlayerSpeed), 1);
            pos.y = CheckpointPrefab.transform.position.y;
            GameObject marker = Instantiate(CheckpointPrefab, pos, Player.transform.rotation, TrackParent);
            marker.transform.localScale = new Vector3(laneWidth * 3f, marker.transform.localScale.y, marker.transform.localScale.z);
        }
    }
    
    private Vector3 TrackPosition(float distance, int lane)
    {
        float laneWidth = LaneSegmentPrefab.transform.localScale.x;
        return Player.transform.position
             + Player.transform.forward * distance
             + Player.transform.right * ((lane - 1) * laneWidth)
             + LaneOffset;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            DestroyImmediate(parent.GetChild(i).gameObject);
    }
}