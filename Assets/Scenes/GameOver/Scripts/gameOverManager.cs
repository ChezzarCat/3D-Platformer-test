using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class gameOverManager : MonoBehaviour
{
    [Header("REFERENCES")]
    public TMP_Text gameOverText1;
    public TMP_Text gameOverText2;
    public TMP_Text gameOverText3;
    public Animator anim;
    public Animator animSound;

    [Header("ENGLISH")]
    public string gameOverTextENG;
    public string gameOverText2ENG;
    public string gameOverText3ENG;

    [Header("SPANISH")]
    public string gameOverTextESP;
    public string gameOverText2ESP;
    public string gameOverText3ESP;

    private string currentLanguage;
    private bool canSkip;

    private List<string> secretScenes = new List<string>();

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        GatherSecretScenes();
        
        anim.SetBool("isOut", false);
        animSound.SetBool("lowerToNothing", false);

        currentLanguage = PlayerPrefs.GetString("GameLanguage", "ENG");

        switch (currentLanguage)
        {
            case "ENG":
                gameOverText1.text = gameOverTextENG;
                gameOverText2.text = gameOverText2ENG;
                gameOverText3.text = gameOverText3ENG;
                break;

            case "ESP":
                gameOverText1.text = gameOverTextESP;
                gameOverText2.text = gameOverText2ESP;
                gameOverText3.text = gameOverText3ESP;
                break;
        }

        canSkip = false;
        StartCoroutine("CanSkip");
    }

    void Update()
    {
        if (Input.anyKeyDown && canSkip)
        {
            canSkip = false;
            StartCoroutine("goToLobby");
        }
    }

    void GatherSecretScenes()
    {
        // Loop through all the scenes in the Build Settings
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            // Get scene path and extract scene name
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // If the scene name starts with "Secret", add it to the list
            if (sceneName.StartsWith("Secret"))
            {
                secretScenes.Add(sceneName);
            }
        }
    }

    IEnumerator goToLobby()
    {
        anim.SetBool("isOut", true);
        animSound.SetBool("lowerToNothing", true);
        FindFirstObjectByType<SAudioManager>().Play("menu_select");
        yield return new WaitForSeconds(2);
        
        int randomNumber = Random.Range(1, 11);
        Debug.Log(randomNumber);

        if (randomNumber == 1 && secretScenes.Count > 0)
        {
            // Choose a random scene from the secretScenes array
            string randomSecretScene = secretScenes[Random.Range(0, secretScenes.Count)];
            SceneManager.LoadScene(randomSecretScene);
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }

    }

    IEnumerator CanSkip()
    {
        yield return new WaitForSeconds(1);
        canSkip = true;
    }
}
