using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CalibrationController : MonoBehaviour
{
    public GameObject AutoCalibrationPanel;
    public GameObject MainCalibrationPanel;
    public Text LatencyCompensationText;
    public CalibrationMetronome CalibrationMetronome;
    
    private CalibrationInputHandler CalibrationInputHandler;
    private CalibrationBeatVisual CalibrationBeatVisual;

    private float LatencyCompensation;
    private float CompensationStep = 0.001f;
    private double TotalOffset;
    private int TapCount;
    private const double MaxOffset = 0.2;
    private int BeatCount;
    
    void Awake()
    {
        CalibrationInputHandler = GetComponent<CalibrationInputHandler>();
        CalibrationBeatVisual =  GetComponent<CalibrationBeatVisual>();
        LatencyCompensation = PlayerPrefs.GetFloat("LatencyCompensation", 0f);
        
        ShowMainCalibrationPanel();
        UpdateCompensationText();
    }

    void Start()
    {
        CalibrationMetronome.StartMetronome();
    }
    
    void Update()
    {
        CalibrationInputHandler.Tick();
    }

    void OnEnable()
    {
        CalibrationInputHandler.OnInputPressed += HandleInput;
        CalibrationMetronome.OnBeat += HandleBeat;
    }
    
    void OnDisable()
    {
        CalibrationInputHandler.OnInputPressed -= HandleInput;
        CalibrationMetronome.OnBeat -= HandleBeat;
    }
    
    public void Exit()
    {
        PlayerPrefs.SetFloat("LatencyCompensation", LatencyCompensation);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("MovementPrototype");
    }

    public void Back()
    {
        ShowMainCalibrationPanel();
        CalibrationBeatVisual.ResetAutoCalibration();
        TotalOffset = 0;
        TapCount = 0;
    }

    public void IncreaseCompensation()
    {
        LatencyCompensation += CompensationStep;
        UpdateCompensationText();
    }

    public void DecreaseCompensation()
    {
        LatencyCompensation -= CompensationStep;
        UpdateCompensationText();
    }
    
    private void HandleInput()
    {
        CalibrationBeatVisual.FlashInput();

        if (TapCount >= CalibrationBeatVisual.ProgressImages.Count)
            return;

        double? offset = CalculateOffset();
        if (!offset.HasValue) 
            return;

        TotalOffset += offset.Value;
        TapCount++;
        double average = TotalOffset / TapCount;

        CalibrationBeatVisual.IncreaseProgress();
        CalibrationBeatVisual.UpdateOvertimeText((float)(average * 1000.0));

        if (TapCount >= CalibrationBeatVisual.ProgressImages.Count)
        {
            LatencyCompensation = Mathf.Max(0f, (float)average);
            UpdateCompensationText();
            CalibrationBeatVisual.ProgressText.text = "Done";
        }
    }

    private void HandleBeat()
    {
        CalibrationBeatVisual.FlashBeat(BeatCount++);
    }

    private double? CalculateOffset()
    {
        double t = AudioSettings.dspTime;
        if (t < CalibrationMetronome.FirstBeatTime - CalibrationMetronome.Interval / 2)
            return null;

        double n = Math.Round((t - CalibrationMetronome.FirstBeatTime) / CalibrationMetronome.Interval);
        double offset = t - (CalibrationMetronome.FirstBeatTime + n * CalibrationMetronome.Interval);
        
        return Math.Abs(offset) > MaxOffset ? (double?)null : offset;
    }
    
    public void ShowAutoCalibrationPanel()
    {
        AutoCalibrationPanel.SetActive(true);
        MainCalibrationPanel.SetActive(false);
        CalibrationInputHandler.CanHandleInput = true;
        TotalOffset = 0;
        TapCount = 0;
        CalibrationBeatVisual.ResetAutoCalibration();
    }
    
    private void ShowMainCalibrationPanel()
    {
        MainCalibrationPanel.SetActive(true);
        AutoCalibrationPanel.SetActive(false);
        CalibrationInputHandler.CanHandleInput = false;
    }
    
    private void UpdateCompensationText()
    {
        LatencyCompensation = Mathf.Max(0f, LatencyCompensation);
        CalibrationMetronome.VisualDelay = LatencyCompensation;
        LatencyCompensationText.text = (LatencyCompensation * 1000f).ToString("0") + " ms";
    }
}
