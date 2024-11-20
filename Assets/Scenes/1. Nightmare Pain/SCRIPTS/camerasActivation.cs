using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camerasActivation : MonoBehaviour
{
    public GameObject cameras;
    public Animator anim;
    int currentCamera = 0;
    public bool cooldown = false;
    public PlayerMovement pm;
    bool isActive = false;

    void Start()
    {
        cameras.SetActive(false);
        currentCamera = 0;
        isActive = false;
    }

    void Update()
    {
        if (isActive)
        {
            anim.SetInteger("curCam", currentCamera);

            if (currentCamera > 3)
                currentCamera = 0;
            else if (currentCamera < 0)
                currentCamera = 3;

            if (!cooldown && ((Input.GetAxis("Horizontal") > 0) || (Input.GetAxis(pm.controllerDetection.axix1) > 0)))
            {
                StartCoroutine("Cooldown", 0.2f);
                currentCamera++;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }
            else if (!cooldown && ((Input.GetAxis("Horizontal") < 0 || (Input.GetAxis(pm.controllerDetection.axix1) < 0))))
            {
                StartCoroutine("Cooldown", 0.2f);
                currentCamera--;
                FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
            }
            
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(pm.controllerDetection.interact))
        {
            if (isActive)
                DeActivateCams();
            else
                ActivateCams();
        }

    }

    public void ActivateCams()
    {
        cameras.SetActive(true);
        isActive = true;
        pm.canMove = false;
    }

    public void DeActivateCams()
    {
        cameras.SetActive(false);
        isActive = false;
        pm.canMove = true;
    }

    IEnumerator Cooldown(float cooldownTime)
    {
        cooldown = true;
        yield return new WaitForSecondsRealtime(cooldownTime);
        cooldown = false;
    }
}
