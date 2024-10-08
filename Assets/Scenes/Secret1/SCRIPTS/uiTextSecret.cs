using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class uiTextSecret : MonoBehaviour
{
    public TextMeshProUGUI moodText;
    private string currentLanguage;


    void Start()
    {
        currentLanguage = PlayerPrefs.GetString("GameLanguage", "ENG");
    }

    void Update()
    {
        switch (currentLanguage)
        {
            case "ENG":
                moodText.text = "F̵͖͖͓̦̖̱̺̻̘͍̪̲̓̿́͛̾̓̓͐ͅͅR̶̢̨̧̪̲̺̦͓̭͈̙̱͙̻͊͛͌̈́̈́͝Ȩ̵̡̨̻̭̱̰̼͈͚͖̊͌̿̉̇Ę̵̨̞͚̥̠̱̗͔̝̗̘̲̄̆"; break;

            case "ESP":
                moodText.text = "L̶̻̦̣̙̠̙͙̈̑͒͛̔̇̏̐͠Ȉ̸̢̧̢̧̧̢̭̹̹̪̻̭̗͖̓̉̈́̾͐͘͝B̶̢̧̨̢̩͇̮̟̥̮̾̆̎̄̎͐̉̐͒̕ͅȒ̷̡̛̺̼̪̲̳̱͇̞͕̄́͒͒͌͝Ȩ̷̡̨̹̠̯͖̼̖͚̮́͒̑̈́̅͝"; break;
        }
    }
}
