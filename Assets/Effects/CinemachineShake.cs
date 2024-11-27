using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera; // Assign your FreeLook camera here
    private float shakeTimer = 0f;

    public void Start()
    {
        CinemachineBasicMultiChannelPerlin noise = freeLookCamera.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CinemachineBasicMultiChannelPerlin noise2 = freeLookCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CinemachineBasicMultiChannelPerlin noise3 = freeLookCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;

        noise2.m_AmplitudeGain = 0f;
        noise2.m_FrequencyGain = 0f;

        noise3.m_AmplitudeGain = 0f;
        noise3.m_FrequencyGain = 0f;
    }

    // Trigger the shake
    public void TriggerShake(float amplitude, float frequency, float duration)
    {
        // Access the Noise component
        CinemachineBasicMultiChannelPerlin noise = freeLookCamera.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CinemachineBasicMultiChannelPerlin noise2 = freeLookCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CinemachineBasicMultiChannelPerlin noise3 = freeLookCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        // Set noise values
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;
        
        noise2.m_AmplitudeGain = amplitude;
        noise2.m_FrequencyGain = frequency;

        noise3.m_AmplitudeGain = amplitude;
        noise3.m_FrequencyGain = frequency;

        // Set shake duration
        shakeTimer = duration;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            if (shakeTimer <= 0f)
            {
                // Reset noise values
                CinemachineBasicMultiChannelPerlin noise = freeLookCamera.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                CinemachineBasicMultiChannelPerlin noise2 = freeLookCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                CinemachineBasicMultiChannelPerlin noise3 = freeLookCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                noise.m_AmplitudeGain = 0f;
                noise.m_FrequencyGain = 0f;

                noise2.m_AmplitudeGain = 0f;
                noise2.m_FrequencyGain = 0f;

                noise3.m_AmplitudeGain = 0f;
                noise3.m_FrequencyGain = 0f;
            }
        }
    }
}
