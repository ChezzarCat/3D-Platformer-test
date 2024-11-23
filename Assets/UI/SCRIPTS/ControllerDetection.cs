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
    public PlayerMovement pm;

    [Header("Input")]
    public KeyCode jump;
    public KeyCode interact;
    public KeyCode run;
    public KeyCode dance;
    public KeyCode pause;
    public string crouch;
    public string axix1;
    public string axix2;

    // Cooldown to prevent flickering
    private float inputSwitchCooldown = 0.5f;
    private float lastInputTime = 0f;

    // Flag to track if a controller is connected
    private bool isControllerConnected = false;

    // Store the name of the currently connected controller
    private string currentControllerName = "";
    
    void Start()
    {
        if (freeLookCamera == null || pm == null)
        {
            Debug.Log("Missing controller freelookcam and player movement, do not worry if this is a scene without player movement");
        }
    }

    void Update()
    {
        DetectActiveController();
        //Debug.Log(activeController);

        switch (activeController)
        {
            case "Keyboard":
                ActivateDeactivateKeyboard(true);
                ActivateDeactivatePSX(false);
                ActivateDeactivateXBOX(false);

                if (freeLookCamera != null)
                {
                    freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y PC";
                    freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X PC";
                }

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
                pause = KeyCode.JoystickButton9;
                crouch = "R2-PS4";
                axix1 = "Axis 7";
                axix2 = "Axis 8";

                if (freeLookCamera != null)
                {
                    freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y PSX";
                    freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X PSX";
                }

                break;

            case "XBOX":
                ActivateDeactivateKeyboard(false);
                ActivateDeactivatePSX(false);
                ActivateDeactivateXBOX(true);
                
                if (freeLookCamera != null)
                {
                    freeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
                    freeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";
                }

                DefaultInput();
                break;
        }

        if (freeLookCamera != null || pm != null)
        {
            if (!pm.canMove)
            {
                freeLookCamera.m_YAxis.m_InputAxisName = "";
                freeLookCamera.m_XAxis.m_InputAxisName = "";
            }
        }


        if (Input.GetKeyDown(KeyCode.F11) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.F4))
        {
            ToggleFullscreen();
        }
    }

    public void ToggleFullscreen()
    {
        // Get the current screen resolution
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;
        bool isFullScreen = Screen.fullScreen;

        // Toggle fullscreen mode
        Screen.SetResolution(currentWidth, currentHeight, !isFullScreen);
    }

    void DefaultInput()
    {
        jump = KeyCode.JoystickButton0;
        interact = KeyCode.JoystickButton1;
        run = KeyCode.JoystickButton2;
        dance = KeyCode.JoystickButton3;
        pause = KeyCode.JoystickButton7;
        crouch = "R2-XBOX";
        axix1 = "Axis 6";
        axix2 = "Axis 7";
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
        // Use unscaled time to prevent issues with Time.timeScale being set to 0
        if (Time.unscaledTime - lastInputTime < inputSwitchCooldown)
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
                        activeController = "XBOX";
                    }
                    
                    lastInputTime = Time.unscaledTime; // Reset cooldown timer
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
                lastInputTime = Time.unscaledTime; // Reset cooldown timer
            }
        }
    }
}
