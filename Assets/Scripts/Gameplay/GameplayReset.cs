using UnityEngine;
using UnityEngine.SceneManagement;

// Scene-local entry point shared by fall detection and the pause menu.
[DisallowMultipleComponent]
public class GameplayReset : MonoBehaviour
{
    // Retain the old entry point for any existing UnityEvent bindings.
    public void ResetGameplay()
    {
        RestartCurrentGameplayScene();
    }

    public static void RestartCurrentGameplayScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(scene.path, LoadSceneMode.Single);
    }
}
