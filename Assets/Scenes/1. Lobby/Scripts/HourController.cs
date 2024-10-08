using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HourController : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Secret"))
            InvokeRepeating("UpdateTime", 0f, 0.01f);
        else if (sceneName.StartsWith("Lobby"))
            InvokeRepeating("UpdateTime", 0f, 0.15f);
        else
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
