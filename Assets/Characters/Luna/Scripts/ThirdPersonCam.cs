using System.Collections;
using UnityEngine;
using Cinemachine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;
    public bool canMove;
    public CinemachineFreeLook cinemachineFreeLook;
    bool isCentering;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        canMove = true;
    }

    private void Update()
    {
        // Handle input in Update for consistent frame timing
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate input direction based on camera orientation
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // If there's input and player can move, rotate player towards input direction
        if (inputDir != Vector3.zero && canMove)
        {
            // Smoothly rotate playerObj towards input direction
            Quaternion targetRotation = Quaternion.LookRotation(inputDir.normalized);
            playerObj.rotation = Quaternion.Slerp(playerObj.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Recenter camera when specific keys are pressed
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.Joystick1Button5)) && canMove && !isCentering)
        {
            StartCoroutine(RecenterCamera(0.5f));
        }
    }

    private void LateUpdate()
    {
        // Handle camera orientation in LateUpdate for consistent tracking after movement
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
    }


    private IEnumerator RecenterCamera(float duration)
    {
        cinemachineFreeLook.m_RecenterToTargetHeading.m_enabled = true;
        isCentering = true;
        yield return new WaitForSeconds(duration);
        cinemachineFreeLook.m_RecenterToTargetHeading.m_enabled = false;
        isCentering = false;
    }
}
