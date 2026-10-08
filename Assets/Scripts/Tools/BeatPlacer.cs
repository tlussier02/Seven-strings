using UnityEngine;

public class BeatPlacer : MonoBehaviour
{
    public ChartLoader ChartLoader;
    public PlayerController Player;
    public GameObject BeatPrefab;

    [ContextMenu("Place Beats")]
    public void PlaceBeats()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        Vector3 startPos = Player.transform.position;
        Vector3 forward = Player.transform.forward;

        foreach (NoteData note in ChartLoader.LoadChart())
        {
            Vector3 pos = startPos + forward * (float)(note.Time * Player.PlayerSpeed);
            Instantiate(BeatPrefab, pos, Player.transform.rotation, transform);
        }
    }
}