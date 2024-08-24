using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
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
    bool readyToJump;

    [Header("Animator")]
    public Animator anim;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    float currentSpeed;
    bool isDashing = false;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        bool wasGrounded = grounded;
        grounded = Physics.Raycast(orientation.transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        if (!wasGrounded && grounded)
        {
            anim.SetBool("isJumping", false);
            jumpCount = 0;
            readyToJump = true;
        }

        MyInput();
        ControlSpeed();

        

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        float speed = new Vector2(horizontalInput, verticalInput).magnitude;

        anim.SetFloat("Speed", speed);

        
        // PARTICLES
        if (speed > 0.01f && grounded && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton0)))
        {
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
            anim.SetBool("isDashing", true);
            if (speed > 0.01f && grounded)
            {
                if (!dashParticles.isPlaying)
                    dashParticles.Play();
            }
            else
            {
                if (dashParticles.isPlaying)
                    dashParticles.Stop();
            }
        }
        else
        {
            dashParticles.Stop();
            anim.SetBool("isDashing", false);
            if (speed > 0.01f && grounded)
            {
                if (!runParticles.isPlaying)
                    runParticles.Play();
            }
            else
            {
                if (runParticles.isPlaying)
                    runParticles.Stop();
            }
        }
        

        // JUMP
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1)) && readyToJump && jumpCount < maxJumps)
        {
            if (jumpBurstParticles.isPlaying)
            {
                jumpBurstParticles.Stop();
                jumpBurstParticles.Play();
            }
            else
            {
                jumpBurstParticles.Play();
            }


            readyToJump = false;
            jumpCount++;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
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

        rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);

        // Clamping the velocity to currentSpeed to prevent sliding
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        anim.SetBool("isJumping", true);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
