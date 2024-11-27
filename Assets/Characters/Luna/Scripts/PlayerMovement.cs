using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [Header("REFERENCES")]
    [SerializeField] private TMP_Text currentStateText;
    [SerializeField] private TMP_Text xPositionText;
    [SerializeField] private TMP_Text yPositionText;
    [SerializeField] private TMP_Text zPositionText;
    [SerializeField] private TMP_Text dragText;

    [Header("GROUND CHECK")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("JUMP SETTINGS")]
    public float coyoteTimeMAX = 0.3f;
    public float coyoteTime;
    public float maxJumpHeight = 2f;
    public float maxJumpTime = 0.5f;
    public int jumpCount = 0;
    public float fallMultiplier = 4f;
    public float comboJumpTimeWindowMax = 0.2f;
    public float comboJumpTimeWindow;

    public bool pauseJumpFrames;
    public bool WaitFramesGround = true;
    public bool isGroundPoundFalling = true;

    private float gravity;
    private float initialJumpVelocity;
    private float fallSpeed;
    public float fallAcceleration = 2f;

    // Input Buffer Variables
    public float jumpBufferTimeMax = 0.15f;
    public float jumpBufferTime;
    private bool jumpBuffered;

    [Header("PARTICLES")]
    public ParticleSystem runParticles;
    public ParticleSystem dashParticles;

    public ParticleSystem burst1;
    public ParticleSystem burst2;
    public ParticleSystem burst3;
    public ParticleSystem burstSpecial;

    public ParticleSystem jumpBurst;

    [Header("INTERNATE STATES")]
    public CinemachineShake shakeScript;
    public bool canMove = true;
    private Rigidbody rb;
    private PlayerState currentState;
    public SAudioManager audioManager;
    public bool isR2Released = true;
    public bool isBounceParry = false;

    public bool IsInState<T>() where T : PlayerState
    {
        return currentState is T;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioManager = Object.FindObjectOfType<SAudioManager>();
        shakeScript = FindObjectOfType<CinemachineShake>();
        rb.freezeRotation = true;
        comboJumpTimeWindow = comboJumpTimeWindowMax;
        coyoteTime = coyoteTimeMAX;
        jumpBufferTime = 0;

        jumpBuffered = false;
        isGroundPoundFalling = true;
        isBounceParry = false;
        
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

            // Buffer jump input
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(controllerDetection.jump)) && rb.velocity.y < 0 && !IsInState<PlayerLongJumpState>())
            {
                jumpBuffered = true;
                jumpBufferTime = jumpBufferTimeMax;
            }
        }

        if (Input.GetAxis(controllerDetection.crouch) <= 0.1f)
        {
            isR2Released = true;
        }

        // TESTING -------------
        currentStateText.text = currentState.ToString();
        xPositionText.text = $"{rb.velocity.x:F2}";
        yPositionText.text = $"{rb.velocity.y:F2}";
        zPositionText.text = $"{rb.velocity.z:F2}";
        dragText.text =  $"{rb.drag}";
    
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            currentState?.FixedUpdate();

            if (!grounded)
            {
                // Handle Jump Buffer
                if (jumpBuffered)
                {
                    jumpBufferTime -= Time.deltaTime;
                    if (jumpBufferTime <= 0)
                    {
                        jumpBuffered = false;
                    }
                }
            }
        }

        if (grounded && comboJumpTimeWindow > 0 && jumpCount > 0)
        {
            DecreaseJumpCountTimer();
        }

        // Apply increasing downward force when not grounded to simulate more realistic gravity
        if (!grounded && isGroundPoundFalling)
        {
            fallSpeed += fallAcceleration * Time.fixedDeltaTime;
            rb.AddForce(Vector3.down * fallSpeed, ForceMode.Acceleration);
        }
        else
        {
            fallSpeed = 0; // Reset fall speed when grounded
        }

        // If jump is buffered and player is grounded, jump immediately
        if (jumpBuffered && grounded)
        {
            jumpBuffered = false;
            SwitchState(new PlayerJumpingState(this));
        }
    }

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

    public void PauseImpactStarter(float timescale, float seconds)
    {
        StartCoroutine(PauseImpact(timescale, seconds));
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

    public IEnumerator WaitGroundPoundIntro()
    {
        yield return new WaitForSeconds(0.3f);
        isGroundPoundFalling = true;
    }

    public IEnumerator PauseImpact(float timescale, float seconds)
    {
        Time.timeScale = timescale;
        yield return new WaitForSecondsRealtime(seconds);
        Time.timeScale = 1f;
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


// MAIN CLASS ----------------------------------------------------------------------

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
        player.GetRigidbody().drag = 10f;
        player.anim.SetFloat("Speed", 0);
    }

    public override void ExitState()
    {
        player.GetRigidbody().drag = 5f;
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

        // If player is falling, switch to falling state
        if (!player.IsInState<PlayerJumpingState>() && !player.grounded)
        {
            player.SwitchState(new PlayerFallingState(player));
        }

        // If player is crouching, switch to crouch
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f) && player.grounded)
        {
            player.SwitchState(new PlayerCrouchState(player));
        }
        else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f) && player.grounded && input.magnitude > 0.1f)
        {
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump)) && player.grounded && !player.IsInState<PlayerJumpingState>())
            {
                player.SwitchState(new PlayerLongJumpState(player));
            }
            else
            {
                player.SwitchState(new PlayerWalkCrouchState(player));
            }
        }
    }
}

