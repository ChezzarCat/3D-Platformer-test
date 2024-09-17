using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public Animator dooranim;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dooranim.SetBool("Open", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dooranim.SetBool("Open", false);
        }
    }
    

}
