using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen3 : MonoBehaviour
{
    public Animator dooranim;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dooranim.SetBool("Open2", false);
            dooranim.SetBool("Open", false);
        }
    }
    

}