// WALK -----------------------------------------------------------------------------

public class PlayerWalkingState : PlayerState
{
    public PlayerWalkingState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.GetRigidbody().drag = 4.5f;
        player.anim.SetFloat("Speed", 0.5f);
        player.runParticles.Play();
        player.dashParticles.Play();
    }

    public override void ExitState()
    {
        player.runParticles.Stop();
        player.dashParticles.Stop();
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

        // If player is falling, switch to falling state
        if (!player.IsInState<PlayerJumpingState>() && !player.grounded)
        {
            player.SwitchState(new PlayerFallingState(player));
        }

        // If player is crouching, switch to crouch
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f) && player.grounded)
        {
            player.SwitchState(new PlayerCrouchState(player));
        }
        else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f) && player.grounded && input.magnitude > 0.1f)
        {
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump)) && player.grounded && !player.IsInState<PlayerJumpingState>())
            {
                player.SwitchState(new PlayerLongJumpState(player));
            }
            else
            {
                player.SwitchState(new PlayerWalkCrouchState(player));
            }
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
    private float jumpTimeMax = 2f; // Maximum time the player can hold the jump
    private float jumpTimeCounter;
    private bool isHoldingJump;
    private float jumpCounterMultiplier = 1;

    private Vector3 targetScale = new Vector3(1f, 1f, 1f);
    private Vector3 jumpScale = new Vector3(0.7f, 1.4f, 0.7f);
    private Vector3 jumpScale2 = new Vector3(0.5f, 1.6f, 0.5f);
    private Vector3 jumpScale3 = new Vector3(0.4f, 1.8f, 0.4f);
    private float scaleLerpFactor = 0f;

    public PlayerJumpingState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.jumpBufferTime = 0;

        player.audioManager.Play("jump");
        player.burst1.Play();
        player.burst2.Play();
        player.jumpBurst.Play();
        

        player.ResetJumpCountTimer();
        player.jumpCount++;

        switch (player.GetJumpCount())
        {
            case 0: jumpCounterMultiplier = 1; player.transform.localScale = jumpScale; break;
            case 1: jumpCounterMultiplier = 1; player.transform.localScale = jumpScale; break;
            case 2: jumpCounterMultiplier = 1.5f; player.transform.localScale = jumpScale2; break;
            case 3: jumpCounterMultiplier = 2.2f; player.transform.localScale = jumpScale3; break;
        }

    
        player.GetRigidbody().drag = 4f;
        player.GetRigidbody().velocity = new Vector3(player.GetRigidbody().velocity.x, 0, player.GetRigidbody().velocity.z); // Reset vertical velocity
        player.GetRigidbody().AddForce(Vector3.up * (player.jumpForce * jumpCounterMultiplier), ForceMode.Impulse);

        player.anim.SetInteger("jumpState", player.GetJumpCount());
        player.anim.SetBool("isJumping", true);

        player.grounded = false;
        jumpTimeCounter = jumpTimeMax;
        isHoldingJump = true;

        player.WaitFramesGround = true;
        player.StartCoroutine("WaitFramesGroundCoroutine");
    }

    public override void ExitState() 
    {
        player.transform.localScale = targetScale;

        if (IsPlayerMoving())
        {
            if (player.jumpCount >= 3)
                player.jumpCount = 0;
        }
        else
        {
            if (player.jumpCount >= 2)
                player.jumpCount = 0;
        }
        
        player.anim.SetBool("isJumping", false);
    }


    public bool IsPlayerMoving()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        return Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
    }


    public override void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        if (player.grounded && !player.WaitFramesGround && (input.magnitude > 0.1f))
        {
            player.SwitchState(new PlayerWalkingState(player));
            player.burst2.Play();
        }
        else if (player.grounded && !player.WaitFramesGround)
        {
            player.SwitchState(new PlayerIdleState(player));
            player.burst2.Play();
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f) && !player.WaitFramesGround && player.isR2Released)
        {
            player.SwitchState(new PlayerGroundPoundState(player));
        }

        // Check if the jump button is released or jump time is over
        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(player.controllerDetection.jump) && player.GetJumpCount() != 3)
        {
            isHoldingJump = false;
            player.jumpBurst.Stop();
        }
    }

    public override void FixedUpdate()
    {

        // MOVE WHILE JUMPING -------------------------------

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
        if ((player.GetRigidbody().velocity.y < 0) || (!isHoldingJump && player.GetRigidbody().velocity.y > 0))
        {
            Vector3 fallForce = Vector3.down * player.fallMultiplier * Mathf.Abs(Physics.gravity.y);
            player.GetRigidbody().AddForce(fallForce, ForceMode.Acceleration);
        }

        // Continue applying upward force if jump is held and time allows
        if (isHoldingJump && jumpTimeCounter > 0)
        {
            player.GetRigidbody().AddForce(Vector3.up * (player.jumpForce * jumpCounterMultiplier) * 0.5f, ForceMode.Acceleration);
            jumpTimeCounter -= Time.fixedDeltaTime;
        }

        SmoothScaleBackToNormal();
    }

    private void SmoothScaleBackToNormal()
    {
        Vector3 jumpScaleNew = new Vector3();
        switch (player.GetJumpCount())
        {
            case 0: jumpScaleNew = jumpScale; break;
            case 1: jumpScaleNew = jumpScale; break;
            case 2: jumpScaleNew = jumpScale2; break;
            case 3: jumpScaleNew = jumpScale3; break;
        }

        float lerpSpeed = 4f; // Speed at which to interpolate back to the original scale
        scaleLerpFactor += Time.fixedDeltaTime * lerpSpeed;
        scaleLerpFactor = Mathf.Clamp(scaleLerpFactor, 0f, 1f);
        player.transform.localScale = Vector3.Lerp(jumpScaleNew, targetScale, scaleLerpFactor);
    }
}

