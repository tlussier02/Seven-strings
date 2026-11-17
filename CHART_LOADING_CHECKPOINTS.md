ChartLoading input and midpoint checkpoint — current local project

Chart source and timing

ChartLoading's ChartLoader references Assets/Beatmaps/Level1.json (GUID d8068a1a905d044638864ee0449b56f0). It loads charts[0] directly; it does not select the CSV, beatmap-test.json, or movement-checkpoints.json at runtime. songPos is milliseconds; ChartLoader divides by 1000. BPM is 120. First playable event is 2.000 seconds and last playable event is 63.750 seconds, a 61.750-second event span. Audio duration is separate from the playable event span.

Temporal midpoint = 2 + (63.75 - 2) / 2 = 32.875 seconds. The existing checkpoint link (663f746e-e71d-47f7-bb23-7210c053fc41 to 0b026c71-69a6-4807-ad27-87d7849d7f60) moved from 44.500 seconds / beat 89 to 33.000 seconds / beat 66. This is the nearest whole beat, 0.125 seconds from the midpoint, on solid center-lane track before the jump at 33.500 seconds. Its existing IDs/schema are preserved; there is one checkpoint. It is event index 32 in the sorted runtime timeline (zero based, including the checkpoint), with 32 earlier gameplay events.

Physical alignment

CheckpointManager derives the spawn from the same straight-track mapping as LevelTools: initial player position + forward * (song time * speed) + right * ((lane - 1) * lane width). The preceding chart lane changes return to lane 1 (center, zero based); speed is 15 and width is 3.5. Thus the spawn is (0, 1, 495), facing forward. The existing scene marker moved to (0, -0.4, 495), preserving its presentation height and full-track width. Track generation already places solid center-lane tiles there; regeneration is unnecessary. No prototype scenes or alternate chart files were changed by this task.

Checkpoint delivery fix

ChartLoader previously appended checkpoints only to Checkpoints and then continued without adding their NoteData to its returned timeline. CheckpointManager consumes LoadChart for spawn construction and restore, and subscribes to ReceptorClock.OnBeat for discovery. All three paths therefore lacked checkpoint events. LoadChart now retains checkpoint NoteData in the clock timeline while still populating Checkpoints for LevelTools markers. NoteManager already excludes IsCheckpoint events from judgement. Existing HasCheckpoint/CheckpointTime guards prevent repeated discovery feedback on frame progression or checkpoint restart. GameplayReset and the DSP scheduling implementation remain unchanged.

Input findings and limited defensive change

The supplied current ChartLoading files did not reproduce permanent movement loss. An isolated Unity 6000.6.8f1 baseline run, before PlayerController changes, confirmed a synthetic D key reaches InputHandler, emits Right, and moves the player from X0 to X3.5. There is one enabled InputHandler, correctly assigned to the active, enabled PlayerController and shared with RhythmController. Awake resolves references before OnEnable subscribes. No current ChartLoading null-reference failure was observed. Historical Logs/Editor.log contains MovementPrototype failures in PlayerController.Awake from a missing LaneSegmentPrefab, followed by a null unsubscribe, and invalid JSON during RhythmController.Awake; those logs do not establish a failure in today's ChartLoading scene.

Input was unblocked, Time.timeScale was 1, and AudioListener.pause was false during the baseline test. ReceptorClock.FoundOvertime became true after the scheduled lead-in and forward movement continued. Countdown intentionally blocks input during lead-in; Settings blocks it via time/audio pause and suppresses the close frame. No stuck blocker was found. InputHandler polls keyboard bindings (A left, D right, Space jump, S roll); it has no gameplay gamepad path. Roll remains intentionally empty.

PlayerController now resolves a missing InputHandler from its assigned ReceptorClock before using a global fallback, derives a missing ReceptorClock from the assigned input object, reports a missing input dependency, remembers the handler actually subscribed, safely unsubscribes, and guards an absent clock in Update. Existing lane bounds, lane tween, jump tween, checkpoint restore, and fall behavior are preserved. These are defensive changes, not a demonstrated cure for a reproducible current-scene input failure. No InputHandler, RhythmController, SettingsMenuController, countdown, or reset changes were necessary.

Validation

Use Tools/Validation/ChartLoadingInputValidation.cs for the updated chart. Copy it into Assets/Editor in an isolated project copy and launch Unity 6000.6.8f1 with -batchmode -nographics -executeMethod ChartLoadingInputValidation.Run (without -quit). The earlier CheckpointValidation.cs harness targets obsolete 10-second assumptions and should not be used to validate the updated chart.

