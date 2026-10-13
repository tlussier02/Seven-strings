using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorToolContext("Level Maker", typeof(BeatPlacer))]
public class LevelMakerContext : EditorToolContext
{
    protected override Type GetEditorToolType(Tool tool) => null;

    public override void OnToolGUI(EditorWindow window)
    {
        base.OnToolGUI(window);

        GameObject selected = Selection.activeGameObject;
        BeatPlacer placer = selected != null ? selected.GetComponent<BeatPlacer>() : null;
        if (placer == null) return;

        Handles.BeginGUI();
        if (GUI.Button(new Rect(10, 10, 110, 24), "Place Beats")) placer.PlaceBeats();
        if (GUI.Button(new Rect(10, 38, 110, 24), "Place Track")) placer.PlaceTrack();
        Handles.EndGUI();
    }
}