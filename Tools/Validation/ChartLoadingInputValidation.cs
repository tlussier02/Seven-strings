using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

// Run only in an isolated project copy: install under Assets/Editor, then
// Unity -batchmode -nographics -executeMethod ChartLoadingInputValidation.Run (without -quit).
public static class ChartLoadingInputValidation
{
    static IEnumerator routine;
    static double deadline;
    static Keyboard keyboard;
    static int events, activations, earlierNotes;
    static PlayerController Player => UnityEngine.Object.FindFirstObjectByType<PlayerController>();
    static RhythmController Rhythm => UnityEngine.Object.FindFirstObjectByType<RhythmController>();
    static CheckpointManager Manager => UnityEngine.Object.FindFirstObjectByType<CheckpointManager>();
    static ReceptorClock Clock => Rhythm.GetComponent<ReceptorClock>();
    static ScheduledAudioPlayer Audio => Rhythm.GetComponent<ScheduledAudioPlayer>();
    static NoteManager Notes => Rhythm.GetComponent<NoteManager>();
    static SettingsMenuController Menu => UnityEngine.Object.FindFirstObjectByType<SettingsMenuController>();

    public static void Run()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        // Headless Editor has no focused Game view. This is test-only configuration.
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        EditorSceneManager.OpenScene("Assets/Scenes/ChartLoading.unity");
        deadline = EditorApplication.timeSinceStartup + 120;
        routine = Validate();
        Application.logMessageReceived += OnLog;
        EditorApplication.update += Tick;
        EditorApplication.isPlaying = true;
    }

    static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            // Do not hide runtime errors behind successful assertions.
            EditorApplication.update -= Tick;
            EditorApplication.Exit(1);
        }
    }
    static void Check(bool ok, string message)
    {
        if (!ok) throw new Exception(message);
        Debug.Log("CHECK PASS: " + message);
    }
    static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Validation timeout");
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || Player == null) return;
            if (!routine.MoveNext())
            {
                Debug.Log("CHARTLOADING INPUT AND CHECKPOINT VALIDATION PASSED");
                EditorApplication.update -= Tick;
                EditorApplication.Exit(0);
            }
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.update -= Tick; EditorApplication.Exit(1); }
    }
    static IEnumerator Wait(float seconds)
    {
        double end = EditorApplication.timeSinceStartup + seconds;
        while (EditorApplication.timeSinceStartup < end) yield return null;
    }
    static void DisableFallsForInputProbe()
    {
        Player.GetComponent<PlayerFallDetector>().enabled = false;
        Player.GetComponent<Rigidbody>().useGravity = false;
    }
    static void KeyState(params Key[] keys)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.Update();
        // Exercise the real polling/event/movement path deterministically in headless Editor.
        Player.InputHandler.Tick();
    }
    static IEnumerator Movement()
    {
        while (!Clock.FoundOvertime || Clock.Receptor < Audio.SongOffset + .2) yield return null;
        Check(!Player.InputHandler.IsGameplayInputBlocked && Time.timeScale == 1 && !AudioListener.pause, "Gameplay unblocked after lead-in");
        float x = Player.transform.position.x, y = Player.transform.position.y, z = Player.transform.position.z;
        int before = events;
        KeyState(Key.A);
        for (var w = Wait(.22f); w.MoveNext();) yield return null;
        Check(Math.Abs(Player.transform.position.x - (x - 3.5f)) < .02f, "A reaches PlayerController and moves left one lane");
        KeyState(); yield return null;
        KeyState(Key.D);
        for (var w = Wait(.22f); w.MoveNext();) yield return null;
        Check(Math.Abs(Player.transform.position.x - x) < .02f, "D reaches PlayerController and moves right one lane");
        KeyState(); yield return null;
        KeyState(Key.Space);
        for (var w = Wait(.12f); w.MoveNext();) yield return null;
        Check(Player.transform.position.y > y + .5f, "Space reaches PlayerController and jumps");
        KeyState(); yield return null;
        KeyState(Key.S); KeyState();
        Check(events >= before + 4, "InputHandler emits left/right/jump/roll events (roll remains intentionally empty)");
        Check(Player.transform.position.z > z + 2, "Forward movement continues after DSP start");
        for (var w = Wait(.6f); w.MoveNext();) yield return null;
    }
    static IEnumerator Reload(bool fresh = false)
    {
        if (fresh) SceneManager.LoadScene("Assets/Scenes/ChartLoading.unity");
        else GameplayReset.RequestRestart();
        yield return null; yield return null;
        Player.InputHandler.OnInputPressed += _ => events++;
    }
    static IEnumerator Validate()
    {
        keyboard = InputSystem.AddDevice<Keyboard>();
        Player.InputHandler.OnInputPressed += _ => events++;
        Check(Player.enabled && Player.gameObject.activeInHierarchy, "Player enabled and active");
        Check(UnityEngine.Object.FindObjectsByType<InputHandler>(FindObjectsSortMode.None).Length == 1 && Player.InputHandler == Rhythm.GetComponent<InputHandler>(), "One correct scene input handler");
        var chart = Rhythm.GetComponent<ChartLoader>();
        var timeline = chart.LoadChart();
        Check(AssetDatabase.GetAssetPath(chart.ChartFile) == "Assets/Beatmaps/Level1.json", "ChartLoading uses Level1.json");
        Check(timeline.Count(n => n.IsCheckpoint) == 1 && timeline.Single(n => n.IsCheckpoint).Time == 33, "One checkpoint event at 33 seconds");
        Check(Math.Abs(timeline.First().Time - 2) < .001 && Math.Abs(timeline.Last().Time - 63.75) < .001, "Playable range 2 to 63.75 seconds");
        Check(timeline.FindIndex(n => n.IsCheckpoint) == 32, "Checkpoint is timeline index 32 including checkpoint (zero based)");
        Check(Vector3.Distance(GameObject.Find("Checkpoint(Clone)").transform.position, new Vector3(0,-.4f,495)) < .01f, "Marker aligned at Z495");
        Check(!Manager.HasCheckpoint && Audio.SongOffset == 0, "Fresh run starts without checkpoint");
        DisableFallsForInputProbe();
        for (var w = Movement(); w.MoveNext();) yield return null;
        Menu.Open();
        int before = events;
        KeyState(Key.A);
        Check(events == before && Time.timeScale == 0 && AudioListener.pause, "Settings intentionally blocks gameplay input");
        KeyState(); Menu.Close(); yield return null; yield return null;
        for (var w = Movement(); w.MoveNext();) yield return null;
        Player.GetComponent<PlayerFallDetector>().enabled = true;
        Player.transform.position = new Vector3(0,-10,10);
        yield return null; yield return null; yield return null;
        Check(!Manager.HasCheckpoint && Audio.SongOffset == 0, "Fall before checkpoint restarts from beginning");
        DisableFallsForInputProbe();
        Player.InputHandler.OnInputPressed += _ => events++;
        for (var w = Movement(); w.MoveNext();) yield return null;
        // Approach checkpoint using the actual clock event, not a direct manager invocation.
        Player.transform.position = new Vector3(0,1,493.5f);
        Audio.ScheduleFrom(32.9); Clock.Seek(32.9); Notes.Seek(32.9);
        Manager.OnCheckpointActivated.AddListener(() => activations++);
        while (!Manager.HasCheckpoint) yield return null;
        Check(Manager.CheckpointTime == 33 && activations == 1, "Clock activates midpoint checkpoint exactly once");
        Check(GameObject.Find("Checkpoint Feedback") != null, "Discovery feedback shown");
        for (var w = Wait(.1f); w.MoveNext();) yield return null;
        Check(activations == 1, "Discovery does not repeat each frame");
        for (int attempt = 0; attempt < 2; attempt++)
        {
            Player.GetComponent<PlayerFallDetector>().enabled = true;
            Player.transform.position = new Vector3(0,-10,550);
            yield return null; yield return null; yield return null;
            Check(Manager.HasCheckpoint && Audio.SongOffset == 33, "Fall restores midpoint and audio offset");
            Check(Vector3.Distance(Player.transform.position,new Vector3(0,1,495)) < .1f, "Checkpoint pose restored at center lane Z495");
            Check(Player.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude < .01f, "Checkpoint velocity cleared");
            Physics.SyncTransforms();
            Check(Physics.Raycast(Player.transform.position, Vector3.down, 2f), "Solid track below checkpoint spawn");
            Check(GameObject.Find("Checkpoint Feedback") == null, "Restore does not replay discovery feedback");
            foreach (var source in Audio.StemSources)
                Check(Math.Abs(source.timeSamples - 33 * source.clip.frequency) <= 1, "Stem sample matches checkpoint time");
            Check(Clock.NextNote.HasValue && Clock.NextNote.Value.Time >= 33, "Earlier clock events skipped");
            Notes.OnJudgement += (n,i,j) => { if(n.Time < 33) earlierNotes++; };
            DisableFallsForInputProbe();
            Player.InputHandler.OnInputPressed += _ => events++;
            for (var w = Movement(); w.MoveNext();) yield return null;
            Check(earlierNotes == 0, "Earlier notes never replay after restart");
            Check(Math.Abs(Clock.Receptor - (33 + AudioSettings.dspTime - Audio.ScheduledStartTime - PlayerPrefs.GetFloat("LatencyCompensation",0))) < .05, "DSP timeline agrees with seek offset");
        }
        Menu.Open(); Menu.Restart(); yield return null; yield return null;
        Check(Manager.HasCheckpoint && Audio.SongOffset == 33 && Time.timeScale == 1 && !AudioListener.pause, "Manual Settings restart restores checkpoint and pause state");
        DisableFallsForInputProbe();
        Player.InputHandler.OnInputPressed += _ => events++;
        for (var w = Movement(); w.MoveNext();) yield return null;
        for (var w = Reload(true); w.MoveNext();) yield return null;
        Check(!Manager.HasCheckpoint && Audio.SongOffset == 0, "Ordinary scene load starts fresh");
    }
}
