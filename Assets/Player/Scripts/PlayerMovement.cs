using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    private int jumpCount = 0;
    public int maxJumps = 1;
    public bool canMove = true;
    bool readyToJump;

    [Header("Animator")]
    public Animator anim;
    public Animator buttonPressAction;

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
    public bool isTalking = false;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    float currentSpeed;
    bool isDashing = false;

    [HideInInspector]
    public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        canMove = true;
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        bool wasGrounded = grounded;
        bool groundedFromOrientation = Physics.Raycast(orientation.transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        bool groundedFromBackOrientation = Physics.Raycast(backOrientation.transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        grounded = groundedFromOrientation || groundedFromBackOrientation;

        if (!wasGrounded && grounded)
        {
            anim.SetBool("isJumping", false);
            jumpCount = 0;
            ResetJump();
        }

        if (isTalking)
            buttonPressAction.SetBool("isInTrigger", false);

        if (canMove)
        {
            MyInput();
            thirdPersonCam.canMove = true;
        }
            

        if (!canMove)
        {
            FindFirstObjectByType<SAudioManager>().Stop("run");
            FindFirstObjectByType<SAudioManager>().Stop("walk");

            dashBurstParticles.Stop();
            runParticles.Stop();

            thirdPersonCam.canMove = false;
        }

        ControlSpeed();

        

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
            
    }

    private void FixedUpdate()
    {
        if (canMove)
            MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        float speed = new Vector2(horizontalInput, verticalInput).magnitude;

        anim.SetFloat("Speed", speed);

        if (grounded && speed < 0.01f && (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton3)))
            anim.SetBool("isDancing", true);

        
        // PARTICLES
        if (speed > 0.01f && grounded && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton0)))
        {
            anim.SetBool("isDancing", false);

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
                anim.SetBool("isDancing", false);
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
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1)) 
            && readyToJump 
            && jumpCount < maxJumps 
            && CanJumpOnSlope()
            && grounded)
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

            anim.SetBool("isDancing", false);
            jumpCount++;
            anim.SetBool("isJumping", true);
            
            Jump();
        }
    }

    private void ControlSpeed()
    {
        // RUN / DASH
        if (grounded && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton0)))
        {
            currentSpeed = runSpeed;
            isDashing = true;
        }
        else
        {
            currentSpeed = walkSpeed;
            isDashing = false;
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

    private void Jump()
    {
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
        StartCoroutine(WaitJumpFrames());
    }

    IEnumerator WaitJumpFrames()
    {
        yield return new WaitForSeconds(0.05f);
        exitingSlope = false;
        readyToJump = true;
    }

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

    public void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Trigger"))
        {
            buttonPressAction.SetBool("isInTrigger", false);
        }
    }
}