// LONG JUMP ---------------------------------------------------------------------------

public class PlayerLongJumpState : PlayerState
{
    private float longJumpPower = 100f;

    private float jumpTimeMax = 2f; // Maximum time the player can hold the jump
    private float jumpTimeCounter;

    private Vector3 targetScale = new Vector3(1f, 1f, 1f);
    private Vector3 jumpScale = new Vector3(0.5f, 1.7f, 0.5f);
    private float scaleLerpFactor = 0f;

    public PlayerLongJumpState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.jumpBufferTime = 0;

        player.audioManager.Play("jump");
        player.burst1.Play();
        player.burst2.Play();
        player.jumpBurst.Play();

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
    
        player.GetRigidbody().drag = 4f;
        player.GetRigidbody().velocity = new Vector3(player.GetRigidbody().velocity.x, 0, player.GetRigidbody().velocity.z); // Reset vertical velocity

        player.GetRigidbody().AddForce(Vector3.up * (player.jumpForce - 3), ForceMode.Impulse);

        Vector3 moveDirection = player.orientation.forward * verticalInput + player.orientation.right * horizontalInput;
        player.GetRigidbody().AddForce(moveDirection.normalized * longJumpPower * 5f, ForceMode.Force);

        player.anim.SetInteger("jumpState", 4);
        player.anim.SetBool("isJumping", true);

