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
    public PlayerMovement pm;
    public CinemachineFreeLook cinemachineFreeLook;
    bool isCentering;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        changeSensibility();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate input direction based on camera orientation
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // If there's input and player can move, rotate player towards input direction
        if (inputDir != Vector3.zero && pm.GetCanMove() && !pm.IsInState<PlayerDiveState>())
        {
            // Smoothly rotate playerObj towards input direction
            Quaternion targetRotation = Quaternion.LookRotation(inputDir.normalized);
            playerObj.rotation = Quaternion.Slerp(playerObj.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // RECENTER CAM
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.JoystickButton5)) && pm.GetCanMove() && !isCentering)
        {
            StartCoroutine(RecenterCamera(0.3f));
        }
    }

    public void changeSensibility()
    {
        int senX = PlayerPrefs.GetInt("camX", 5);
        int senY = PlayerPrefs.GetInt("camY", 5);

        switch (senX)
        {
            case 1: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 50; break;
            case 2: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 100; break;
            case 3: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 200; break;
            case 4: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 250; break;
            case 5: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 300; break;    //DEFAULT
            case 6: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 350; break;
            case 7: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 400; break;
            case 8: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 450; break;
            case 9: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 500; break;
            case 10: cinemachineFreeLook.m_XAxis.m_MaxSpeed = 550; break;
        }

        switch (senY)
        {
            case 1: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 0.2f; break;
            case 2: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 0.4f; break;
            case 3: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 0.6f; break;
            case 4: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 0.8f; break;
            case 5: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 1f; break;    //DEFAULT
            case 6: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 1.25f; break;
            case 7: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 1.5f; break;
            case 8: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 1.75f; break;
            case 9: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 2f; break;
            case 10: cinemachineFreeLook.m_YAxis.m_MaxSpeed = 2.5f; break;
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
