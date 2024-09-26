using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HourController : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    void Start()
    {
        InvokeRepeating("UpdateTime", 0f, 0.15f);
    }

    void UpdateTime()
    {
        // Generate random hours and minutes
        int randomHour = Random.Range(0, 24);
        int randomMinute = Random.Range(0, 60);

        // Format the time in HH:mm
        string formattedTime = randomHour.ToString("00") + ":" + randomMinute.ToString("00");

        timeText.text = formattedTime;
    }
}
