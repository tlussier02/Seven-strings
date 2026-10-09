using UnityEngine;
using UnityEngine.UI;

public class HarmonyMeterView : MonoBehaviour
{
    [SerializeField] private HarmonyMeter harmonyMeter;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text valueLabel;   // optional, can be left empty
    
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
            RectTransform rect = fillImage.rectTransform;
            rect.anchorMin = new Vector2(0f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(amount, rect.anchorMax.y);
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
                new GradientColorKey(new Color(0.90f, 0.30f, 0.30f), 0f),  
                new GradientColorKey(new Color(0.95f, 0.80f, 0.30f), 0.5f),
                new GradientColorKey(new Color(0.35f, 0.75f, 1.00f), 1f)   
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return gradient;
    }
}
