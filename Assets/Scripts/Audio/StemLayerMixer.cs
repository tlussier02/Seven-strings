using UnityEngine;

// Fades are lined up to the song's beats/bars (the "grid") so layers enter and leave in time with the music
public class StemLayerMixer : MonoBehaviour
{
    public enum Grid { Sixteenth, Eighth, Beat, HalfBar, Bar }
    
    [Header("References")]
    [SerializeField] private ScheduledAudioPlayer player;
    [SerializeField] private HarmonyMeter harmonyMeter;
    [Tooltip("Optional. If set, BPM is read from the loaded chart.")]
    [SerializeField] private ChartLoader chartLoader;

    [Header("Tempo")]
    [SerializeField, Min(1f)] private float fallbackBpm = 120f;
    [SerializeField, Min(1)] private int beatsPerBar = 4;
    [Tooltip("Seconds from the start of the audio file to beat 1 (0 if the stems start on the downbeat).")]
    [SerializeField, Min(0f)] private float firstBeatOffset = 0f;

    [Header("Layer thresholds (normalized meter, one per stem)")]
    [Tooltip("Meter value at which each stem turns on. Stem 0 = 0 means always on.")]
    [SerializeField] private float[] stemThresholds = { 0f, 0.35f, 0.75f };
    [Tooltip("A stem only turns back off once the meter falls this far BELOW its threshold. " +
             "Stops a stem flickering when the meter hovers around 35% / 75%.")]
    [SerializeField, Range(0f, 0.3f)] private float hysteresis = 0.10f;

    [Header("Fade in")]
    [SerializeField] private Grid fadeInGrid = Grid.Beat;
    [SerializeField, Min(0.01f)] private float fadeInBeats = 1f;

    [Header("Fade out")]
    [SerializeField] private Grid fadeOutGrid = Grid.Bar;
    [SerializeField, Min(0.01f)] private float fadeOutBeats = 2f;

    [Header("Debug")]
    [SerializeField] private bool logTransitions = false;

    private class StemState
    {
        public bool TargetOn;
        public float Gain;
        public bool Fading;
        public float From, To;
        public double Start, End;
    }

    private StemState[] stems;

    double SecondsPerBeat
    {
        get
        {
            float bpm = chartLoader != null && chartLoader.Bpm > 0f ? chartLoader.Bpm : fallbackBpm;
            return 60.0 / bpm;
        }
    }

    // DSP time of the song's beat 1; Grid lines are counted from here
    double SongZeroDsp => player.ScheduledStartTime - player.SongOffset + firstBeatOffset;

    void OnEnable()  => harmonyMeter.OnHarmonyChanged += HandleHarmonyChanged;
    void OnDisable() => harmonyMeter.OnHarmonyChanged -= HandleHarmonyChanged;

    void Start()
    {
        stems = new StemState[player.StemCount];
        for (int i = 0; i < stems.Length; i++)
        {
            bool on = harmonyMeter.Normalized >= ThresholdFor(i);
            stems[i] = new StemState { TargetOn = on, Gain = on ? 1f : 0f };
            player.SetStemGain(i, stems[i].Gain);
        }
    }

    void Update()
    {
        if (stems == null)
            return;

        // DSP time stops while the game is paused, so fades pause too.
        double now = AudioSettings.dspTime;

        for (int i = 0; i < stems.Length; i++)
        {
            StemState s = stems[i];
            if (!s.Fading || now < s.Start)
                continue; // not fading, or still waiting for its grid line

            float t = (float)((now - s.Start) / (s.End - s.Start));
            if (t >= 1f)
            {
                s.Gain = s.To;
                s.Fading = false;
            }
            else
            {
                s.Gain = Mathf.Lerp(s.From, s.To, Mathf.SmoothStep(0f, 1f, t));
            }
            player.SetStemGain(i, s.Gain);
        }
    }

    private void HandleHarmonyChanged(int current, int max)
    {
        if (stems == null)
            return;

        float normalized = (float)current / max;
        for (int i = 0; i < stems.Length; i++)
        {
            float threshold = ThresholdFor(i);
            bool on = stems[i].TargetOn
                ? normalized >= threshold - hysteresis   // on: stays on until the meter drops below threshold - hysteresis
                : normalized >= threshold;               // off: turns on once the meter reaches the threshold
            if (threshold <= 0f) on = true;              // stem 0 (base layer) is always on

            if (on != stems[i].TargetOn)
                Retarget(i, on);
        }
    }

    private void Retarget(int i, bool on)
    {
        StemState s = stems[i];
        s.TargetOn = on;
        float to = on ? 1f : 0f;
        double now = AudioSettings.dspTime;

        // Music hasn't started yet = no fade.
        if (!player.IsRunning || now < SongZeroDsp)
        {
            s.Gain = to;
            s.Fading = false;
            player.SetStemGain(i, to);
            return;
        }

        if (s.Fading)
        {
            if (now < s.Start)
            {
                // cancel the fade
                s.Fading = false;
                return;
            }

            // Changed direction mid-fade: reverse right away from the current volume
            // instead of waiting for another beat
            float beats = on ? fadeInBeats : fadeOutBeats;
            double duration = Mathf.Abs(to - s.Gain) * beats * SecondsPerBeat;
            BeginFade(s, s.Gain, to, now, now + Mathf.Max(0.01f, (float)duration));
            return;
        }

        double spb = SecondsPerBeat;
        if (on)
        {
            // Fade starts on the next grid line.
            double start = NextBoundary(now, GridBeats(fadeInGrid) * spb);
            BeginFade(s, s.Gain, 1f, start, start + fadeInBeats * spb);
        }
        else
        {
            double start = NextBoundary(now, GridBeats(fadeOutGrid) * spb);
            BeginFade(s, s.Gain, 0f, start, start + fadeOutBeats * spb);
        }

        if (logTransitions)
            Debug.Log($"[StemLayerMixer] stem{i} -> {(on ? "IN" : "OUT")} at dsp {s.Start:F3} (now {now:F3})");
    }

    private static void BeginFade(StemState s, float from, float to, double start, double end)
    {
        s.From = from;
        s.To = to;
        s.Start = start;
        s.End = end;
        s.Fading = true;
    }

    // The next grid line after dspTime (as a DSP time)
    private double NextBoundary(double dspTime, double gridLen)
    {
        double songPos = dspTime - SongZeroDsp;
        double index = System.Math.Floor(songPos / gridLen) + 1.0;
        return SongZeroDsp + index * gridLen;
    }
    
    private float GridBeats(Grid grid) => grid switch
    {
        Grid.Sixteenth => 0.25f,
        Grid.Eighth    => 0.5f,
        Grid.Beat      => 1f,
        Grid.HalfBar   => beatsPerBar * 0.5f,
        _              => beatsPerBar
    };

    private float ThresholdFor(int stemIndex) =>
        stemIndex < stemThresholds.Length ? stemThresholds[stemIndex] : 1f;
}
