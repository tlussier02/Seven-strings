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

    public void Awake()
    {
        float distance =  NotePrefab.transform.position.y - InputPanel.transform.position.y;
        FallSpeed = distance / FallDuration;
        DestroyY =  InputPanel.transform.position.y - DestroyBuffer;
        Debug.Log($"NotePrefab Y: {NotePrefab.transform.position.y}, InputPanel Y: {InputPanel.transform.position.y}, destroyY: {DestroyY}, fallSpeed: {FallSpeed}");
    }
    
    public void Tick(double receptor, bool foundOvertime)
    {
        if (!foundOvertime) return;

        while (NextSpawnIndex < Notes.Count &&
               receptor >= Notes[NextSpawnIndex].Time - FallDuration)
        {
            SpawnNote(Notes[NextSpawnIndex]);
            NextSpawnIndex++;
        }
    }

    public void SpawnNote(NoteData note)
    {
        GameObject newNote = Instantiate(NotePrefab);
        NoteAnimation noteAnimation = newNote.GetComponent<NoteAnimation>();
        noteAnimation.Initialize(FallSpeed, DestroyY);
    }

    public void SetNotes(List<NoteData> notes)
    {
        Notes = notes;
        NextSpawnIndex = 0;
    }
}
