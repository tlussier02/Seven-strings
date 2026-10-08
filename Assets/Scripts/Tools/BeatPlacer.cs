using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeatPlacer : MonoBehaviour
{
    public ChartLoader ChartLoader;
    public PlayerController Player;
    public GameObject BeatPrefab;
    public Transform TrackParent;
    public GameObject TrackSegmentPrefab;
    public Material TrackMaterialA;
    public Material TrackMaterialB;
    public float SegmentLength = 10f;
    public int ExtraSegments = 5;
    public Vector3 TrackOffset;

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
        float totalDistance = (float)notes[notes.Count() - 1].Time * Player.PlayerSpeed;
        int count = (int)(totalDistance / SegmentLength) + 1 + ExtraSegments;

        Vector3 start = Player.transform.position;
        Vector3 forward = Player.transform.forward;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = start + forward * (i * SegmentLength + SegmentLength / 2f) + TrackOffset;
            GameObject segment = Instantiate(TrackSegmentPrefab, pos, Player.transform.rotation, TrackParent);
            segment.GetComponentInChildren<Renderer>().sharedMaterial = (i % 2 == 0) ? TrackMaterialA : TrackMaterialB;
        }
    }
    
}