        player.grounded = false;
        jumpTimeCounter = jumpTimeMax;

        player.WaitFramesGround = true;
        player.StartCoroutine("WaitFramesGroundCoroutine");
    }

    public override void ExitState() 
    {
        player.transform.localScale = targetScale;

        // Maintain horizontal velocity when transitioning out of LongJump
        Vector3 currentVelocity = player.GetRigidbody().velocity;
        player.GetRigidbody().velocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        player.anim.SetBool("isJumping", false);
        player.burst2.Play();
    }



    public override void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        // If player is still , switch to idle
        if (player.grounded && !player.WaitFramesGround && input.magnitude < 0.1f)
        {
            player.SwitchState(new PlayerIdleState(player));
        }
        else if (player.grounded && !player.WaitFramesGround && input.magnitude > 0.1f)
        {
            player.SwitchState(new PlayerWalkingState(player));
        }

        // If player is crouching, switch to crouch
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f) && player.grounded && !player.WaitFramesGround)
        {
            player.SwitchState(new PlayerCrouchState(player));
        }
        else if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f) && player.grounded && input.magnitude > 0.1f && !player.WaitFramesGround)
        {
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump)) && player.grounded && !player.IsInState<PlayerJumpingState>())
            {
                player.SwitchState(new PlayerLongJumpState(player));
            }
            else
            {
                player.SwitchState(new PlayerWalkCrouchState(player));
            }
        }

    }

    public override void FixedUpdate()
    {
        if (jumpTimeCounter > 0)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            Vector3 moveDirection = player.orientation.forward * verticalInput + player.orientation.right * horizontalInput;
            player.GetRigidbody().AddForce(moveDirection.normalized * longJumpPower * 0.5f, ForceMode.Force);

            jumpTimeCounter -= Time.fixedDeltaTime;
        }

        SmoothScaleBackToNormal();
    }

    private void SmoothScaleBackToNormal()
    {
        float lerpSpeed = 4f; // Speed at which to interpolate back to the original scale
        scaleLerpFactor += Time.fixedDeltaTime * lerpSpeed;
        scaleLerpFactor = Mathf.Clamp(scaleLerpFactor, 0f, 1f);
        player.transform.localScale = Vector3.Lerp(jumpScale, targetScale, scaleLerpFactor);
    }
}

// GROUND-POUND ---------------------------------------------------------------------------

public class PlayerGroundPoundState : PlayerState
{
    private Vector3 targetScale = new Vector3(1f, 1f, 1f);
    private Vector3 fallScale = new Vector3(0.5f, 1.5f, 0.5f);
    private float scaleLerpFactor = 0f;
    private float scaleLerpFactor2 = 0f;

    private bool isScalingToFall = true;

    public PlayerGroundPoundState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.isR2Released = false;
        player.GetRigidbody().useGravity = false;
        player.GetRigidbody().velocity = Vector3.zero;
        player.grounded = false;
        
