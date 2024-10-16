using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class bedTrigger : MonoBehaviour
{
    public PlayerMovement pm;
    public string nightmareToLoad;
    public CinemachineVirtualCamera dialogueCam;
    public Transform playerTransf;

    public Animator fadeLight;
    public Animator animSound;

    public GameObject nightmareUI;
    public GameObject nightmareUIESP;

    bool hasSelected = false;
    bool canSelect = true;
    bool isTransitionating = false;
    bool isExiting = false;

    private string currentLanguage;

    void Start()
    {
        dialogueCam.Priority = 1;

        currentLanguage = PlayerPrefs.GetString("GameLanguage", "ENG");

		switch (currentLanguage)
		{
			case "ENG": //nothing ; 
                break;
			case "ESP": nightmareUI = nightmareUIESP; 
                break;
		}
    }

    void Update()
    {
        if (hasSelected)
        {

            if (!isTransitionating && canSelect && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(pm.controllerDetection.jump)))
            {
                StartCoroutine("LoadNightmare");
                FindFirstObjectByType<SAudioManager>().Play("menu_select");
                isTransitionating = true;


            }
            else if (!isTransitionating && canSelect && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(pm.controllerDetection.interact)))
            {
                isExiting = true;
                StartCoroutine("WaitFrames2");
                hasSelected = false;
                dialogueCam.Priority = 1;

                pm.canMove = true;
                pm.isTalking = false;

                nightmareUI.SetActive(false);
                FindFirstObjectByType<SAudioManager>().Play("menu_back");
            }
        }
    }

    public void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && canSelect && !isExiting && !hasSelected && pm.grounded && (Input.GetKey(KeyCode.E) || Input.GetKey(pm.controllerDetection.interact)))
        {
            canSelect = false;
            StartCoroutine("WaitFrames");

            hasSelected = true;
            nightmareUI.SetActive(true);
            dialogueCam.Priority = 20;

            pm.anim.SetFloat("Speed", 0f);
		    pm.rb.velocity = Vector3.zero;
            pm.canMove = false;
            pm.isTalking = true;

            pm.DanceAction(false);
            FindFirstObjectByType<SAudioManager>().Play("menu_select");
        }
    }

    IEnumerator WaitFrames()
    {
        yield return new WaitForSeconds(0.5f);
        canSelect = true;
    }

    IEnumerator WaitFrames2()
    {
        yield return new WaitForSeconds(0.5f);
        isExiting = false;
    }

    IEnumerator LoadNightmare()
    {
        FindFirstObjectByType<SAudioManager>().Play("dream_transition");
        fadeLight.SetTrigger("activate");
        animSound.SetTrigger("lowerForever");
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(nightmareToLoad);
    }
}
