using UnityEngine;
using UnityEngine.SceneManagement;

// Shared by automatic falls, manual restart, and legacy UnityEvent bindings.
[DisallowMultipleComponent]
public class GameplayReset : MonoBehaviour
{
    private static string pendingScene;
    private static string pendingCheckpoint;
    private static bool restarting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPendingRestart()
    {
        pendingScene = pendingCheckpoint = null;
        restarting = false;
        SceneManager.sceneLoaded -= CompleteRestart;
    }

    public void ResetGameplay() => RequestRestart();
    public static void RestartCurrentGameplayScene() => RequestRestart();

    public static void RequestRestart()
    {
        if (restarting) return;
        Scene scene = SceneManager.GetActiveScene();
        CheckpointManager checkpoint = FindAnyObjectByType<CheckpointManager>();
        pendingScene = scene.path;
        pendingCheckpoint = checkpoint != null && checkpoint.gameObject.scene == scene && checkpoint.HasCheckpoint
            ? checkpoint.CheckpointId : null;
        restarting = true;
        SceneManager.sceneLoaded += CompleteRestart;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Debug.Log($"Restart requested. Active checkpoint: {pendingCheckpoint ?? "none"}");
        SceneManager.LoadScene(scene.path, LoadSceneMode.Single);
    }

    private static void CompleteRestart(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= CompleteRestart;
        string id = scene.path == pendingScene ? pendingCheckpoint : null;
        // Consume the token once; ordinary menu/scene loads always begin a fresh run.
        pendingScene = pendingCheckpoint = null;
        restarting = false;
        if (id == null) return;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            CheckpointManager checkpoint = root.GetComponentInChildren<CheckpointManager>();
            if (checkpoint != null && checkpoint.Restore(id)) return;
        }
        Debug.LogWarning($"Checkpoint '{id}' is no longer authored; restarting from beginning.");
    }
}
