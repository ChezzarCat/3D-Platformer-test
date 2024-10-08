using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumpscareTrigger : MonoBehaviour
{
    public Animator anim;

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("MainCamera"))
        {
            anim.SetTrigger("jumpscare");
        }
    }
}
