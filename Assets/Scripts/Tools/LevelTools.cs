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
    public Transform TrackParent;
    public Transform BeatParent;
    public Material LaneMaterialA;
    public Material LaneMaterialB;
    public Vector3 LaneOffset;

    public int SegmentsPerBeat = 8;
    public int ExtraStartSegments = 2;
    public int ExtraEndSegments = 5;
    public int TurnBufferSteps = 4;
    public float JumpLengthBeats = 1f;
    public int JumpGapInsetSteps = 1;
    public float RollLengthBeats = 1f;
    public float ObstacleHeight = 1.5f;
    
    private float BeatsPerSegment => 1 / SegmentsPerBeat;
    
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
            beat.name = $"{note.Input}_{note.Time:F2}";
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
        Vector3 prefabScale = LaneSegmentPrefab.transform.localScale;
        float laneWidth = prefabScale.x;

        int lastStep = (int)System.Math.Round(notes[notes.Count - 1].Time / secondsPerStep);
        int audioSteps = ScheduledAudioPlayer != null
            ? (int)System.Math.Ceiling(ScheduledAudioPlayer.GetMaxLength() / secondsPerStep) : 0;
        int totalSteps = Mathf.Max(lastStep, audioSteps) + ExtraEndSegments;

        byte[] mask = new byte[totalSteps];
        bool[] gap = new bool[totalSteps];
        var obstacles = new List<(int start, int length, int lane)>();

        int lane = 1, cursor = 0;
        foreach (NoteData note in notes)
        {
            double stepsF = note.Time / secondsPerStep;
            int s = (int)System.Math.Round(stepsF);
            if (System.Math.Abs(stepsF - s) > 0.01)
                Debug.LogWarning($"Note {note.Id} is off the {SegmentsPerBeat}-per-beat grid");

            for (; cursor < s && cursor < totalSteps; cursor++)
                mask[cursor] |= (byte)(1 << lane);

            if (note.Lane != lane)
                Debug.LogWarning($"Note {note.Id} ({note.Input}) is in lane {note.Lane} but the path is in lane {lane}");

            switch (note.Input)
            {
                case InputType.Left:
                case InputType.Right:
                    int target = Mathf.Clamp(lane + (note.Input == InputType.Right ? 1 : -1), 0, 2);
                    if (target == lane) Debug.LogWarning($"{note.Input} at {note.Time:F2}s has no lane to move into");
                    byte both = (byte)((1 << lane) | (1 << target));
                    for (int k = Mathf.Max(0, s - TurnBufferSteps); k <= Mathf.Min(totalSteps - 1, s + TurnBufferSteps); k++)
                        mask[k] |= both;
                    lane = target;
                    break;

                case InputType.Jump:
                    int jumpSteps = Mathf.RoundToInt(JumpLengthBeats * SegmentsPerBeat);
                    for (int k = s + JumpGapInsetSteps; k < s + jumpSteps - JumpGapInsetSteps && k < totalSteps; k++)
                        gap[k] = true;
                    break;

                case InputType.Roll:
                    obstacles.Add((s, Mathf.RoundToInt(RollLengthBeats * SegmentsPerBeat), lane));
                    break;
            }
        }
        for (; cursor < totalSteps; cursor++)
            mask[cursor] |= (byte)(1 << lane);

        Vector3 start = Player.transform.position;
        Vector3 forward = Player.transform.forward;
        Vector3 right = Player.transform.right;

        for (int s = -ExtraStartSegments; s < totalSteps; s++)
        {
            if (s >= 0 && gap[s]) continue;
            byte m = s < 0 ? (byte)(1 << 1) : mask[s];
            for (int l = 0; l < 3; l++)
            {
                if ((m & (1 << l)) == 0) continue;
                Vector3 pos = start + forward * (s * segmentLength + segmentLength / 2f)
                                    + right * ((l - 1) * laneWidth) + LaneOffset;
                GameObject seg = Instantiate(LaneSegmentPrefab, pos, Player.transform.rotation, TrackParent);
                seg.transform.localScale = new Vector3(prefabScale.x, prefabScale.y, segmentLength);
                seg.name = $"Track_{s}_Lane{l}";
                seg.GetComponentInChildren<Renderer>().sharedMaterial = ((s + l) & 1) == 0 ? LaneMaterialA : LaneMaterialB;
            }
        }

        foreach (var o in obstacles)
        {
            float length = o.length * segmentLength;
            Vector3 pos = start + forward * (o.start * segmentLength + length / 2f)
                                + right * ((o.lane - 1) * laneWidth) + LaneOffset + Vector3.up * ObstacleHeight;
            GameObject obj = Instantiate(RollObstaclePrefab, pos, Player.transform.rotation, TrackParent);
            obj.transform.localScale = new Vector3(laneWidth, obj.transform.localScale.y, length);
        }
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            DestroyImmediate(parent.GetChild(i).gameObject);
    }
}