using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Scene-local authored spawn. Only its chart ID travels through a requested scene reload.
[DisallowMultipleComponent]
public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private string checkpointId;
    [SerializeField] private Transform checkpointSpawn;
    [SerializeField] private PlayerController player;
    [SerializeField] private RhythmController rhythm;
    [SerializeField, Range(0, 2)] private int spawnLane = 1;

    [SerializeField] private bool useChartSpawns;
    public UnityEvent OnCheckpointActivated = new UnityEvent();
    private readonly Dictionary<string, (Transform spawn, int lane)> chartSpawns = new();

    void Awake()
    {
        if (!useChartSpawns) return;
        LevelTools level = FindFirstObjectByType<LevelTools>();
        Vector3 origin = player.transform.position;
        int lane = 1;
        float width = level.LaneSegmentPrefab.transform.localScale.x;
        foreach (NoteData note in rhythm.GetComponent<ChartLoader>().LoadChart())
        {
            if (note.IsCheckpoint)
            {
                var spawn = new GameObject("Chart Checkpoint " + note.Id).transform;
                spawn.SetParent(transform, false);
                spawn.SetPositionAndRotation(origin + player.transform.forward * (float)(note.Time * player.PlayerSpeed)
                    + player.transform.right * ((lane - 1) * width), player.transform.rotation);
                chartSpawns[note.Id] = (spawn, lane);
            }
            else if (note.Input == InputType.Left || note.Input == InputType.Right)
                lane = Mathf.Clamp(lane + (note.Input == InputType.Right ? 1 : -1), 0, 2);
        }
    }

    private bool SelectSpawn(string id)
    {
        if (!useChartSpawns) return id == checkpointId && checkpointSpawn != null;
        if (!chartSpawns.TryGetValue(id, out var entry)) return false;
        checkpointId = id;
        checkpointSpawn = entry.spawn;
        spawnLane = entry.lane;
        return true;
    }

    public bool HasCheckpoint { get; private set; }
    public string CheckpointId => checkpointId;
    public double CheckpointTime { get; private set; }

    void Start()
    {
        rhythm.GetComponent<ReceptorClock>().OnBeat += HandleBeat;
    }

    void OnDestroy()
    {
        if (rhythm != null)
            rhythm.GetComponent<ReceptorClock>().OnBeat -= HandleBeat;
    }

    private void HandleBeat(NoteData note)
    {
        if (!note.IsCheckpoint || (HasCheckpoint && note.Time <= CheckpointTime))
            return;
        Transform candidate;
        if (useChartSpawns)
        {
            if (!chartSpawns.TryGetValue(note.Id, out var entry)) return;
            candidate = entry.spawn;
        }
        else
        {
            if (note.Id != checkpointId || checkpointSpawn == null) return;
            candidate = checkpointSpawn;
        }
        // Timing alone is insufficient: falling or leaving the track cannot earn a checkpoint.
        Vector3 delta = player.transform.position - candidate.position;
        if (delta.y < -0.5f || Mathf.Abs(Vector3.Dot(delta, candidate.right)) > 1.5f)
            return;
        SelectSpawn(note.Id);
        HasCheckpoint = true;
        CheckpointTime = note.Time;
        OnCheckpointActivated.Invoke();
        Debug.Log($"Checkpoint activated: ID={checkpointId}, Time={note.Time:F3}", this);
    }

    public bool Restore(string id)
    {
        if (player == null || rhythm == null || !SelectSpawn(id))
            return false;
        foreach (NoteData note in rhythm.GetComponent<ChartLoader>().LoadChart())
        {
            if (!note.IsCheckpoint || note.Id != id) continue;
            // Resolve time from the current chart, never from saved gameplay state.
            HasCheckpoint = true;
            CheckpointTime = note.Time;
            player.RestoreCheckpoint(checkpointSpawn, spawnLane);
            rhythm.GetComponent<ScheduledAudioPlayer>().ScheduleFrom(note.Time);
            rhythm.GetComponent<ReceptorClock>().Seek(note.Time);
            rhythm.GetComponent<NoteManager>().Seek(note.Time);
            Debug.Log($"Restarting from checkpoint {id} at {note.Time:F3} seconds", this);
            return true;
        }
        return false;
    }

    public void ClearCheckpoint()
    {
        HasCheckpoint = false;
        CheckpointTime = 0d;
    }
}
