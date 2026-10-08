using UnityEngine;
using UnityEngine.UI;

// Draws a HarmonyMeter as a filled bar. Only reads from the meter, never changes it.
public class HarmonyMeterView : MonoBehaviour
{
    [SerializeField] private HarmonyMeter harmonyMeter;
    // Image Type "Sliced" (recommended): the bar resizes so rounded ends stay crisp.
    // Image Type "Filled": the bar is cropped with fillAmount (rounded ends get squashed).
    [SerializeField] private Image fillImage;
    [SerializeField] private Text valueLabel;   // optional, can be left empty

    [Tooltip("Fraction of the bar the fill moves per second. 0 = snap instantly.")]
    [SerializeField, Min(0f)] private float fillSpeed = 1.5f;

    [SerializeField] private bool tintByValue = true;
    [SerializeField] private Gradient fillColor = DefaultGradient();

    private float targetFill;
    private float displayedFill;

    void OnEnable()
    {
        harmonyMeter.OnHarmonyChanged += HandleHarmonyChanged;
    }

    void OnDisable()
    {
        harmonyMeter.OnHarmonyChanged -= HandleHarmonyChanged;
    }

    void Start()
    {
        // Read the starting value directly, then snap so the bar doesn't animate in on load.
        HandleHarmonyChanged(harmonyMeter.Current, harmonyMeter.Max);
        SetFill(targetFill);
    }

    void Update()
    {
        if (Mathf.Approximately(displayedFill, targetFill))
            return;

        SetFill(fillSpeed > 0f
            ? Mathf.MoveTowards(displayedFill, targetFill, fillSpeed * Time.deltaTime)
            : targetFill);
    }

    private void SetFill(float amount)
    {
        displayedFill = amount;

        if (fillImage.type == Image.Type.Filled)
        {
            fillImage.fillAmount = amount;
        }
        else
        {
            // Stretch the fill from the left edge to 'amount' of its parent's width.
            RectTransform rect = fillImage.rectTransform;
            rect.anchorMin = new Vector2(0f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(amount, rect.anchorMax.y);
            // Hide it at zero so a sliver of rounded ends doesn't linger.
            fillImage.enabled = amount > 0.001f;
        }

        UpdateColor();
    }

    private void HandleHarmonyChanged(int current, int max)
    {
        targetFill = (float)current / max;

        if (valueLabel != null)
            valueLabel.text = $"{current} / {max}";
    }

    private void UpdateColor()
    {
        if (tintByValue)
            fillImage.color = fillColor.Evaluate(displayedFill);
    }

    private static Gradient DefaultGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.90f, 0.30f, 0.30f), 0f),   // low: red
                new GradientColorKey(new Color(0.95f, 0.80f, 0.30f), 0.5f), // mid: yellow
                new GradientColorKey(new Color(0.35f, 0.75f, 1.00f), 1f)    // full: blue
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return gradient;
    }
}
