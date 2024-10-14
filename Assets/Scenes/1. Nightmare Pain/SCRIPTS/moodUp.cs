using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moodUp : MonoBehaviour
{
    public MoodManager mood;

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            mood.moodLevel++;
        }
    }

    public void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            mood.moodLevel--;
        }
    }
}
