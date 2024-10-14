using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HourController : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    private int currentHour = 1;
    private int currentMinute = 0;

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Secret"))
            InvokeRepeating("UpdateTime", 0f, 0.01f);

        else if (sceneName.StartsWith("Lobby"))
            InvokeRepeating("UpdateTime", 0f, 0.15f);

        else if (sceneName.StartsWith("Pain"))
        {
            InvokeRepeating("PainTime", 0f, 2f);
            currentHour = 1;
            currentMinute = 0;
        }

        else
            InvokeRepeating("UpdateTime", 0f, 0.15f);
    }

    void UpdateTime()
    {
        int randomHour = Random.Range(0, 24);
        int randomMinute = Random.Range(0, 60);

        string formattedTime = randomHour.ToString("00") + ":" + randomMinute.ToString("00");

        timeText.text = formattedTime;
    }

    void PainTime()
    {
        string formattedTime = currentHour.ToString("00") + ":" + currentMinute.ToString("00");
        timeText.text = formattedTime;

        currentMinute++;

        // If minutes reach 60, increment the hour and reset minutes
        if (currentMinute == 60)
        {
            currentMinute = 0;
            currentHour++;

            // If the hour reaches 7, switch to the Lobby scene
            if (currentHour == 7)
            {
                SceneManager.LoadScene("Lobby");
            }
        }
    }
}
