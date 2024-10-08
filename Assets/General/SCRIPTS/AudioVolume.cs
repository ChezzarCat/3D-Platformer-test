using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioVolume : MonoBehaviour
{
    public AudioMixer audioMixerMaster;
    private float volumeSaved = 100;

    void Start()
    {
        volumeSaved = PlayerPrefs.GetFloat("volume", 100f);
    }

    void Update()
    {
        volumeSaved = PlayerPrefs.GetFloat("volume", 100f);

        // Apply a logarithmic conversion to make the volume feel more natural to the human ear
        float volumeLinear = volumeSaved / 100f;
        float volumeInDb = Mathf.Log10(Mathf.Max(volumeLinear, 0.0001f)) * 20f; // Convert to decibels
        audioMixerMaster.SetFloat("volume", volumeInDb);
    }
}
