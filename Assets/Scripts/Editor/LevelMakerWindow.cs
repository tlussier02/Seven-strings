using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorToolOwner(typeof(LevelMakerContext))]
public class LevelMakerWindow : EditorWindow, ISupportsEditorTools
{
    private Camera m_Camera;
    public Camera handlesCamera => m_Camera;

    void OnEnable()
    {
        var go = EditorUtility.CreateGameObjectWithHideFlags("LevelMakerCamera", HideFlags.HideAndDontSave, typeof(Camera));
        m_Camera = go.GetComponent<Camera>();
        m_Camera.enabled = false;
    }

    void OnDisable()
    {
        if (m_Camera != null) DestroyImmediate(m_Camera.gameObject);
    }
}

[EditorToolContext("Level Maker", targetToolOwner = typeof(LevelMakerWindow))]
public class LevelMakerContext : EditorToolContext
{
    protected override Type GetEditorToolType(Tool tool) => null; // no built-in tools yet

    public override void OnToolGUI(EditorWindow window)
    {
        base.OnToolGUI(window);
        // draw timing markers with Handles here
    }
}
