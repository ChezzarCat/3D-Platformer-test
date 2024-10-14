using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public ThirdPersonCam thirdPersonCam;
    public float walkSpeed;
    public float runSpeed;
    public float groundDrag = 5f; // Increase this value to reduce sliding
    public float jumpForce;
    public float jumpCooldown;
    public ParticleSystem runParticles;
    public ParticleSystem dashParticles;
    public ParticleSystem dashBurstParticles;
    public ParticleSystem jumpBurstParticles;
    public bool canMove = true;
    public bool canOnlyMoveCam = true;
    bool readyToJump = false;
    bool pauseJumpFrames = true;
    
    [Header("Stamina")]
    public Slider staminaSlider;
    public Animator sliderAnim;
    public float staminaDecreaseRate = 10f;  // How fast stamina drains when running
    public float staminaRecoveryRate = 5f;   // How fast stamina recovers when not running
    private float stamina = 100f;

    [Header("Animator")]
    public Animator anim;
    public Animator buttonPressAction;
    public Animator soundAnim;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Stupid stairs")]
    public float maxSlopAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    public Transform orientation;
    public Transform backOrientation;

    [Header("Others")]
    public MoodManager moodManager;
    public bool isTalking = false;
    public ControllerDetection controllerDetection;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    float currentSpeed;
    float speed;
    bool isDashing = false;
    bool reachedZeroStamina = false;
    private DeathManager gameOver;
    bool isScaredToDance;

    [HideInInspector]
    public Rigidbody rb;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentSpeed = walkSpeed;

        gameOver = FindObjectOfType<DeathManager>();
    }

    // GROUND AND GENERAL FLAGS ---------------------------------------------------------

    private void Update()
    {
        bool wasGrounded = grounded;
        bool groundedFromOrientation = Physics.Raycast(orientation.transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        bool groundedFromBackOrientation = Physics.Raycast(backOrientation.transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        grounded = groundedFromOrientation || groundedFromBackOrientation;

        if (!wasGrounded && grounded)
        {
            ResetJump();
        }

        if (isTalking)
        {
            buttonPressAction.SetBool("isInTrigger", false);
            dashParticles.Stop();
            runParticles.Stop();
        }
            

        if (canMove && canOnlyMoveCam)
        {
            MyInput();
            thirdPersonCam.canMove = true;
        }
        else
        {
            FindFirstObjectByType<SAudioManager>().Stop("run");
            FindFirstObjectByType<SAudioManager>().Stop("walk");

            dashBurstParticles.Stop();
            runParticles.Stop();

            thirdPersonCam.canMove = false;
        }

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;


        if (gameObject.transform.position.y < -30f)
        {
            canMove = false;
            gameOver.StartCoroutine("Dies");
        }


        ControlSpeed();
        UpdateStamina();
    }

    private void FixedUpdate()
    {
        if (canMove && canOnlyMoveCam)
            MovePlayer();
    }

    // INPUT ---------------------------------------------------------

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        speed = new Vector2(horizontalInput, verticalInput).magnitude;

        anim.SetFloat("Speed", speed);

        if (grounded && speed < 0.01f && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(controllerDetection.dance)))
            DanceAction(true);
        
        // PARTICLES (PARTICLES CODE IS SUCH A MESS BUT IT WORKS, DO NOT TOUCH CUZ I TRIED TO MAKE IT MODULAR BUT I KEEP BREAKING IT SO I'LL JUST LEAVE IT LIKE THIS LOL)
        if (speed > 0.01f && grounded && stamina > 0 && !reachedZeroStamina && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(controllerDetection.run)))
        {
            DanceAction(false);

            if (dashBurstParticles.isPlaying)
            {
                dashBurstParticles.Stop();
                dashBurstParticles.Play();
            }
            else
            {
                dashBurstParticles.Play();
            }
        }

        if (isDashing)
        {
            runParticles.Stop();
            FindFirstObjectByType<SAudioManager>().Stop("walk");
            anim.SetBool("isDashing", true);
            if (speed > 0.01f && grounded)
            {
                if (!dashParticles.isPlaying)
                {
                    dashParticles.Play();
                    FindFirstObjectByType<SAudioManager>().Play("run");
                }
            }
            else
            {
                if (dashParticles.isPlaying)
                {
                    dashParticles.Stop();
                    FindFirstObjectByType<SAudioManager>().Stop("run");
                }
            }
        }
        else
        {
            
            dashParticles.Stop();
            FindFirstObjectByType<SAudioManager>().Stop("run");
            if (speed > 0.01f && grounded)
            {
                anim.SetBool("isDashing", false);
                DanceAction(false);
                if (!runParticles.isPlaying)
                {
                    runParticles.Play();
                    FindFirstObjectByType<SAudioManager>().Play("walk");
                }
            }
            else
            {
                if (runParticles.isPlaying)
                {
                    runParticles.Stop();
                    FindFirstObjectByType<SAudioManager>().Stop("walk");
                }
            }
        }
        

        // JUMP
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(controllerDetection.jump)) && readyToJump && CanJumpOnSlope() && grounded && pauseJumpFrames) //  && jumpCount < maxJumps
            Jump();
    }

    // MOVEMENT ------------------------------------------------------------------

    private void ControlSpeed()
    {
        // RUN / DASH
        if (grounded && canMove && stamina > 0 && !reachedZeroStamina && speed > 0.01f && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(controllerDetection.run)))
        {
            currentSpeed = runSpeed;
            isDashing = true;

            stamina -= staminaDecreaseRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, 100f);
        }
        else
        {
            currentSpeed = walkSpeed;
            isDashing = false;
        }

        if (stamina == 0)
        {
            reachedZeroStamina = true;
            sliderAnim.SetBool("isZero", true);
        }
        else if (stamina == 100)
        {
            reachedZeroStamina = false;
            sliderAnim.SetBool("isZero", false);
        }
    }

    private void UpdateStamina()
    {
        // Replenish stamina when not dashing and clamp it to max 100
        if (!isDashing)
        {
            stamina += staminaRecoveryRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, 100f);
        }

        // Update the stamina slider
        if (staminaSlider != null)
        {
            staminaSlider.value = stamina;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            if (isDashing && !CanJumpOnSlope())
                rb.AddForce(GetSlopeMoveDirection() * currentSpeed * 5f, ForceMode.Force);
            else if (!isDashing && !CanJumpOnSlope())
                rb.AddForce(GetSlopeMoveDirection() * currentSpeed * 8f, ForceMode.Force);
            else if (isDashing && CanJumpOnSlope())
                rb.AddForce(GetSlopeMoveDirection() * currentSpeed * 10f, ForceMode.Force);
            else if (!isDashing && CanJumpOnSlope())
                rb.AddForce(GetSlopeMoveDirection() * currentSpeed * 10f, ForceMode.Force);

            if(rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);

        // Clamping the velocity to currentSpeed to prevent sliding
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

        if (readyToJump)
            rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > currentSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * currentSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }   
    }

    public void CanMove(bool canIt)
    {
        canMove = canIt;
    }

    // JUMP ------------------------------------------------------------

    private void Jump()
    {
        readyToJump = false;
            
        FindFirstObjectByType<SAudioManager>().Play("jump");

        if (jumpBurstParticles.isPlaying)
        {
            jumpBurstParticles.Stop();
            jumpBurstParticles.Play();
        }
        else
        {
            jumpBurstParticles.Play();
        }

        DanceAction(false);
        anim.SetBool("isJumping", true);

        rb.useGravity = true;
        exitingSlope = true;

        // reset y velocity
        if (rb.velocity.y > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
        
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        anim.SetBool("isJumping", false);
        StartCoroutine(WaitJumpFrames());
    }

    IEnumerator WaitJumpFrames()
    {
        yield return new WaitForSeconds(0.05f);
        exitingSlope = false;
        readyToJump = true;
    }

    public IEnumerator WaitJumpFrames2()
    {
        pauseJumpFrames = false;
        yield return new WaitForSeconds(0.2f);
        pauseJumpFrames = true;
    }

    // DANCE ------------------------------------------------------------

    public void DanceAction(bool isDance)
    {
        if (moodManager != null)
        {
            if (moodManager.moodLevel == 0)
                isScaredToDance = false;
            else
                isScaredToDance = true;
        }
        else
        {
            isScaredToDance = true;
        }

        if (!isScaredToDance)
        {
            if (isDance)
            {
                soundAnim.SetBool("isDancing", true);
                FindFirstObjectByType<SAudioManager>().Play("luna_dance");
                anim.SetBool("isDancing", true); 
            }
            else
            {
                soundAnim.SetBool("isDancing", false);
                FindFirstObjectByType<SAudioManager>().Stop("luna_dance");
                anim.SetBool("isDancing", false); 
            }
        }
    }

    // SLOPE ------------------------------------------------------------

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private bool CanJumpOnSlope()
    {
        if (OnSlope())
        {
            if (rb.velocity.y > 0)
            {
                return false;
            }
        }

        return true;
    }

    // TRIGGERS ------------------------------------------------------------

    public void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Trigger") && !grounded)
        {
            buttonPressAction.SetBool("isInTrigger", false);
        }
        else if (collision.gameObject.CompareTag("Trigger") && grounded && !isTalking)
        {
            buttonPressAction.SetBool("isInTrigger", true);
        }
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Trigger") && !grounded)
        {
            FindFirstObjectByType<SAudioManager>().Stop("menu_scroll");
        }
        else if (collision.gameObject.CompareTag("Trigger") && grounded && !isTalking)
        {
            FindFirstObjectByType<SAudioManager>().Play("menu_scroll");
        }
    }

    public void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Trigger"))
        {
            buttonPressAction.SetBool("isInTrigger", false);
        }
    }
}