The new harness configures headless Editor input routing only in the isolated copy, injects keyboard state through the Input System, and calls the actual InputHandler.Tick polling/event path deterministically. Player tweens, forward movement, countdown, rhythm clock, checkpoint discovery, falls, scene reload, and audio seek run in Play mode. This does not establish physical keyboard/controller focus behavior, rendered appearance, or audible synchronization in the interactive Editor.

Manual follow-up: open ChartLoading, focus the Game view, wait for the countdown, and use A/D/Space. Open/close Settings and repeat. Follow the chart through beat 66, confirm one notification, then fall and confirm the center-lane respawn at Z495 with the song at 33 seconds. Repeat a fall and Settings > Restart; discovery should not replay. An ordinary scene load must start from the beginning. No Inspector setup is required for the current ChartLoading scene. If movement still fails interactively, capture the current Console error and Game-view focus state; the current local baseline did not reproduce it.

Automated result: Unity 6000.6.8f1 exited 0 with CHARTLOADING INPUT AND CHECKPOINT VALIDATION PASSED. Log: /tmp/seven-strings-input-checkpoint-validation.log. Left/right/jump passed at initial start, after Settings close, after a beginning restart, after each of two checkpoint falls, and after a paused manual restart. Additional assertions passed for active scene/chart, event index, marker alignment, single discovery, feedback suppression on restore, ground beneath spawn, cleared velocity, exact stem seek samples, DSP seek agreement, skipped earlier notes, and fresh-run clearing. No runtime errors or NullReferenceExceptions were logged. git diff --check passed.

Files changed in this task: Assets/Scripts/Gameplay/PlayerController.cs (input dependency/subscription guards); Assets/Scripts/Chart/ChartLoader.cs (checkpoint event delivery); Assets/Beatmaps/Level1.json (existing checkpoint timing); Assets/Scenes/ChartLoading.unity (existing marker position); CHART_LOADING_CHECKPOINTS.md (this report); Tools/Validation/ChartLoadingInputValidation.cs (new isolated Play-mode validation). Other staged and untracked files predate this task and were preserved. No files were staged, committed, or pushed; the branch remains retarst-test.

Full git status at completion:

```text
On branch retarst-test
Changes to be committed:
  (use "git restore --staged <file>..." to unstage)
	modified:   Assets/Beatmaps/beatmap-test.json
	modified:   Assets/Scenes/ChartLoading.unity
	modified:   Assets/Scenes/MovementPrototype.unity
	modified:   Assets/Scripts/Audio/ScheduledAudioPlayer.cs
	modified:   Assets/Scripts/Audio/SfxPlayer.cs
	modified:   Assets/Scripts/Audio/StemLayerMixer.cs
	modified:   Assets/Scripts/Gameplay/GameplayReset.cs
	modified:   Assets/Scripts/Gameplay/NoteManager.cs
	modified:   Assets/Scripts/Gameplay/PlayerController.cs
	modified:   Assets/Scripts/Gameplay/PlayerFallDetector.cs
	modified:   Assets/Scripts/Gameplay/ReceptorClock.cs
	modified:   Assets/Scripts/Tools/DevTools.cs
	modified:   Assets/Scripts/Tools/LevelTools.cs
	modified:   Assets/Scripts/UI/AudioVolumeSettings.cs
	modified:   Assets/Scripts/UI/SettingsMenuController.cs

Changes not staged for commit:
  (use "git add <file>..." to update what will be committed)
  (use "git restore <file>..." to discard changes in working directory)
	modified:   Assets/Beatmaps/Level1.json
	modified:   Assets/Scenes/ChartLoading.unity
	modified:   Assets/Scripts/Chart/ChartLoader.cs
	modified:   Assets/Scripts/Gameplay/PlayerController.cs

Untracked files:
  (use "git add <file>..." to include in what will be committed)
	Assets/Beatmaps/LevelMapping - Test Extended 2.csv
	Assets/Beatmaps/LevelMapping - Test Extended 2.csv.meta
	Assets/Beatmaps/movement-checkpoints.json
	Assets/Beatmaps/movement-checkpoints.json.meta
	Assets/Scripts/Editor 2.meta
	Assets/Scripts/Gameplay/CheckpointManager.cs
	Assets/Scripts/Gameplay/CheckpointManager.cs.meta
	Assets/Scripts/UI/CheckpointFeedback.cs
	Assets/Scripts/UI/CheckpointFeedback.cs.meta
	Assets/Sound/UPDATED_stem0 2.wav
	Assets/Sound/UPDATED_stem0 2.wav.meta
	Assets/Sound/UPDATED_stem1 2.wav
	Assets/Sound/UPDATED_stem1 2.wav.meta
	Assets/Sound/lvl1_stem2 2.wav
	Assets/Sound/lvl1_stem2 2.wav.meta
	CHART_LOADING_CHECKPOINTS.md
	CHECKPOINT_RESTART.md
	Tools/

```
