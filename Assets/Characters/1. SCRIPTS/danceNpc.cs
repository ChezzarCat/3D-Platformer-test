using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class DanceNpc : MonoBehaviour
{
    public Rig rigConstraint;
    private Animator animPlayer;
    public Animator animNPC;
    public float transitionDuration = 0.5f;

    private Coroutine weightCoroutine;

    void Start()
    {
        rigConstraint.weight = 0f;

        GameObject playerObject = GameObject.Find("LUNA - PLAYER");
        if (playerObject != null)
        {
            animPlayer = playerObject.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("LUNA - PLAYER not found!");
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (weightCoroutine != null)
            {
                StopCoroutine(weightCoroutine);
            }

            weightCoroutine = StartCoroutine(SmoothTransition(1f));

            if (animPlayer.GetBool("isDancing"))
            {
                animNPC.SetBool("isDancing", true);
            }
            else
            {
                animNPC.SetBool("isDancing", false);
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (weightCoroutine != null)
            {
                StopCoroutine(weightCoroutine);
            }

            weightCoroutine = StartCoroutine(SmoothTransition(0f));

            animNPC.SetBool("isDancing", false);
        }
    }

    private IEnumerator SmoothTransition(float targetWeight)
    {
        float startWeight = rigConstraint.weight;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            rigConstraint.weight = Mathf.Lerp(startWeight, targetWeight, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rigConstraint.weight = targetWeight;
    }
}
