using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public static class CheckpointValidation
{
    static int stage;
    static double until;
    static int frame;
    static int restarts;
    static int oldJudgements;
    static bool hooked;
    public static void Run()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene("Assets/Scenes/ChartLoading.unity");
        EditorApplication.update += Tick;
        until = EditorApplication.timeSinceStartup + 180;
        EditorApplication.isPlaying = true;
    }
    static void Check(bool ok, string message)
    {
        if (!ok) throw new Exception(message);
        Debug.Log("CHECK PASS: " + message);
    }
    static void ValidateSettings(RhythmController rhythm)
    {
        var menu = UnityEngine.Object.FindFirstObjectByType<SettingsMenuController>();
        Canvas.ForceUpdateCanvases();
        var button = GameObject.Find("Settings (Esc)").GetComponent<Button>();
        Check(button.IsActive() && button.IsInteractable(), "Settings button visible and interactable");
        var pointer = new PointerEventData(EventSystem.current) {
            position = RectTransformUtility.WorldToScreenPoint(null, button.transform.position) };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button,
            "Settings button receives the top UI raycast");
        menu.Open();
        Check(menu.IsOpen && Time.timeScale == 0 && AudioListener.pause, "Settings pauses gameplay");
        var sliders = UnityEngine.Object.FindObjectsByType<Slider>(FindObjectsSortMode.None);
        Check(sliders.Length == 3, "Settings has Master, Music and SFX sliders");
        var slider = sliders.Single(x => x.name == "Sound Effects Volume Slider");
        bool hadPreference = PlayerPrefs.HasKey(AudioVolumeSettings.SfxVolumeKey);
        float original = AudioVolumeSettings.SfxVolume;
        float music = rhythm.GetComponent<ScheduledAudioPlayer>().StemSources[0].volume;
        slider.value = 23;
        var source = rhythm.GetComponent<SfxPlayer>().transform.Find("SfxSource").GetComponent<AudioSource>();
        Check(Math.Abs(source.volume - .23f) < .001f && Math.Abs(AudioVolumeSettings.SfxVolume - .23f) < .001f,
            "SFX slider updates its source and saved preference");
        Check(rhythm.GetComponent<ScheduledAudioPlayer>().StemSources[0].volume == music,
            "SFX slider leaves music volume unchanged");
        slider.value = Mathf.Round(original * 100);
        menu.Close();
        if (hadPreference) PlayerPrefs.SetFloat(AudioVolumeSettings.SfxVolumeKey, original);
        else PlayerPrefs.DeleteKey(AudioVolumeSettings.SfxVolumeKey);
        PlayerPrefs.Save();
        Check(!menu.IsOpen && Time.timeScale == 1 && !AudioListener.pause, "Resume releases pause");
    }

    static void Tick()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > until) throw new Exception("Validation timeout, stage " + stage);
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            var manager = UnityEngine.Object.FindFirstObjectByType<CheckpointManager>();
            var rhythm = UnityEngine.Object.FindFirstObjectByType<RhythmController>();
            if (player == null || manager == null || rhythm == null) return;
            var audio = rhythm.GetComponent<ScheduledAudioPlayer>();
            var clock = rhythm.GetComponent<ReceptorClock>();
            var notes = rhythm.GetComponent<NoteManager>();
            var fall = player.GetComponent<PlayerFallDetector>();
            if (stage == 0)
            {
                Check(!manager.HasCheckpoint && audio.SongOffset == 0, "Fresh run has no checkpoint");
                ValidateSettings(rhythm);
                stage = 1; frame = Time.frameCount;
                GameplayReset.RequestRestart(); return;
            }
            if (Time.frameCount <= frame + 2) return;
            if (stage == 1)
            {
                Check(audio.SongOffset == 0 && !manager.HasCheckpoint, "Pre-checkpoint restart begins at zero");
                fall.enabled = false;
                player.GetComponent<Rigidbody>().useGravity = false;
                player.transform.position = new Vector3(0,1,148.5f);
                audio.ScheduleFrom(9.9); clock.Seek(9.9); notes.Seek(9.9);
                stage = 2;
            }
            if (stage == 2 && manager.HasCheckpoint)
            {
                Check(manager.CheckpointTime == 10, "Chart checkpoint activates at 10 seconds");
                Check(GameObject.Find("Checkpoint Feedback").GetComponent<Canvas>().isActiveAndEnabled, "Checkpoint discovery feedback is visible");
                until = EditorApplication.timeSinceStartup + 90;
                stage = 3;
            }
            if (stage == 3 && clock.Receptor >= 15)
            {
                // Exercise automatic failure well past the checkpoint.
                fall.enabled = true;
                player.transform.position = new Vector3(0,-10,225);
                stage = 4; frame = Time.frameCount; hooked = false;
                return;
            }
            if (stage == 4)
            {
                Check(manager.HasCheckpoint && audio.SongOffset == 10, "Fall restores active checkpoint and audio offset");
                Check(GameObject.Find("Checkpoint Feedback") == null, "Restored checkpoint does not repeat discovery feedback");
                ValidateSettings(rhythm);
                Check(Vector3.Distance(player.transform.position,new Vector3(0,1,150)) < .1f, "Safe authored checkpoint position restored");
                Check(player.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude < .01f, "Rigidbody velocity cleared");
                Check(Physics.Raycast(player.transform.position, Vector3.down, player.GetComponent<Collider>().bounds.extents.y + 0.2f), "Ground exists below authored spawn");
                foreach (var source in audio.StemSources)
                    Check(Math.Abs(source.timeSamples - 10 * source.clip.frequency) <= 1, "Stem seeks to checkpoint sample before playback");
                Check(rhythm.GetComponent<HarmonyMeter>().Current == 0, "Harmony reset to starting value");
                Check(UnityEngine.Object.FindObjectsByType<ScheduledAudioPlayer>(FindObjectsSortMode.None).Length == 1, "One audio manager after restart");
                Check(audio.transform.GetComponentsInChildren<AudioSource>().Count(source => source.name.StartsWith("Stem")) == audio.StemCount, "No duplicate stem sources");
                fall.enabled = false; player.GetComponent<Rigidbody>().useGravity = false;
                if (!hooked) { notes.OnJudgement += (n,i,j) => { if(n.Time < 10) oldJudgements++; }; hooked = true; }
                stage = 5;
            }
            if (stage == 5 && clock.FoundOvertime && clock.Receptor >= 20)
            {
                Check(Math.Abs(clock.Receptor - (10 + AudioSettings.dspTime - audio.ScheduledStartTime - PlayerPrefs.GetFloat("LatencyCompensation",0))) < .1, "DSP chart clock agrees with audio offset");
                Check(oldJudgements == 0, "Earlier notes never replay");
                if (++restarts < 2)
                {
                    stage = 3; return;
                }
                stage = 6; frame = Time.frameCount;
                var menu = UnityEngine.Object.FindFirstObjectByType<SettingsMenuController>();
                menu.Open();
                Check(menu.IsOpen && Time.timeScale == 0 && AudioListener.pause, "Settings opens and pauses");
                menu.Restart(); return;
            }
            if (stage == 6)
            {
                Check(Time.timeScale == 1 && !AudioListener.pause && audio.SongOffset == 10, "Paused manual restart restores checkpoint and unpauses");
                stage = 7; frame = Time.frameCount;
                UnityEngine.SceneManagement.SceneManager.LoadScene("Assets/Scenes/ChartLoading.unity"); return;
            }
            if (stage == 7)
            {
                Check(!manager.HasCheckpoint && audio.SongOffset == 0, "Ordinary fresh scene load clears checkpoint");
                Debug.Log("CHECKPOINT VALIDATION PASSED");
                EditorApplication.update -= Tick;
                EditorApplication.Exit(0);
            }
        }
        catch(Exception ex)
        {
            Debug.LogException(ex); EditorApplication.update -= Tick; EditorApplication.Exit(1);
        }
    }
}
