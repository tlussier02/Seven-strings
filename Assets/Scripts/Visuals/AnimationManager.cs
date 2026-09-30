using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public GameObject NotePrefab;
    public GameObject InputPanel;
    public Vector3 SpawnPosition;
    public float FallDuration;
    public float DestroyBuffer = 400f;
    
    private List<NoteData> Notes;
    private int NextSpawnIndex;
    private float FallSpeed;
    private float DestroyY;

    public void Awake()
    {
        float distance =  SpawnPosition.y - InputPanel.transform.position.y;
        FallSpeed = distance / FallDuration;
        DestroyY =  InputPanel.transform.position.y - DestroyBuffer;
    }
    
    public void Tick(double receptor)
    {
        
        while (NextSpawnIndex < Notes.Count &&
               receptor >= Notes[NextSpawnIndex].Time - FallDuration)
        {
            SpawnNote(Notes[NextSpawnIndex]);
            NextSpawnIndex++;
        }
    }

    public void SpawnNote(NoteData note)
    {
        GameObject newNote = Instantiate(NotePrefab, SpawnPosition, Quaternion.identity);
        NoteAnimation noteAnimation = newNote.GetComponent<NoteAnimation>();
        noteAnimation.Initialize(FallSpeed, DestroyY);
    }

    public void SetNotes(List<NoteData> notes)
    {
        Notes = notes;
        NextSpawnIndex = 0;
    }
}
