using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Pause : MonoBehaviour
{
    [Header("SCRIPTS")]
    public PauseController pc;
    public ThirdPersonCam playercam;

    [Header("ANIMATOR")]
    public Animator animSelect;
    public Animator animSelectOptions;
    public Animator animSelectLanguage;

    [Header("OTHERS")]
    public GameObject optionsObject;
    
    public TMP_Text volumetext;
    public TMP_Text sensibilityX;
    public TMP_Text sensibilityY;
    
    private int currSelection = 0;
    private int currSelectionOptions = 0;
    private int currSelectionLanguage = 0;
    private float volumeSaved = 100;

    private int currSelectionCAMX = 5;
    private int currSelectionCAMY = 5;

    public bool cooldown = false;

    bool isInAnotherScreen = false;
    bool isOptions = false;

    private string currentLanguage;

    void Start()
    {
        currSelection = 0;
        optionsObject.SetActive(false);

        currentLanguage = PlayerPrefs.GetString("GameLanguage", "ENG");

        if (currentLanguage == "ENG")
            currSelectionLanguage = 0;
        else if (currentLanguage == "ESP")
            currSelectionLanguage = 1;

        currSelectionCAMX = PlayerPrefs.GetInt("camX", 5);
        currSelectionCAMY = PlayerPrefs.GetInt("camY", 5);
        volumeSaved = PlayerPrefs.GetFloat("volume", 100f);
    }

    void Update()
    {
        animSelect.SetInteger("Select", currSelection);

        if (currSelection > 3)
            currSelection = 0;
        else if (currSelection < 0)
            currSelection = 3;

        // NORMAL -------------------------------------------------------------------

        if (isInAnotherScreen == false)
        {

            if (!cooldown && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || (Input.GetAxis("Vertical") > 0) || (Input.GetAxis(pc.controllerDetection.axix2) > 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelection--;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }
            else if (!cooldown && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || (Input.GetAxis("Vertical") < 0 || (Input.GetAxis(pc.controllerDetection.axix2) < 0))))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelection++;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }


            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(pc.controllerDetection.jump))
            {
                switch (currSelection)
                {
                    case 0: 
                        cooldown = false;
                        pc.Pause();
                        FindFirstObjectByType<SAudioManager>().Play("menu_select");
                        break;

                    case 1:
                        isOptions = true;
                        isInAnotherScreen = true;
                        optionsObject.SetActive(true);
                        FindFirstObjectByType<SAudioManager>().Play("menu_select");
                        break;

                    case 2:
                        // GALLERY
                        FindFirstObjectByType<SAudioManager>().Play("menu_select");
                        break;

                    case 3:
                        cooldown = false;
                        Application.Quit();
                        FindFirstObjectByType<SAudioManager>().Play("menu_select");
                        break;

                }
            }
        }

        // OPTIONS -------------------------------------------------------------------

        if (isOptions)
        {
            animSelectOptions.SetInteger("Select", currSelectionOptions);

            if (currSelectionOptions > 4)
                currSelectionOptions = 0;
            else if (currSelectionOptions < 0)
                currSelectionOptions = 4;

            // BUTTONS ---------------------------------------------------------------

            if (!cooldown && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || (Input.GetAxis("Vertical") > 0) || (Input.GetAxis(pc.controllerDetection.axix2) > 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelectionOptions--;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }
            else if (!cooldown && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || (Input.GetAxis("Vertical") < 0) || (Input.GetAxis(pc.controllerDetection.axix2) < 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelectionOptions++;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }

            // SOUND

            if (volumeSaved > 100)
                volumeSaved = 100;
            else if (volumeSaved < 0)
                volumeSaved = 0;

            volumetext.text = volumeSaved.ToString();

            if (currSelectionOptions == 0 && !cooldown && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || (Input.GetAxis("Horizontal") > 0) || (Input.GetAxis(pc.controllerDetection.axix1) > 0)))
            {
                StartCoroutine("Cooldown", 0.1f);
                volumeSaved++;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");

                PlayerPrefs.SetFloat("volume", volumeSaved);
                PlayerPrefs.Save();
            }
            else if (currSelectionOptions == 0 && !cooldown && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || (Input.GetAxis("Horizontal") < 0) || (Input.GetAxis(pc.controllerDetection.axix1) < 0)))
            {
                StartCoroutine("Cooldown", 0.1f);
                volumeSaved--;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");

                PlayerPrefs.SetFloat("volume", volumeSaved);
                PlayerPrefs.Save();
            }


            // LANGUAGE

            animSelectLanguage.SetInteger("Select", currSelectionLanguage);

            if (currSelectionLanguage > 1)
                currSelectionLanguage = 0;
            else if (currSelectionLanguage < 0)
                currSelectionLanguage = 1;

            if (currSelectionOptions == 1 && !cooldown && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || (Input.GetAxis("Horizontal") > 0) || (Input.GetAxis(pc.controllerDetection.axix1) > 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelectionLanguage++;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }
            else if (currSelectionOptions == 1 && !cooldown && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || (Input.GetAxis("Horizontal") < 0) || (Input.GetAxis(pc.controllerDetection.axix1) < 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelectionLanguage--;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }


            // CAM X
            
            sensibilityX.text = currSelectionCAMX.ToString();

            if (currSelectionCAMX > 10)
                currSelectionCAMX = 10;
            else if (currSelectionCAMX < 1)
                currSelectionCAMX = 1;

            if (currSelectionOptions == 2 && !cooldown && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || (Input.GetAxis("Horizontal") > 0) || (Input.GetAxis(pc.controllerDetection.axix1) > 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelectionCAMX++;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }
            else if (currSelectionOptions == 2 && !cooldown && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || (Input.GetAxis("Horizontal") < 0) || (Input.GetAxis(pc.controllerDetection.axix1) < 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelectionCAMX--;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }



            // CAM Y

            sensibilityY.text = currSelectionCAMY.ToString();

            if (currSelectionCAMY > 10)
                currSelectionCAMY = 10;
            else if (currSelectionCAMY < 1)
                currSelectionCAMY = 1;

            if (currSelectionOptions == 3 && !cooldown && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || (Input.GetAxis("Horizontal") > 0) || (Input.GetAxis(pc.controllerDetection.axix1) > 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelectionCAMY++;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }
            else if (currSelectionOptions == 3 && !cooldown && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || (Input.GetAxis("Horizontal") < 0) || (Input.GetAxis(pc.controllerDetection.axix1) < 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currSelectionCAMY--;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }
            

            // ACCEPT

            if (currSelectionOptions == 4 && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(pc.controllerDetection.jump)))
            {
                if (currSelectionLanguage == 0)
                    PlayerPrefs.SetString("GameLanguage", "ENG");
                else if (currSelectionLanguage == 1)
                    PlayerPrefs.SetString("GameLanguage", "ESP");

                PlayerPrefs.SetInt("camX", currSelectionCAMX);
                PlayerPrefs.SetInt("camY", currSelectionCAMY);

                PlayerPrefs.SetFloat("volume", volumeSaved);

                PlayerPrefs.Save();

                playercam.changeSensibility();

                FindFirstObjectByType<SAudioManager>().Play("menu_select");
                currSelectionOptions = 0;
                optionsObject.SetActive(false);
                isInAnotherScreen = false;
                isOptions = false;
                cooldown = false;
            }

            // back

            if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(pc.controllerDetection.interact)))
            {
                if (currSelectionLanguage == 0)
                    PlayerPrefs.SetString("GameLanguage", "ENG");
                else if (currSelectionLanguage == 1)
                    PlayerPrefs.SetString("GameLanguage", "ESP");

                PlayerPrefs.SetInt("camX", currSelectionCAMX);
                PlayerPrefs.SetInt("camY", currSelectionCAMX);

                PlayerPrefs.Save();

                playercam.changeSensibility();

                FindFirstObjectByType<SAudioManager>().Play("menu_back");
                currSelectionOptions = 0;
                optionsObject.SetActive(false);
                isInAnotherScreen = false;
                isOptions = false;
                cooldown = false;
            }
        }
        
    }

    IEnumerator Cooldown(float cooldownTime)
    {
        cooldown = true;
        yield return new WaitForSecondsRealtime(cooldownTime);
        cooldown = false;
    }
}
