using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public Animator animFade;
    public Animator animSound;
    public Animator lunaAnim;
    public PlayerMovement pm;

    void Start()
    {
        animFade.SetBool("isOut", false);
        animSound.SetBool("lowerToNothing", false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            StartCoroutine("Dies");
    }


    public IEnumerator Dies()
    {
        pm.canMove = false;
        lunaAnim.SetTrigger("isDead");
        lunaAnim.SetBool("isDancing", false);
        lunaAnim.SetBool("isJumping", false);
        lunaAnim.SetBool("isDashing", false);
        lunaAnim.SetFloat("Speed", 0);
        FindFirstObjectByType<SAudioManager>().Stop("luna_dance");

        animSound.SetBool("lowerToNothing", true);
        yield return new WaitForSeconds(2);
        animFade.SetBool("isOut", true);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("GameOver");
    }
}