        player.isGroundPoundFalling = false;
        player.anim.SetBool("isGroundPounding", true);
        player.StartCoroutine("WaitGroundPoundIntro");
    }

    public override void ExitState() 
    {
        player.GetRigidbody().useGravity = true;
        player.transform.localScale = targetScale;
    }

    

    public override void Update()
    {
        if (player.isGroundPoundFalling == true)
        {
            player.GetRigidbody().useGravity = true;
        }

        if (player.grounded)
        {
            if (player.isGroundPoundFalling == false)
            {
                player.burstSpecial.Play();
                player.isBounceParry = true;
            }
                
            
            player.SwitchState(new PlayerJumpPoundState(player));
        }

    }

    public override void FixedUpdate()
    {
        if (player.isGroundPoundFalling == true)
        {
            Vector3 fallForce = Vector3.down * (player.fallMultiplier * 5) * Mathf.Abs(Physics.gravity.y);
            player.GetRigidbody().AddForce(fallForce, ForceMode.Acceleration);
            SmoothScaleBackToNormal();
        }
           
    }

    private void SmoothScaleBackToNormal()
    {
        if (isScalingToFall)
        {
            // Lerp to fallScale
            float lerpSpeed = 6f;
            scaleLerpFactor += Time.fixedDeltaTime * lerpSpeed;
            scaleLerpFactor = Mathf.Clamp(scaleLerpFactor, 0f, 1f);

            player.transform.localScale = Vector3.Lerp(targetScale, fallScale, scaleLerpFactor);

            if (scaleLerpFactor >= 1f)
            {
                // Transition to next phase
                isScalingToFall = false;
            }
        }
        else
        {
            // Lerp back to targetScale
            float lerpSpeed = 1f;
            scaleLerpFactor2 += Time.fixedDeltaTime * lerpSpeed;
            scaleLerpFactor2 = Mathf.Clamp(scaleLerpFactor2, 0f, 1f);

            player.transform.localScale = Vector3.Lerp(fallScale, targetScale, scaleLerpFactor2);

        }
    }
}

// JUMP-POUND -----------------------------------------------------------------------------

public class PlayerJumpPoundState : PlayerState
{
    private float jumpTimeMax = 2f; // Maximum time the player can hold the jump
    private float jumpTimeCounter;
    private float jumpCounterMultiplier = 1.7f;

    private Vector3 targetScale = new Vector3(1f, 1f, 1f);
    private Vector3 groundScale = new Vector3(1.4f, 0.6f, 1.4f);
    private Vector3 jumpScale = new Vector3(0.6f, 1.7f, 0.6f);
    private float scaleLerpFactor = 0f;

    private bool isScalingToJump = true;

    public PlayerJumpPoundState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.jumpBufferTime = 0;

        player.audioManager.Play("jump");
        player.burst2.Play();
        player.burst3.Play();
        player.jumpBurst.Play();

    
        player.GetRigidbody().drag = 4f;
        player.GetRigidbody().velocity = new Vector3(player.GetRigidbody().velocity.x, 0, player.GetRigidbody().velocity.z); // Reset vertical velocity
        player.GetRigidbody().AddForce(Vector3.up * (player.jumpForce * jumpCounterMultiplier), ForceMode.Impulse);
        
        player.anim.SetBool("isJumping", true);

        if (player.isBounceParry == true)
        {
            player.PauseImpactStarter(0.2f, 0.3f);
            player.shakeScript.TriggerShake(4f, 6f, 0.2f);
            player.anim.SetInteger("jumpState", 6);
        }
        else
        {
            player.PauseImpactStarter(0.1f, 0.07f);
            player.shakeScript.TriggerShake(3f, 5f, 0.1f);
            player.anim.SetInteger("jumpState", 5);
        }

        
        player.isBounceParry = false;

        player.grounded = false;
        jumpTimeCounter = jumpTimeMax;

