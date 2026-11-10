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
            LevelTools levelTools = UnityEngine.Object.FindAnyObjectByType<LevelTools>();
            if (levelTools == null || levelTools.BeatParent == null)
                return;
            
            GameObject beatParent = levelTools.BeatParent.gameObject;

            if (evt.newValue)
                SceneVisibilityManager.instance.Show(beatParent, true);
            else
                SceneVisibilityManager.instance.Hide(beatParent, true);
        });
        return beatsToggle;
    }

    private static void Run(System.Action<LevelTools> action)
    {
        LevelTools levelTools = UnityEngine.Object.FindAnyObjectByType<LevelTools>();
        if (levelTools == null)
        {
            Debug.LogWarning("No BeatPlacer in the scene.");
            return;
        }

        action(levelTools);
        EditorSceneManager.MarkSceneDirty(levelTools.gameObject.scene);
    }
}