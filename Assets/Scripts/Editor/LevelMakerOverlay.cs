using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Level Maker")]
public class LevelMakerOverlay : Overlay
{
    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement();
        root.Add(new Button(() => Run(p => p.PlaceBeats())) { text = "Place Beats" });
        root.Add(new Button(() => Run(p => p.PlaceTrack())) { text = "Place Track" });
        return root;
    }

    private static void Run(System.Action<BeatPlacer> action)
    {
        BeatPlacer placer = UnityEngine.Object.FindFirstObjectByType<BeatPlacer>();
        if (placer == null)
        {
            Debug.LogWarning("No BeatPlacer in the scene.");
            return;
        }

        action(placer);
        EditorSceneManager.MarkSceneDirty(placer.gameObject.scene);
    }
}