        player.WaitFramesGround = true;
        player.StartCoroutine("WaitFramesGroundCoroutine");
    }

    public override void ExitState() 
    {
        player.transform.localScale = targetScale;

        player.anim.SetBool("isJumping", false);
        player.anim.SetBool("isGroundPounding", false);
    }

    public override void Update()
    {
        if (player.grounded && !player.WaitFramesGround)
        {
            player.SwitchState(new PlayerIdleState(player));
            player.burst2.Play();
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f) && !player.WaitFramesGround && player.isR2Released)
        {
            player.SwitchState(new PlayerGroundPoundState(player));
        }
    }

    public override void FixedUpdate()
    {
        // MOVE WHILE JUMPING -------------------------------

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
        if ((player.GetRigidbody().velocity.y < 0))
        {
            Vector3 fallForce = Vector3.down * player.fallMultiplier * Mathf.Abs(Physics.gravity.y);
            player.GetRigidbody().AddForce(fallForce, ForceMode.Acceleration);
        }

        // Continue applying upward force if jump is held and time allows
        if (jumpTimeCounter > 0)
        {
            player.GetRigidbody().AddForce(Vector3.up * (player.jumpForce * jumpCounterMultiplier) * 0.5f, ForceMode.Acceleration);
            jumpTimeCounter -= Time.fixedDeltaTime;
        }

        SmoothScaleBackToNormal();
    }

    private void SmoothScaleBackToNormal()
    {
        if (isScalingToJump)
        {
            // Lerp to jumpScale
            float lerpSpeed = 6f;
            scaleLerpFactor += Time.fixedDeltaTime * lerpSpeed;
            scaleLerpFactor = Mathf.Clamp(scaleLerpFactor, 0f, 1f);

            player.transform.localScale = Vector3.Lerp(groundScale, jumpScale, scaleLerpFactor);

            if (scaleLerpFactor >= 1f)
            {
                // Transition to scaling back to target
                isScalingToJump = false;
                scaleLerpFactor = 0f; // Reset for the next phase
            }
        }
        else
        {
            // Lerp back to targetScale
            float lerpSpeed = 4f;
            scaleLerpFactor += Time.fixedDeltaTime * lerpSpeed;
            scaleLerpFactor = Mathf.Clamp(scaleLerpFactor, 0f, 1f);

            player.transform.localScale = Vector3.Lerp(jumpScale, targetScale, scaleLerpFactor);

        }
    }

}

// FALLING -----------------------------------------------------------------------------

public class PlayerFallingState : PlayerState
{
    public PlayerFallingState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.GetRigidbody().drag = 4f;
        player.coyoteTime = player.coyoteTimeMAX;
        player.anim.SetFloat("Speed", 0);
    }

    public override void ExitState()
    {
        player.coyoteTime = player.coyoteTimeMAX;
        player.burst2.Play();
    }

    public override void Update()
    {
        if (player.coyoteTime > 0)
        {
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump)) && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f))
            {
                player.SwitchState(new PlayerLongJumpState(player));
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump))
            {
                player.SwitchState(new PlayerJumpingState(player));
            }
            
        }
        else
        {
            if (player.grounded)
            {
                // Check horizontal velocity to determine if we should transition to walking instead of idle
                if (player.GetRigidbody().velocity.magnitude > 0.1f)
                {
                    player.SwitchState(new PlayerWalkingState(player));
                }
                else
                {
                    player.SwitchState(new PlayerIdleState(player));
                }
            }
            else
            {
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetAxis(player.controllerDetection.crouch) > 0.1f) && player.isR2Released)
                {
                    player.SwitchState(new PlayerGroundPoundState(player));
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        DecreaseCoyoteTimer();

        // FALL

        Vector3 fallForce = Vector3.down * (player.fallMultiplier + 2) * Mathf.Abs(Physics.gravity.y);
        player.GetRigidbody().AddForce(fallForce, ForceMode.Acceleration);

        // MOVE WHILE FALLING
        
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        // Adjust speed based on how much the joystick or key is pressed
        float speedFactor = Mathf.Clamp(input.magnitude, 0.1f, 1f);
        Vector3 moveDirection = player.orientation.forward * verticalInput + player.orientation.right * horizontalInput;
        player.GetRigidbody().AddForce(moveDirection.normalized * player.walkSpeed * speedFactor * 5f, ForceMode.Force);
    }

    private void DecreaseCoyoteTimer()
    {   
        if (player.coyoteTime > 0)
            player.coyoteTime -= Time.fixedDeltaTime;
    }
}

