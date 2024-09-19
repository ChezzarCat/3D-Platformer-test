using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class danceNpc : MonoBehaviour
{
    private Animator animplayer;
    public Animator animNPC;

    void Start()
    {
        GameObject playerObject = GameObject.Find("LUNA - PLAYER");
        if (playerObject != null)
        {
            animplayer = playerObject.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("LUNA - PLAYER not found!");
        }
    }

    public void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") )
        {
            if (animplayer.GetBool("isDancing"))
            {
                animNPC.SetBool("isDancing", true);
            }
            else
            {
                animNPC.SetBool("isDancing", false);
            }
        }
        
    }

    public void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") )
        {
            animNPC.SetBool("isDancing", false);
        }
        
    }
}
