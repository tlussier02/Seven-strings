using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class CalibrationBeatVisual : MonoBehaviour
{
    public List<Image> BeatImages;
    public List<Image> ProgressImages;
    public Image InputImage;
    public Text ProgressText;
    public Text OvertimeText;

    public Color IdleColor;

    private int CurrentProgress;
    private Coroutine ProgressFlashRoutine;
    private Coroutine InputFlashRoutine;

    public void ResetAutoCalibration()
    {
        CurrentProgress = 0;
        ResetProgressImages();
        UpdateProgressText();
        UpdateOvertimeText(0.00f);
    }

    public void FlashInput()
    {
        InputFlashRoutine = StartCoroutine(FlashImage(InputImage, Color.white, IdleColor, 0.08f, 0.3f));
    }

    public void IncreaseProgress()
    {
        if (CurrentProgress >= ProgressImages.Count)
            return;
        ProgressFlashRoutine = StartCoroutine(FlashImage(ProgressImages[CurrentProgress], Color.white, Color.darkBlue, 0.1f, 0.5f));
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
            image.color = IdleColor;
        }
    }

    private void UpdateProgressText()
    {
        ProgressText.text = CurrentProgress.ToString($"0/{ProgressImages.Count}");
    }
    
    private void ResetBeatImages()
    {
        foreach (var image in BeatImages)
        {
            image.GetComponent<Image>().color = IdleColor;
        }

        CurrentProgress = 0;
    }
    
    private IEnumerator FlashImage(Image targetImage, Color startColor, Color endColor, float flashDuration, float fadeDuration)
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
