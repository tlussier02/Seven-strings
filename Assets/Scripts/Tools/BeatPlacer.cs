using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeatPlacer : MonoBehaviour
{
    public ChartLoader ChartLoader;
    public LanesPlayerController Player;
    public GameObject BeatPrefab;
    public Transform TrackParent;
    public GameObject LaneSegmentPrefab;
    public Material LaneMaterialA;
    public Material LaneMaterialB;
    
    public Vector3 LaneOffset;
    public int ExtraStartSegments = 2;
    public int ExtraEndSegments = 5;
    public float BeatsPerSegment = 1f;

    [ContextMenu("Place Beats")]
    public void PlaceBeats()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        foreach (NoteData note in ChartLoader.LoadChart())
        {
            Vector3 pos = BeatPrefab.transform.position;
            pos.z = Player.transform.position.z + (float)(note.Time * Player.PlayerSpeed);
            Instantiate(BeatPrefab, pos, BeatPrefab.transform.rotation, transform);
        }
    }
    
    [ContextMenu("Place Track")]
    public void PlaceTrack()
    {
        for (int i = TrackParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(TrackParent.GetChild(i).gameObject);

        List<NoteData> notes = ChartLoader.LoadChart();

        Vector3 prefabScale = LaneSegmentPrefab.transform.localScale;
        float laneWidth = prefabScale.x;
        float segmentLength = (60f / ChartLoader.Bpm) * Player.PlayerSpeed * BeatsPerSegment;

        float totalDistance = (float)notes[notes.Count - 1].Time * Player.PlayerSpeed;
        int forwardCount = (int)(totalDistance / segmentLength) + 1 + ExtraEndSegments;

        Vector3 start = Player.transform.position;
        Vector3 forward = Player.transform.forward;
        Vector3 right = Player.transform.right;

        for (int n = 0; n < ExtraStartSegments + forwardCount; n++)
        {
            int i = n - ExtraStartSegments;
            for (int lane = 0; lane < 3; lane++)
            {
                Vector3 pos = start
                              + forward * (i * segmentLength + segmentLength / 2f)
                              + right * ((lane - 1) * laneWidth)
                              + LaneOffset;
                GameObject segment = Instantiate(LaneSegmentPrefab, pos, Player.transform.rotation, TrackParent);
                segment.transform.localScale = new Vector3(prefabScale.x, prefabScale.y, segmentLength);
                segment.name = $"Track_{i}_Lane{lane}";
                segment.GetComponentInChildren<Renderer>().sharedMaterial =
                    ((n + lane) % 2 == 0) ? LaneMaterialA : LaneMaterialB;
            }
        }
    }
}