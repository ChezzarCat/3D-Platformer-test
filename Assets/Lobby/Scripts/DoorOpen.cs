using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public Animator dooranim;
    public DoorOpen2 dooropen;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && dooranim.GetBool("Open2") == false)
        {
            dooranim.SetBool("Open", true);
        }
    }
    

}
