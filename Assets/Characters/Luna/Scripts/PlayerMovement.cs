using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("MOVEMENT")]
    public ThirdPersonCam thirdPersonCam;
    public float walkSpeed;
    public float runSpeed;
    public float jumpForce;
    public Transform orientation;
    public ControllerDetection controllerDetection;
    public Animator anim;
    public Animator buttonPressAction;

    [Header("GROUND CHECK")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("INTERNATE STATES")]
    public bool canMove = true;
    private Rigidbody rb;
    private PlayerState currentState;
    public SAudioManager audioManager;
    public bool pauseJumpFrames;
    public bool WaitFramesGround = true;

    [Header("JUMP SETTINGS")]
    public float maxJumpHeight = 2f;
    public float maxJumpTime = 0.5f;
    public int jumpCount = 0;
    public float comboJumpTimeWindowMax = 0.2f;
    public float comboJumpTimeWindow;

    [Header("PARTICLES")]
    public ParticleSystem runParticles;

    private float gravity;
    private float initialJumpVelocity;

    // Public method to check the current state
    public bool IsInState<T>() where T : PlayerState
    {
        return currentState is T;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioManager = Object.FindObjectOfType<SAudioManager>();
        rb.freezeRotation = true;
        comboJumpTimeWindow = comboJumpTimeWindowMax;
        
        // Set initial state to idle
        SwitchState(new PlayerIdleState(this));

        // Calculate gravity and initial jump velocity based on desired jump height and time
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (canMove)
        {
            currentState?.Update();
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            currentState?.FixedUpdate();
        }

        if (grounded && comboJumpTimeWindow > 0 && jumpCount > 0)
        {
            DecreaseJumpCountTimer();
        }
    }

    // Method to switch between different states
    public void SwitchState(PlayerState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();
    }

    public void DecreaseJumpCountTimer()
    {   
        comboJumpTimeWindow -= Time.fixedDeltaTime;

        if (comboJumpTimeWindow <= 0 && jumpCount > 0)
        {
            jumpCount = 0;
            comboJumpTimeWindow = comboJumpTimeWindowMax;
        }
    }

    // GETTERS AND SETTERS --------------------------------------

    public Rigidbody GetRigidbody() {  return rb; }

    public bool GetCanMove() {  return canMove; }

    public float GetGravity()  {  return gravity;  }

    public float GetInitialJumpVelocity()   {  return initialJumpVelocity;  }

    public int GetJumpCount() { return jumpCount; }

    public void ResetJumpCountTimer() { comboJumpTimeWindow = comboJumpTimeWindowMax; }

    // COROUTINES -------------------------------------------

    public IEnumerator WaitJumpFrames2()
    {
        pauseJumpFrames = false;
        yield return new WaitForSeconds(0.2f);
        pauseJumpFrames = true;
    }

    public IEnumerator WaitFramesGroundCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        WaitFramesGround = false;
    }

    // TRIGGERS ------------------------------------------------------------

    public void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Trigger") && !grounded)
        {
            buttonPressAction.SetBool("isInTrigger", false);
        }
        else if (collision.gameObject.CompareTag("Trigger") && grounded && !IsInState<PlayerDancingState>() && canMove)
        {
            buttonPressAction.SetBool("isInTrigger", true);
        }
        else if (collision.gameObject.CompareTag("Trigger") && grounded && !IsInState<PlayerDancingState>() && !canMove)
        {
            buttonPressAction.SetBool("isInTrigger", false);
        }
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Trigger") && !grounded)
        {
            audioManager.Stop("menu_scroll");
        }
        else if (collision.gameObject.CompareTag("Trigger") && grounded && !IsInState<PlayerDancingState>())
        {
            audioManager.Play("menu_scroll");
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

// MAIN CLASS

public abstract class PlayerState
{
    protected PlayerMovement player;

    public PlayerState(PlayerMovement player)
    {
        this.player = player;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}

// IDLE -----------------------------------------------------------------------------

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.anim.SetFloat("Speed", 0);
    }

    public override void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        // If there is input, switch to walking state
        if (input.magnitude > 0.1f)
        {
            player.SwitchState(new PlayerWalkingState(player));
        }

        // If space key is pressed, switch to jumping state
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump)) && player.pauseJumpFrames && player.grounded && !player.IsInState<PlayerJumpingState>())
        {
            player.SwitchState(new PlayerJumpingState(player));
        }

        // If dance key is pressed, switch to dancing state
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(player.controllerDetection.dance))
        {
            player.SwitchState(new PlayerDancingState(player));
        }
    }
}

// WALK -----------------------------------------------------------------------------

public class PlayerWalkingState : PlayerState
{
    public PlayerWalkingState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.anim.SetFloat("Speed", 0.5f);
        player.runParticles.Play();
    }

    public override void ExitState()
    {
        player.runParticles.Stop();
    }

    public override void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        // If there is no input, switch to idle state
        if (input.magnitude < 0.1f)
        {
            player.SwitchState(new PlayerIdleState(player));
        }

        // If space key is pressed, switch to jumping state
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump)) && player.grounded && !player.IsInState<PlayerJumpingState>())
        {
            player.SwitchState(new PlayerJumpingState(player));
        }

        // If dance key is pressed, switch to dancing state
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(player.controllerDetection.dance))
        {
            player.SwitchState(new PlayerDancingState(player));
        }
    }

    public override void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        // Adjust speed based on how much the joystick or key is pressed
        float speedFactor = Mathf.Clamp(input.magnitude, 0.1f, 1f);
        Vector3 moveDirection = player.orientation.forward * verticalInput + player.orientation.right * horizontalInput;
        player.GetRigidbody().AddForce(moveDirection.normalized * player.walkSpeed * speedFactor * 5f, ForceMode.Force);

        // Set animator speed value based on the player's movement speed
        player.anim.SetFloat("Speed", speedFactor);
    }
}

