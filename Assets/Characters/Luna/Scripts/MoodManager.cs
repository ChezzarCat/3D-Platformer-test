using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoodManager : MonoBehaviour
{
    [Header("Mood Level")]
    [Tooltip("0 = NEUTRAL | 1 = WORRIED | 2 = SCARED | 3 = TERRIFIED")]
    public int moodLevel = 0;

    [Header("References")]
    public Animator anim;
    public TextMeshProUGUI moodText;

    private string currentLanguage;


    void Start()
    {
        currentLanguage = PlayerPrefs.GetString("GameLanguage", "ENG");
        moodLevel = 0;
    }

    void Update()
    {
        anim.SetInteger("moodLevel", moodLevel);
        
        switch (currentLanguage)
        {
            case "ENG":
                switch(moodLevel)
                {
                    case 0: moodText.text = "NEUTRAL"; break;
                    case 1: moodText.text = "WORRIED"; break;
                    case 2: moodText.text = "SCARED"; break;
                    case 3: moodText.text = "TERRIFIED"; break;
                }

                break;

            case "ESP":
                switch(moodLevel)
                {
                    case 0: moodText.text = "NEUTRAL"; break;
                    case 1: moodText.text = "PREOCUPADA"; break;
                    case 2: moodText.text = "ASUSTADA"; break;
                    case 3: moodText.text = "ATERRORIZADA"; break;
                }
                
                break;
        }

        if (moodLevel > 3)
            moodLevel = 3;
        else if (moodLevel < 0)
            moodLevel = 0;
    }
}
