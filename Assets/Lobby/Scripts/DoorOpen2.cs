using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen2 : MonoBehaviour
{
    public Animator dooranim;
    public DoorOpen dooropen;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && dooranim.GetBool("Open") == false)
        {
            dooranim.SetBool("Open2", true);
        }
    }
    

}
