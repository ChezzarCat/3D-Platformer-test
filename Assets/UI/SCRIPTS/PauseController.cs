using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PauseController : MonoBehaviour
{
    public AudioMixer audioMixerPause;
    public ControllerDetection controllerDetection;
    public GameObject pauseScreen;
    public PlayerMovement pm;
    public Pause pause;

    private bool lockPlayer;

    void Start()
    {
        pauseScreen.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(controllerDetection.pause))
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (pauseScreen.activeSelf)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                pauseScreen.SetActive(false);
                Time.timeScale = 1f;
                audioMixerPause.SetFloat("volumegame", 0f);
                FindFirstObjectByType<SAudioManager>().Stop("pause_music");

                pause.cooldown = false;

                if (lockPlayer == true)
                    pm.canMove = true;
                
                StartCoroutine(pm.WaitJumpFrames2());
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                pauseScreen.SetActive(true);
                Time.timeScale = 0f;
                audioMixerPause.SetFloat("volumegame", -80f);
                FindFirstObjectByType<SAudioManager>().Play("pause_music");

                pause.cooldown = false;

                lockPlayer = true;
                if (pm.canMove == false)
                    lockPlayer = false;
                else
                    pm.canMove = false;
            }
    }
}
