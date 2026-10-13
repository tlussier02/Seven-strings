using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
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
        
        root.Add(ToggleBeats());
        
        return root;
    }

    private VisualElement ToggleBeats()
    {
        var beatsToggle = new ToolbarToggle { text = "Show Beats" };
        beatsToggle.SetValueWithoutNotify(true);
        beatsToggle.RegisterValueChangedCallback(evt =>
        {
            BeatPlacer placer = UnityEngine.Object.FindAnyObjectByType<BeatPlacer>();
            if (placer == null) return;

            if (evt.newValue)
                SceneVisibilityManager.instance.Show(placer.gameObject, true);
            else
                SceneVisibilityManager.instance.Hide(placer.gameObject, true);
        });
        return beatsToggle;
    }

    private static void Run(System.Action<BeatPlacer> action)
    {
        BeatPlacer placer = UnityEngine.Object.FindAnyObjectByType<BeatPlacer>();
        if (placer == null)
        {
            Debug.LogWarning("No BeatPlacer in the scene.");
            return;
        }

        action(placer);
        EditorSceneManager.MarkSceneDirty(placer.gameObject.scene);
    }
}