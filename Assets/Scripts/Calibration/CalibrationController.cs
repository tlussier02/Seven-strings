using UnityEngine;
using UnityEngine.UI;

public class CalibrationController : MonoBehaviour
{
    public GameObject AutoCalibrationPanel;
    public GameObject MainCalibrationPanel;
    public Text LatencyCompensationText;
    
    private CalibrationInputHandler CalibrationInputHandler;
    private CalibrationBeatVisual CalibrationBeatVisual;

    private float LatencyCompensation;
    private float CompensationStep = 0.01f;

    void Awake()
    {
        CalibrationInputHandler = GetComponent<CalibrationInputHandler>();
        CalibrationBeatVisual =  GetComponent<CalibrationBeatVisual>();
        
        ShowMainCalibrationPanel();
        UpdateCompensationText();
    }

    void Update()
    {
        CalibrationInputHandler.Tick();
    }

    void OnEnable()
    {
        CalibrationInputHandler.OnInputPressed += HandleInput;
    }
    
    void OnDiable()
    {
        CalibrationInputHandler.OnInputPressed -= HandleInput;
    }
    
    public void Exit()
    {
        
    }

    public void Back()
    {
        ShowMainCalibrationPanel();
        CalibrationBeatVisual.ResetAutoCalibration();
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
    
    public void ShowMainCalibrationPanel()
    {
        MainCalibrationPanel.SetActive(true);
        AutoCalibrationPanel.SetActive(false);
        CalibrationInputHandler.CanHandleInput = false;
        
        // start beat visual and audio looping
        // update calibration value (pull from ReceptorClock? or just store copy since new scene)
            // when updating actual latency compensation value in receptor clock,
            // since it is in other scene we would need a player data json or txt or something to store that value
    }
    
    public void ShowAutoCalibrationPanel()
    {
        AutoCalibrationPanel.SetActive(true);
        MainCalibrationPanel.SetActive(false);
        CalibrationInputHandler.CanHandleInput = true;
    }

    private void HandleInput()
    {
        CalibrationBeatVisual.FlashInput();
        CalibrationBeatVisual.IncreaseProgress();
    }
    
    private void UpdateCompensationText()
    {
        LatencyCompensationText.text = LatencyCompensation.ToString("0.00 ms");
    }
}
