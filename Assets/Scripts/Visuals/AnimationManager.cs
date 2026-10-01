using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public GameObject NotePrefab;
    public GameObject InputPanel;
    public float FallDuration;
    public float DestroyBuffer;
    
    private List<NoteData> Notes;
    private int NextSpawnIndex;
    private float FallSpeed;
    private float DestroyY;
    private Dictionary<double, NoteAnimation> ActiveAnimations = new Dictionary<double, NoteAnimation>();
    
    public void Awake()
    {
        float distance =  NotePrefab.transform.position.y - InputPanel.transform.position.y;
        FallSpeed = distance / FallDuration;
        DestroyY =  InputPanel.transform.position.y - DestroyBuffer;
    }
    
    public void Tick(double receptor, bool foundOvertime, double preSyncTime)
    {
        double effectiveTime = foundOvertime ? receptor : preSyncTime;
        
        while (NextSpawnIndex < Notes.Count &&
               effectiveTime >= Notes[NextSpawnIndex].Time - FallDuration)
        {
            SpawnNote(Notes[NextSpawnIndex]);
            NextSpawnIndex++;
        }
    }

    public void HandleJudgement(NoteData note,InputType input, InputJudge.Judgement judgement)
    {
        if (ActiveAnimations.TryGetValue(note.Time, out NoteAnimation animation))
        {
            Destroy(animation.gameObject);
            ActiveAnimations.Remove(note.Time);
        }
    }

    public void SpawnNote(NoteData note)
    {
        GameObject newNote = Instantiate(NotePrefab);
        NoteAnimation noteAnimation = newNote.GetComponent<NoteAnimation>();
        noteAnimation.Initialize(FallSpeed, DestroyY);
        ActiveAnimations[note.Time] = noteAnimation;
    }

    public void SetNotes(List<NoteData> notes)
    {
        Notes = notes;
        NextSpawnIndex = 0;
    }
}
