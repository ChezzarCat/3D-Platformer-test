using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTextTrigger : MonoBehaviour
{
    [Header("REFERENCES")]
    public dialogueManagerNormal dialogueManagerNormal;
    public PlayerMovement player;
    public ControllerDetection controllerDetection;

    [Header("DIALOGUE")]
    public dialogue dialogue_ENG;
    public dialogue dialogue_ESP;
    public dialogue dialogueAlt_ENG;
    public dialogue dialogueAlt_ESP;
    public bool mustInteract;
    public bool changesAfterTalking;
    public bool isNPC;
    /*[Tooltip("CAN ONLY BE 0(TOP) 1(MIDDLE) 2(BOTTOM)")]
    public int cameraHeight;*/


    private Animator childAnimator;
    private string currentLanguage;

    public void Start()
    {
        currentLanguage = PlayerPrefs.GetString("GameLanguage", "ENG");


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
                }
                else
                {
                    dialogueManagerNormal.isNpc = false;
                }

                dialogueManagerNormal.lookAtNPC(gameObject.transform);

                //SEND DIALOGUE DEPENDING ON THE LANGUAGE
                switch(currentLanguage)
                {
                    case "ENG":
                        dialogueManagerNormal.StartDialogue(dialogue_ENG);
                        if (changesAfterTalking)
                            dialogue_ENG = dialogueAlt_ENG;
                        break;

                    case "ESP":
                        dialogueManagerNormal.StartDialogue(dialogue_ESP);
                        if (changesAfterTalking)
                            dialogue_ESP = dialogueAlt_ESP;
                        break;
                }


                Destroy(gameObject);
                   

            }
        }
        
    }

    public void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && (Input.GetKey(KeyCode.E) || Input.GetKey(controllerDetection.interact)) && mustInteract && dialogueManagerNormal.isShowing == false && player.grounded)
        {
            if (isNPC)
            {
                dialogueManagerNormal.isNpc = true;
                dialogueManagerNormal.AnimNpc(childAnimator);
            }
            else
            {
                dialogueManagerNormal.isNpc = false;
            }

            dialogueManagerNormal.lookAtNPC(gameObject.transform);
            
            //SEND DIALOGUE DEPENDING ON THE LANGUAGE
            switch(currentLanguage)
            {
                case "ENG":
                    dialogueManagerNormal.StartDialogue(dialogue_ENG);
                    if (changesAfterTalking)
                        dialogue_ENG = dialogueAlt_ENG;
                    break;

                case "ESP":
                    dialogueManagerNormal.StartDialogue(dialogue_ESP);
                    if (changesAfterTalking)
                        dialogue_ESP = dialogueAlt_ESP;
                    break;
            }
        }
        
    }
}
