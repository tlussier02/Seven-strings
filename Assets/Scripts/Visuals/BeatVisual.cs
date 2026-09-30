using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BeatVisual : MonoBehaviour
{
    public SpriteRenderer InputPanel;
    public Image BeatPanelImage;
    public Image AccuracyPanelImage;
    public Text AccuracyText;

    public Color idleColor = Color.gray;

    private Coroutine inputFlashRoutine;
    private Coroutine beatFlashRoutine;

    public void FlashInput()
    {
        if (inputFlashRoutine != null) StopCoroutine(inputFlashRoutine);
        inputFlashRoutine = StartCoroutine(FlashSprite(InputPanel, Color.cornflowerBlue, 0.1f));
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
    
    private IEnumerator FlashSprite(SpriteRenderer target, Color color, float duration)
    {
        float originalAlpha = target.color.a;
        color.a = originalAlpha;
        
        Color idle = Color.white;
        idle.a = originalAlpha;

        target.color = color;
        yield return new WaitForSeconds(duration);
        target.color = idle;
    }
    
}