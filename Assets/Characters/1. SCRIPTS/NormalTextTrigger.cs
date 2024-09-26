using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTextTrigger : MonoBehaviour
{
    [Header("REFERENCES")]
    public dialogueManagerNormal dialogueManagerNormal;
    public PlayerMovement player;

    [Header("DIALOGUE")]
    public dialogue dialogue;
    public dialogue dialogueAlt;
    public bool mustInteract;
    public bool changesAfterTalking;
    public bool isNPC;


    private Animator childAnimator;

    public void Start()
    {
        if (isNPC)
        {
            Transform childTransform = transform.childCount > 0 ? transform.GetChild(0) : null;

            if (childTransform != null)
            {
                childAnimator = childTransform.GetComponent<Animator>();

                if (childAnimator == null)
                {
                    Debug.LogWarning("Animator component not found on child.");
                }
            }
            else
            {
                Debug.LogWarning("No child found for the NPC.");
            }
        }
        
    }


    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && player.grounded)
        {
            if (!mustInteract)
            {
                if (isNPC)
                {
                    dialogueManagerNormal.isNpc = true;
                    dialogueManagerNormal.AnimNpc(childAnimator);
                    dialogueManagerNormal.lookAtNPC(gameObject.transform);
                }
                else
                {
                    dialogueManagerNormal.isNpc = false;
                }

                dialogueManagerNormal.StartDialogue(dialogue);

                if (changesAfterTalking)
                    dialogue = dialogueAlt;
                   

            }
        }
        
    }

    public void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.JoystickButton2)) && mustInteract && dialogueManagerNormal.isShowing == false && player.grounded)
        {
            if (isNPC)
            {
                dialogueManagerNormal.isNpc = true;
                dialogueManagerNormal.AnimNpc(childAnimator);
                dialogueManagerNormal.lookAtNPC(gameObject.transform);
            }
            else
            {
                dialogueManagerNormal.isNpc = false;
            }
                
            dialogueManagerNormal.StartDialogue(dialogue);
            if (changesAfterTalking)
                    dialogue = dialogueAlt;
        }
        
    }
}
