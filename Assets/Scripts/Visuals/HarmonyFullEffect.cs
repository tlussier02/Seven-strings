using UnityEngine;

public class HarmonyFullEffect : MonoBehaviour
{
    [SerializeField] private HarmonyMeter harmonyMeter;
    
    [SerializeField] private ParticleSystem auraParticles;
    
    [SerializeField] private ParticleSystem burstParticles;
    
    [SerializeField, Range(0f, 1f)] private float exitThreshold = 0.8f;

    private bool isActive;

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
        auraParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        HandleHarmonyChanged(harmonyMeter.Current, harmonyMeter.Max);
    }

    private void HandleHarmonyChanged(int current, int max)
    {
        float normalized = (float)current / max;

        if (!isActive && current >= max)
            SetActive(true);
        else if (isActive && normalized < exitThreshold)
            SetActive(false);
    }

    private void SetActive(bool active)
    {
        isActive = active;

        if (active)
        {
            auraParticles.Play();
            if (burstParticles != null)
                burstParticles.Play();
        }
        else
        {
            auraParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
