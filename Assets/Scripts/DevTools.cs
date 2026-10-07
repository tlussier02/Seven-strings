using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DevTools : MonoBehaviour
{
    // === Keyboard Shortcuts ===
    public Key ToggleOverlayKey = Key.Backquote; // The ` key (above Tab)
    public Key ReloadSceneKey = Key.R;
    
    // The position where the player should respawn - assign this in the Inspector
    public Transform PlayerSpawn; 

    // === Private Variables (Internal State) ===
    // Whether the dev tools window is currently visible
    private bool visible = false;
    
    // Defines the position and size of the dev tools window (x, y, width, height)
    private Rect windowRectangle = new Rect(30, 30, 400, 300);
    
    // Text input field for loading a specific scene by name
    private string sceneToLoad = "";

    /// <summary>
    /// Called every frame by Unity - checks for keyboard input to activate dev tool features
    /// </summary>
    private void Update()
    {
        HandleToggleOverlay();
        HandleReloadScene();
    }

    /// <summary>
    /// Checks if the player pressed the key to show/hide the dev tools window
    /// </summary>
    private void HandleToggleOverlay()
    {
        if (Keyboard.current[ToggleOverlayKey].wasPressedThisFrame)
        {
            // Toggle means: if visible, hide it; if hidden, show it
            visible = !visible;
        }
    }

    /// <summary>
    /// Checks if the player pressed the key to reload the current scene
    /// </summary>
    private void HandleReloadScene()
    {
        if (Keyboard.current[ReloadSceneKey].wasPressedThisFrame)
        {
            ReloadActiveScene();
        }
    }

    /// <summary>
    /// Called by Unity to render GUI elements (buttons, text fields, windows, etc.)
    /// OnGUI is an older Unity UI system, but still useful for quick debug interfaces
    /// </summary>
    private void OnGUI()
    {
        // Don't draw anything if the window is hidden
        if (!visible)
            return;

        // Draw a draggable window with a unique ID number
        // The window will call DrawWindow() to fill in its contents
        windowRectangle = GUI.Window(987654, windowRectangle, DrawWindow, "Dev Tools");
    }

    /// <summary>
    /// Draws all the contents inside the dev tools window
    /// </summary>
    /// <param name="id">Unique window ID (required by Unity's GUI system)</param>
    private void DrawWindow(int id)
    {
        // Draw scene management controls (reload, load by name)
        DrawSceneControls();
        
        // Allow the user to drag the window around the screen
        EnableWindowDragging();
    }

    /// <summary>
    /// Draws a button to respawn the player at the spawn point
    /// </summary>
    private void DrawRespawnButton()
    {
        if (GUI.Button(new Rect(10, 30, 130, 25), "Respawn Player"))
        {
            RespawnPlayer();
        }
    }

    /// <summary>
    /// Draws all scene management controls
    /// </summary>
    private void DrawSceneControls()
    {
        DrawReloadSceneButton();
        DrawRespawnButton();
        DrawLoadSceneControls();
    }

    /// <summary>
    /// Draws a button to reload the current scene
    /// </summary>
    private void DrawReloadSceneButton()
    {
        if (GUI.Button(new Rect(10, 60, 130, 25), "Reload Scene (F5)"))
        {
            ReloadActiveScene();
        }
    }

    /// <summary>
    /// Draws controls to load a scene by typing its name
    /// </summary>
    private void DrawLoadSceneControls()
    {
        // Label for the text field
        GUI.Label(new Rect(10, 90, 100, 20), "Load scene");
        
        // Text field to type a scene name
        sceneToLoad = GUI.TextField(new Rect(10, 115, 120, 20), sceneToLoad);
        
        // Button to load the typed scene name
        if (GUI.Button(new Rect(10, 140, 70, 20), "Load"))
        {
            LoadScene(sceneToLoad);
        }
    }

    /// <summary>
    /// Makes the entire window draggable by clicking and dragging the title bar
    /// </summary>
    private void EnableWindowDragging()
    {
        // A large rectangle covering the title bar area allows dragging
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    /// <summary>
    /// Safely converts a string to an integer, returning the default value if conversion fails
    /// This prevents crashes when the user types invalid text
    /// </summary>
    /// <param name="input">The string to convert</param>
    /// <param name="defaultValue">The value to return if conversion fails</param>
    /// <returns>The parsed integer or the default value</returns>
    private int ParseIntField(string input, int defaultValue)
    {
        int parsedValue;
        
        // Try to parse the string as an integer
        if (int.TryParse(input, out parsedValue))
        {
            // Success! Return the parsed value
            return parsedValue;
        }
        
        // Failed to parse, return the default value
        return defaultValue;
    }

    /// <summary>
    /// Respawns the player at the designated spawn point (or their current position if no spawn point is set)
    /// </summary>
    private void RespawnPlayer()
    {
        Vector3 spawnPos = PlayerSpawn.position;
    }

    /// <summary>
    /// Reloads the currently active scene, effectively restarting the level
    /// </summary>
    private void ReloadActiveScene()
    {
        // Get the current scene
        Scene scene = SceneManager.GetActiveScene();
        
        // Load it again (this resets everything in the scene)
        SceneManager.LoadScene(scene.name);
    }

    /// <summary>
    /// Loads a scene by name
    /// </summary>
    /// <param name="sceneName">The name of the scene to load (must match exactly)</param>
    private void LoadScene(string sceneName)
    {
        // Only load if the user actually typed something
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
