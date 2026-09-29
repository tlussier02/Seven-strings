using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BeatVisual : MonoBehaviour
{
    public Image InputPanelImage;
    public Image BeatPanelImage;
    public Image AccuracyPanelImage;
    public Text AccuracyText;

    public Color idleColor = Color.gray;

    private Coroutine inputFlashRoutine;
    private Coroutine beatFlashRoutine;

    public void FlashInput()
    {
        if (inputFlashRoutine != null) StopCoroutine(inputFlashRoutine);
        inputFlashRoutine = StartCoroutine(Flash(InputPanelImage, Color.cornflowerBlue, 0.1f));
    }

    public void FlashBeat()
    {
        if (beatFlashRoutine != null) StopCoroutine(beatFlashRoutine);
        beatFlashRoutine = StartCoroutine(Flash(BeatPanelImage, Color.lightCoral, 0.1f));
    }

    public void DisplayAccuracy(string accuracyText, Color color)
    {
        AccuracyPanelImage.color = color;
        AccuracyText.text = accuracyText;
    }

    private IEnumerator Flash(Image target, Color color,  float duration)
    {
        target.color = color;
        yield return new WaitForSeconds(duration);
        target.color = idleColor;
    }
    
}