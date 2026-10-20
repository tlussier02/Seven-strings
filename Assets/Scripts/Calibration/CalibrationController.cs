using Unity.VectorGraphics;
using UnityEngine;

public class CalibrationController : MonoBehaviour
{
    public GameObject AutoCalibrationPanel;
    public GameObject MainCalibrationPanel;

    public void Awake()
    {
        MainCalibrationPanel.SetActive(true);
        AutoCalibrationPanel.SetActive(false);
    }
    
    public void Exit()
    {
        
    }

    public void Back()
    {
        ShowMainCalibrationPanel();
    }
    
    public void ShowMainCalibrationPanel()
    {
        MainCalibrationPanel.SetActive(true);
        AutoCalibrationPanel.SetActive(false);
        // start beat visual and audio looping
        // update calibration value (pull from ReceptorClock? or just store copy since new scene)
            // when updating actual latency compensation value in receptor clock,
            // since it is in other scene we would need a player data json or txt or something to store that value
    }
    
    public void ShowAutoCalibrationPanel()
    {
        AutoCalibrationPanel.SetActive(true);
        MainCalibrationPanel.SetActive(false);
    }
}
