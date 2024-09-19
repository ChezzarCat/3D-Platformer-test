using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtPlayer : MonoBehaviour
{
    Animator anim;
    public bool ikActive = false;
    public Transform playerObject;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK()
    {
        if (anim)
        {
            if(ikActive)
            {
                if (playerObject != null)
                {
                    anim.SetLookAtWeight(1);
                    anim.SetLookAtPosition(playerObject.position);
                }
            }
            else
            {
                anim.SetLookAtWeight(0);
            }
        }
    }
}