// CROUCH -----------------------------------------------------------------------------

public class PlayerCrouchState : PlayerState
{
    public PlayerCrouchState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.GetRigidbody().drag = 2;
        player.anim.SetBool("isCrouching", true);
    }

    public override void ExitState()
    {
        player.anim.SetBool("isCrouching", false);
        player.isR2Released = false;
    }

    public override void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        // If there is input, switch to walking state
        if (input.magnitude > 0.1f)
        {
            player.SwitchState(new PlayerWalkCrouchState(player));
        }

        // If player stops crouching, switch to idle state
        if (Input.GetKeyUp(KeyCode.LeftShift) && player.controllerDetection.activeController == "Keyboard")
        {
            player.SwitchState(new PlayerIdleState(player));
        }
        else if (Input.GetAxis(player.controllerDetection.crouch) <= 0.1f && player.controllerDetection.activeController != "Keyboard")
        {
            player.SwitchState(new PlayerIdleState(player));
        }

        // If space key is pressed, switch to jumping state
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump)) && player.pauseJumpFrames && player.grounded && !player.IsInState<PlayerJumpingState>())
        {
            player.SwitchState(new PlayerJumpingState(player));
        }

        // If player is falling, switch to falling state
        if (!player.IsInState<PlayerJumpingState>() && !player.grounded)
        {
            player.SwitchState(new PlayerFallingState(player));
        }
    }
}

// WALK CROUCH -----------------------------------------------------------------------------

public class PlayerWalkCrouchState : PlayerState
{
    public PlayerWalkCrouchState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.GetRigidbody().drag = 2f;
        player.runParticles.Play();
        player.anim.SetFloat("Speed", 0.5f);
        player.anim.SetBool("isCrouching", true);
    }

    public override void ExitState()
    {
        player.runParticles.Stop();
        player.anim.SetFloat("Speed", 0f);
        player.anim.SetBool("isCrouching", false);
        player.isR2Released = false;
    }

    public override void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(horizontalInput, verticalInput);

        // If there is no input, switch to idle state
        if (input.magnitude < 0.1f)
        {
            player.SwitchState(new PlayerCrouchState(player));
        }

        // If player stops crouching, switch to idle state
        if (Input.GetKeyUp(KeyCode.LeftShift) && player.controllerDetection.activeController == "Keyboard")
        {
            player.SwitchState(new PlayerIdleState(player));
        }
        else if (Input.GetAxis(player.controllerDetection.crouch) <= 0.1f && player.controllerDetection.activeController != "Keyboard")
        {
            player.SwitchState(new PlayerIdleState(player));
        }

        // If space key is pressed, switch to jumping state
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(player.controllerDetection.jump)) && player.pauseJumpFrames && player.grounded && !player.IsInState<PlayerJumpingState>())
        {
            player.SwitchState(new PlayerLongJumpState(player));
        }

        // If player is falling, switch to falling state
        if (!player.IsInState<PlayerLongJumpState>() && !player.IsInState<PlayerJumpingState>() && !player.grounded)
        {
            player.SwitchState(new PlayerFallingState(player));
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
        player.GetRigidbody().AddForce(moveDirection.normalized * (player.walkSpeed / 8) * speedFactor * 5f, ForceMode.Force);

        // Set animator speed value based on the player's movement speed
        player.anim.SetFloat("Speed", speedFactor);
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
