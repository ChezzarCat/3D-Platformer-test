using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtCamera : MonoBehaviour
{
    public Camera mainCamera;

    void Start()
    {
        // If no camera is set, assign the main camera automatically
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
         // Get the direction from the object to the camera
        Vector3 direction = mainCamera.transform.position - transform.position;


        // Now, set the object's forward direction towards the camera
        transform.LookAt(transform.position - direction, Vector3.up);
    }
}
