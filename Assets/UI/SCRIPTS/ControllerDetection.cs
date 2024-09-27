using UnityEngine;
using Cinemachine;

public class ControllerDetection : MonoBehaviour
{
    [Header("REFERENCES")]
    public string activeController = "Keyboard";
    public GameObject[] KeyboardSpritesUI;
    public GameObject[] PSXSpritesUI;
    public GameObject[] XBOXSpritesUI;
    public CinemachineFreeLook freeLookCamera;

    [Header("Input")]
    public KeyCode jump;
    public KeyCode interact;
    public KeyCode run;
    public KeyCode dance;

    // Cooldown to prevent flickering
    private float inputSwitchCooldown = 0.5f;
    private float lastInputTime = 0f;

    // Flag to track if a controller is connected
    private bool isControllerConnected = false;

    // Store the name of the currently connected controller
    private string currentControllerName = "";
    

    void Update()
    {
        DetectActiveController();
        Debug.Log($"Detected Active Controller: {activeController}");

        switch (activeController)
        {
            case "Keyboard":
                ActivateDeactivateKeyboard(true);
                ActivateDeactivatePSX(false);
                ActivateDeactivateXBOX(false);

                freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y PC";
                freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X PC";

                DefaultInput();
                break;

            case "PSX":
                ActivateDeactivateKeyboard(false);
                ActivateDeactivatePSX(true);
                ActivateDeactivateXBOX(false);

                jump = KeyCode.JoystickButton1;
                interact = KeyCode.JoystickButton2;
                run = KeyCode.JoystickButton0;
                dance = KeyCode.JoystickButton3;

                freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y PSX";
                freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X PSX";

                break;

            case "XBOX":
                ActivateDeactivateKeyboard(false);
                ActivateDeactivatePSX(false);
                ActivateDeactivateXBOX(true);

                freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
                freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";

                DefaultInput();
                break;

            case "Other":
                ActivateDeactivateKeyboard(false);
                ActivateDeactivatePSX(false);
                ActivateDeactivateXBOX(true);

                freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
                freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";

                DefaultInput();
                break;
        }
    }

    void DefaultInput()
    {
        jump = KeyCode.JoystickButton0;
        interact = KeyCode.JoystickButton1;
        run = KeyCode.JoystickButton2;
        dance = KeyCode.JoystickButton3;
    }

    void ActivateDeactivateKeyboard(bool activate)
    {
        foreach (GameObject obj in KeyboardSpritesUI)
        {
            obj.SetActive(activate);
        }
    }

    void ActivateDeactivatePSX(bool activate)
    {
        foreach (GameObject obj in PSXSpritesUI)
        {
            obj.SetActive(activate);
        }
    }

    void ActivateDeactivateXBOX(bool activate)
    {
        foreach (GameObject obj in XBOXSpritesUI)
        {
            obj.SetActive(activate);
        }
    }

    void DetectActiveController()
    {
        // Only allow input switch if cooldown has passed
        if (Time.time - lastInputTime < inputSwitchCooldown)
        {
            return;
        }

        string[] joystickNames = Input.GetJoystickNames();
        bool controllerStillConnected = false;

        // Check for currently connected controllers
        foreach (string joystick in joystickNames)
        {
            if (!string.IsNullOrEmpty(joystick))
            {
                controllerStillConnected = true;

                // If no controller is connected, detect and set the first connected controller
                if (!isControllerConnected)
                {
                    currentControllerName = joystick.ToLower();
                    isControllerConnected = true;

                    if (currentControllerName.Contains("wireless") || currentControllerName.Contains("dualshock") || currentControllerName.Contains("ps4"))
                    {
                        activeController = "PSX";
                    }
                    else if (currentControllerName.Contains("xbox") || currentControllerName.Contains("xinput"))
                    {
                        activeController = "XBOX";
                    }
                    else
                    {
                        activeController = "Other";
                    }
                    
                    lastInputTime = Time.time; // Reset cooldown timer
                }

                // Break out of the loop once the connected controller is identified
                break;
            }
        }

        // If no controllers are detected, set active input to keyboard
        if (!controllerStillConnected)
        {
            isControllerConnected = false;
            currentControllerName = "";

            // Check for keyboard input only if no controllers are connected
            if (Input.anyKeyDown)
            {
                activeController = "Keyboard";
                lastInputTime = Time.time; // Reset cooldown timer
            }
        }
    }
}
