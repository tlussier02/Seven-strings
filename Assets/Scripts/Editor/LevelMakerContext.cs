using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorToolContext("Level Maker", null)]
public class LevelMakerContext : EditorToolContext
{
    private BeatPlacer placer;

    protected override Type GetEditorToolType(Tool tool) => null;

    public override void OnToolGUI(EditorWindow window)
    {
        base.OnToolGUI(window);

        if (placer == null)
            placer = UnityEngine.Object.FindFirstObjectByType<BeatPlacer>();

        Handles.BeginGUI();
        if (placer == null)
        {
            GUI.Label(new Rect(10, 10, 220, 20), "No BeatPlacer in scene");
        }
        else
        {
            if (GUI.Button(new Rect(10, 10, 110, 24), "Place Beats")) placer.PlaceBeats();
            if (GUI.Button(new Rect(10, 38, 110, 24), "Place Track")) placer.PlaceTrack();
        }
        Handles.EndGUI();
    }
}