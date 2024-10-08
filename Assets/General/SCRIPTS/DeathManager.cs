using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public Animator animFade;
    public Animator animSound;

    void Start()
    {
        animFade.SetBool("isOut", false);
        animSound.SetBool("lowerToNothing", false);
    }


    public IEnumerator Dies()
    {
        animSound.SetBool("lowerToNothing", true);
        yield return new WaitForSeconds(2);
        animFade.SetBool("isOut", true);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("GameOver");
    }
}
