using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class HourController : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI timeTextShadow;

    void Start()
    {
        UpdateTime();
    }

    void Update()
    {
        if (Time.frameCount % 60 == 0)
        {
            UpdateTime();
        }
    }

    void UpdateTime()
    {
        DateTime now = DateTime.Now;
        string formattedTime = now.ToString("HH:mm");
        timeText.text = formattedTime;
        timeTextShadow.text = formattedTime;
    }
}
