using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BeatVisual : MonoBehaviour
{
    public Image AccuracyPanelImage;
    public Text AccuracyText;

    public Color idleColor = Color.gray;

    private Coroutine textFlashRoutine;

    public void DisplayAccuracy(string accuracyText, Color color)
    {
        if (textFlashRoutine != null) StopCoroutine(textFlashRoutine);
        textFlashRoutine = StartCoroutine(FlashText(AccuracyText, AccuracyPanelImage, color, accuracyText, 0.5f));
    }
    
    private IEnumerator FlashText(Text targetText, Image targetImage, Color color, string text, float duration)
    {
        targetImage.color = color;
        targetText.text = text;
        yield return new WaitForSeconds(duration);
        targetImage.color = idleColor;
        targetText.text = "";
    }
    
}