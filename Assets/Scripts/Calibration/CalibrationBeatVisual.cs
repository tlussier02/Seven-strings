using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class CalibrationBeatVisual : MonoBehaviour
{
    public List<GameObject> BeatImages;
    public List<Image> ProgressImages;
    public Text ProgressText;
    public Text OvertimeText;
    
    public Color idleColor = Color.gray;

    private int CurrentProgress;
    private Coroutine ProgressFlashRoutine;

    public void ResetAutoCalibration()
    {
        CurrentProgress = 0;
        ResetProgressImages();
        UpdateProgressText();
        UpdateOvertimeText(0.00f);
    }

    public void IncreaseProgress()
    {
        if (CurrentProgress >= ProgressImages.Count)
            return;
        ProgressFlashRoutine = StartCoroutine(FlashProgressPanel(ProgressImages[CurrentProgress], Color.white, Color.darkBlue, 0.1f, 0.5f));
        CurrentProgress += 1;
        UpdateProgressText();
    }

    public void UpdateOvertimeText(float overtime)
    {
        OvertimeText.text = overtime.ToString("0.00 ms");
    }

    private void ResetProgressImages()
    {
        foreach (var image in ProgressImages)
        {
            image.color = idleColor;
        }
    }

    private void UpdateProgressText()
    {
        ProgressText.text = CurrentProgress.ToString($"0/{ProgressImages.Count}");
    }
    
    private void ResetBeatImages()
    {
        foreach (var panel in BeatImages)
        {
            panel.GetComponent<Image>().color = idleColor;
        }

        CurrentProgress = 0;
    }
    
    private IEnumerator FlashProgressPanel(Image targetImage, Color startColor, Color endColor, float flashDuration, float fadeDuration)
    {
        float elapsedTime = 0;
        
        targetImage.color = startColor;
        yield return new WaitForSeconds(flashDuration);
        
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration; 
            
            targetImage.color = Color.Lerp(startColor, endColor, t);
            
            yield return null;
        }

        targetImage.color = endColor;
    }
}