// JUMP -----------------------------------------------------------------------------

public class PlayerJumpingState : PlayerState
{
    private float fallMultiplier = 2.5f; // Adjust this to control how fast the player falls
    private float jumpTimeMax = 0.5f; // Maximum time the player can hold the jump
    private float jumpTimeCounter;
    private bool isHoldingJump;
    private float jumpCounterMultiplier = 1;

    public PlayerJumpingState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.ResetJumpCountTimer();
        player.jumpCount++;

        switch (player.GetJumpCount())
        {
            case 0: jumpCounterMultiplier = 1; break;
            case 1: jumpCounterMultiplier = 1; break;
            case 2: jumpCounterMultiplier = 1.5f; break;
            case 3: jumpCounterMultiplier = 2f; break;
        }


        player.GetRigidbody().drag = 5.5f;
        player.GetRigidbody().velocity = new Vector3(player.GetRigidbody().velocity.x, 0, player.GetRigidbody().velocity.z); // Reset vertical velocity
        player.GetRigidbody().AddForce(Vector3.up * (player.jumpForce * jumpCounterMultiplier), ForceMode.Impulse);
        player.anim.SetBool("isJumping", true);
        player.grounded = false;
        jumpTimeCounter = jumpTimeMax;
        isHoldingJump = true;

        player.WaitFramesGround = true;
        player.StartCoroutine("WaitFramesGroundCoroutine");
    }

    public override void ExitState() 
    {
        if (player.GetRigidbody().velocity.x != 0f)
        {
            if (player.jumpCount >= 3)
                player.jumpCount = 0;
        }
        else
        {
            if (player.jumpCount >= 2)
                player.jumpCount = 0;
        }

        player.GetRigidbody().drag = 5;
        player.anim.SetBool("isJumping", false);
    }

    public override void Update()
    {
        if (player.grounded && !player.WaitFramesGround)
        {
            player.SwitchState(new PlayerIdleState(player));
        }

        // Check if the jump button is released or jump time is over
        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(player.controllerDetection.jump) && player.GetJumpCount() != 3)
        {
            isHoldingJump = false;
        }
    }

    public override void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        // Adjust speed based on how much the joystick or key is pressed
        float speedFactor = Mathf.Clamp(input.magnitude, 0.1f, 1f);
        Vector3 moveDirection = player.orientation.forward * verticalInput + player.orientation.right * horizontalInput;
        player.GetRigidbody().AddForce(moveDirection.normalized * player.walkSpeed * speedFactor * 5f, ForceMode.Force);

        // Set animator speed value based on the player's movement speed
        player.anim.SetFloat("Speed", speedFactor);

        // JUMP ---------------------------------------------

        // Apply extra gravity to fall faster
        if (player.GetRigidbody().velocity.y < 0)
        {
            Vector3 fallForce = Vector3.down * fallMultiplier * Mathf.Abs(Physics.gravity.y);
            player.GetRigidbody().AddForce(fallForce, ForceMode.Acceleration);
        }
        else if (!isHoldingJump && player.GetRigidbody().velocity.y > 0)
        {
            // Apply gravity multiplier when jump is not held to stop ascending
            Vector3 fallForce = Vector3.down * fallMultiplier * Mathf.Abs(Physics.gravity.y);
            player.GetRigidbody().AddForce(fallForce, ForceMode.Acceleration);
        }

        // Continue applying upward force if jump is held and time allows
        if (isHoldingJump && jumpTimeCounter > 0)
        {
            player.GetRigidbody().AddForce(Vector3.up * (player.jumpForce * jumpCounterMultiplier) * 0.5f, ForceMode.Acceleration);
            jumpTimeCounter -= Time.fixedDeltaTime;
        }
    }
}

// DANCE -----------------------------------------------------------------------------

public class PlayerDancingState : PlayerState
{
    public PlayerDancingState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.anim.SetBool("isDancing", true);
        player.audioManager.Play("luna_dance");
    }

    public override void ExitState() 
    {
        player.audioManager.Stop("luna_dance");
        player.anim.SetBool("isDancing", false);
    }

    public override void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        // If there is input, switch to walking state
        if (input.magnitude > 0.1f)
        {
            player.SwitchState(new PlayerWalkingState(player));
        }

        // If space key is pressed, switch to jumping state
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump)) && player.pauseJumpFrames && player.grounded && !player.IsInState<PlayerJumpingState>())
        {
            player.SwitchState(new PlayerJumpingState(player));
        }

        // If dance key is released, stop dancing and switch to idle state
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(player.controllerDetection.dance))
        {
            player.SwitchState(new PlayerIdleState(player));
        }
    }
}
