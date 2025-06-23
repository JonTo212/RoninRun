using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerV2 : MonoBehaviour
{
    [Header("Movement Variables")]
    [SerializeField] private float moveSpeed = 10.0f;              //Ground move speed
    [SerializeField] private float maxVelocity = 15.0f;
    [SerializeField] private float runAcceleration = 7.5f;         //Ground accel
    [SerializeField] private float runDeceleration = 5f;         //Ground decel
    [SerializeField] private float airAcceleration = 2.5f;         //Air accel
    [SerializeField][Range(0, 1)] private float airControl = 0.3f;              //Air control precision multipler (0-1)
    [SerializeField] private float sideStrafeAcceleration = 50f;  // How fast acceleration occurs to get up to sideStrafeSpeed
    [SerializeField] private float sideStrafeSpeed = 1.0f;          // Max speed to generate when side strafing
    //[SerializeField] private float jumpSpeed = 10.0f;
    [HideInInspector] public Vector3 inputVector = Vector3.zero; //used for dash, wishdir is local space
    [HideInInspector] public Vector3 wishdir;

    [Header("Crouching/Sliding Variables")]
    [SerializeField] private float slideForce = 0.5f;
    [SerializeField] private float slideJumpForce = 0.25f;
    [SerializeField] private float slideBoostDuration;
    private bool canSlideJump;
    [HideInInspector] public bool crouched;
    private bool hasSlid;
    [HideInInspector] public bool slide;
    private bool wasCrouchingLastFrame;

    [Header("Gravity, Friction, JumpHeight")]
    [SerializeField] private float timeToZero;
    [SerializeField] private float timeToMaxSpeed;
    [SerializeField] private float frictionValue;
    [SerializeField] private float apexHeight;
    [SerializeField] private float apexTime;
    public float gravity;
    public bool isGrounded;
    private float playerHeight;
    private bool wishJump = false;
    private float jumpVel;
    public bool useGrav;
    //public float gravity = 25f;
    //[SerializeField] private float friction = 7.5f; //Default friction

    [Header("Misc")]
    public bool holdToBHop;
    public bool canWallRun;
    private float standHeight = 1.8f;
    private float crouchHeight = 0.6f;
    public Transform playerView; //Camera
    public float xMouseSensitivity = 30.0f;
    public float yMouseSensitivity = 30.0f;

    //Slope variables
    private float groundRayDistance = 1f;
    private RaycastHit slopeHit;
    private float slopeForce;
    private Vector3 slopeNormal;
    private float slopeAngle;
    [HideInInspector] public bool slopeSlide;
    private bool slideQueue;

    //Camera rotations
    private float rotX;
    private float rotY;

    //UI, anim and components
    private Vector3 uiMaxVel; //for UI top speed display
    [HideInInspector] public CharacterController characterController;
    private CapsuleCollider capsule;
    [HideInInspector] public Vector3 playerVelocity = Vector3.zero;
    [HideInInspector] public float playerTopVelocity = 0.0f; //for UI
    [HideInInspector] public Vector3 clampedVel; //for UI
    [SerializeField] private CameraSwap camSwap;
    [SerializeField] private WallRun wallRun;
    [SerializeField] private LayerMask shurikenLayer;
    [HideInInspector] public float animXInput;
    [HideInInspector] public float animZInput;

    void Start()
    {
        gravity = 2 * apexHeight / Mathf.Pow(apexTime, 2);
        jumpVel = 2 * apexHeight / apexTime;
        frictionValue = maxVelocity / timeToZero;
        runAcceleration = maxVelocity / timeToMaxSpeed;

        //Hide the cursor
        Cursor.visible = false;

        //Set camera
        if (playerView == null && Camera.main != null)
        {
            playerView = Camera.main.transform;
        }

        //Set up static variables
        characterController = GetComponent<CharacterController>();
        capsule = GetComponent<CapsuleCollider>();
        camSwap = GetComponent<CameraSwap>();
        slopeForce = -3f * jumpVel;
        //slopeForce = -3f * jumpSpeed;
        canWallRun = true;
        useGrav = true;
    }

    void Update()
    {
        HandleCameraMovement();
        HandleMovementInput();
        QueueJump();

        isGrounded = characterController.isGrounded;
        playerHeight = characterController.height;

        if (useGrav)
        {
            Gravity(); //Needs to be before slope handling
        }

        //Apply downward force to smooth slope movement
        if (OnSlope())
        {
            HandleSlope();
        }
        else
        {
            slopeSlide = false;
        }

        //Movement
        if (isGrounded)
        {
            GroundMove();
            Jump();
        }
        else if (!isGrounded)
        {
            AirMove();
            hasSlid = false; //enable for repeated slide boosts
        }

        KillVelocityIfHitHead();
        Crouch();

        clampedVel = Vector3.ClampMagnitude(new Vector3(playerVelocity.x, 0, playerVelocity.z), maxVelocity);
        clampedVel.y = playerVelocity.y;

        characterController.Move(clampedVel * Time.deltaTime);

        /* Calculate top velocity */
        uiMaxVel = clampedVel;
        uiMaxVel.y = 0.0f;
        playerTopVelocity = Mathf.Max(playerTopVelocity, uiMaxVel.magnitude);

    }

    #region Detection 
    private bool OnSlope()
    {
        if (!isGrounded) return false;

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + groundRayDistance, ~shurikenLayer)) //ignore shurikens because of the box collider edges
        {
            if (Mathf.Abs(slopeHit.normal.y) < 0.99f)
            {
                slopeNormal = slopeHit.normal;
                slopeAngle = Vector3.Angle(transform.up, slopeNormal);
                return true;
            }
        }
        return false;
    }

    public bool CanStandUp()
    {
        Vector3 checkPosition = transform.position + Vector3.up * standHeight;

        return !Physics.CheckSphere(checkPosition, 0.1f, ~shurikenLayer);
    }

    private bool ExtendedGroundCheck()
    {
        float checkDistance = 0.1f; // extend as needed
        Vector3 origin = transform.position + Vector3.up * 0.1f; // small offset to avoid self-hit
        return Physics.Raycast(origin, Vector3.down, out RaycastHit hit, checkDistance, shurikenLayer);
    }

    #endregion

    #region Input
    //Set movement direction based on player input
    void HandleMovementInput()
    {
        if (wallRun.wallRunning)
            inputVector.x = 0;
        else
            inputVector.x = Input.GetAxisRaw("Horizontal");

        inputVector.z = Input.GetAxisRaw("Vertical");
        wishdir = new Vector3(inputVector.x, 0, inputVector.z);

        //Smooth input for animation transitions, instead of -1/0/1 from GetAxisRaw
        float animationSmoothSpeed = 3f;
        animZInput = Mathf.Lerp(animZInput, inputVector.z, Time.deltaTime * animationSmoothSpeed);
        animXInput = Mathf.Lerp(animXInput, inputVector.x, Time.deltaTime * animationSmoothSpeed);
    }

    //Queues jumps to allow for bhopping
    void QueueJump()
    {
        bool jumpInput = holdToBHop ? Input.GetButton("Jump") : Input.GetButtonDown("Jump");

        if (jumpInput && !wishJump) //queue jump with jump input
        {
            wishJump = true;
            slopeForce = jumpVel;
            canWallRun = true;
        }
        if (Input.GetButtonUp("Jump")) //this block is needed because it kills the jump input, so players can't just spam jump midair and bhop 'easily'
        {
            wishJump = false;
            slopeForce = 3f * -jumpVel;
            canWallRun = false;
        }
    }
    void HandleCameraMovement()
    {
        rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity * 0.02f;
        rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * 0.02f;

        //Lock vertical mouse movement if 3rd person, clamp it so you can't look past your feet and straight up
        if (camSwap.FirstPersonCamera.activeInHierarchy)
        {
            rotX = Mathf.Clamp(rotX, -90f, 90f);
        }
        else if (camSwap.ThirdPersonCamera.activeInHierarchy)
        {
            rotX = Mathf.Clamp(rotX, 0f, 30f);
        }

        transform.rotation = Quaternion.Euler(0, rotY, 0);
        playerView.rotation = Quaternion.Euler(rotX, rotY, wallRun.currentTilt); //currentTilt is wall-running tilt
    }
    #endregion

    /*******************************************************************************************************\
   |* MOVEMENT
   \*******************************************************************************************************/

    #region Jumping and gravity
    void Jump()
    {
        if (wishJump)
        {
            playerVelocity.y = jumpVel;
            //playerVelocity.y = jumpSpeed;
            if (canSlideJump && crouched)
            {
                SlideJump();
            }
            wishJump = false;
            slopeForce = 3f * -jumpVel;
            //slopeForce = 3f * -jumpSpeed;
        }
    }

    void SlideJump()
    {
        playerVelocity.x += playerVelocity.x * slideJumpForce;
        playerVelocity.z += playerVelocity.z * slideJumpForce;
        canSlideJump = false;
        slide = false;
    }

    void Gravity()
    {
        if (!isGrounded)
        {
            playerVelocity.y -= gravity * Time.deltaTime;
        }
        else //don't apply grav when grounded, otherwise it builds up over time
        {
            playerVelocity.y = -gravity * Time.deltaTime;
        }
    }

    void KillVelocityIfHitHead()
    {
        if ((characterController.collisionFlags & CollisionFlags.Above) != 0 && playerVelocity.y > 0f)
        {
            playerVelocity.y = 0f;
        }
    }

    #endregion

    #region Crouching/sliding
    void Crouch()
    {
        //Check for crouching input
        bool crouchInput = Input.GetKey(KeyCode.LeftControl);
        bool crouchPressedThisFrame = crouchInput && !wasCrouchingLastFrame;
        wasCrouchingLastFrame = crouchInput;

        if (crouchInput) crouched = true;
        else if (CanStandUp()) crouched = false;

        //If crouch input is detected, set collider height
        if (crouched)
        {
            characterController.height = Mathf.Lerp(crouchHeight, playerHeight, Time.deltaTime);

            if (crouchPressedThisFrame)
            {
                slideQueue = true;
            }

            Vector3 xzVel = new Vector3(playerVelocity.x, 0, playerVelocity.z);
            bool hasProperSpeed = xzVel.magnitude > 1f && xzVel.magnitude < maxVelocity; //under max velocity & close to default run speed (i.e. not accelerating)
            bool canSlide = hasProperSpeed && !hasSlid && inputVector.z > 0;

            if (canSlide)
            {
                if (isGrounded && slideQueue)
                {
                    slide = true;
                    StartCoroutine(SlideBoost());
                }
            }
        }

        //Reset collider height if there is no crouch input
        else if (CanStandUp() && !crouched)
        {
            characterController.height = Mathf.Lerp(standHeight, playerHeight, Time.deltaTime);
            hasSlid = false;
            slide = false;
        }

        capsule.height = characterController.height;
    }

    //Movespeed boost if sliding
    IEnumerator SlideBoost()
    {
        playerVelocity += clampedVel * slideForce;
        hasSlid = true;
        canSlideJump = true;
        slideQueue = false;
        yield return new WaitForSeconds(slideBoostDuration);

        slide = false;//for animation
    }

    void HandleSlope()
    {
        float slopeAccel = 0.5f * Mathf.Sin(slopeAngle * Mathf.Deg2Rad); //Accelerate faster depending on slope angle

        if (crouched)
        {
            slopeSlide = true;

            if (slopeNormal.z != 0)
            {
                playerVelocity.z += slopeNormal.z > 0 ? slopeAccel : -slopeAccel; //if normal > 0, + accel otherwise - accel
            }
            if (slopeNormal.x != 0)
            {
                playerVelocity.x += slopeNormal.x > 0 ? slopeAccel : -slopeAccel;
            }
        }
        else
        {
            slopeSlide = false;
        }

        playerVelocity.y = slopeForce;
    }
    #endregion

    #region Air strafing
    //Air strafing
    void AirMove()
    {
        //If there is diagonal input, cancel the forward input
        if (inputVector.z >= 0 && inputVector.x != 0)
        {
            wishdir = new Vector3(inputVector.x, 0, 0);
        }

        //Set the direction of the desired direction vector to be the local direction of the player and save the directional vector
        wishdir = transform.TransformDirection(wishdir).normalized;

        //Save direction vector magnitude value, multiplying it by the movespeed for acceleration value
        float wishspeed = wishdir.magnitude * moveSpeed;
        float wishspeedOriginal = wishspeed;

        //Default air acceleration
        float accel = airAcceleration;

        //Change acceleration value if there is sideways input for sideways strafing
        if (inputVector.x != 0)
        {
            if (wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        float modifiedWishspeed = crouched ? wishspeed / 3f : wishspeed; //if you're crouched, your desired speed and acceleration are cut in 1/3
        float modifiedAccel = crouched ? accel / 3f : accel;

        if (airControl > 0)
        {
            AirControl(wishdir, wishspeedOriginal);
        }

        Accelerate(wishdir, modifiedWishspeed, modifiedAccel);
    }

    /**
     * Air control occurs when the player is in the air, it allows
     * players to move side to side much faster rather than being
     * 'sluggish' when it comes to cornering.
     */
    void AirControl(Vector3 wishdir, float wishspeed)
    {
        if (wishspeed == 0 || Mathf.Abs(inputVector.z) == 0)
        {
            return;
        }

        float yVel = playerVelocity.y;
        playerVelocity.y = 0;
        float speed = playerVelocity.magnitude; //only take the XZ velocity
        playerVelocity.Normalize(); //change to unit vector to compare to wishdir, which is also normalized

        /*
         * Most important part - Dot product compares similarity in direction between desired direction (which is the input translated to local space, which changes based on camera rotation)
         * and current velocity direction.
         * 
         * 32 is a scaling factor to make the value more significant, I just found it online lol
         * 
         * Dot is squared so it's only aligned to the forward direction -> e.g. Dot(45 degrees) = 0.707 * 0.707 = 0.5, therefore 45 degrees = 0.5 (half).
         * Perfect forward = 1, 90 degrees = 0 
         * This means that, without strafing, players' air control is lower (e.g. if you want to move 45 degrees from your current facing direction, you only get 50% of the acceleration)
         */

        float dot = Vector3.Dot(playerVelocity, wishdir);
        float airControlCoefficient = 32f * airControl * dot * Time.deltaTime;


        //The 2nd normalize ensures that it's still a direction vector. You add the desired direction * air control factor, which changes direction of velocity
        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishdir.x * airControlCoefficient;
            playerVelocity.z = playerVelocity.z * speed + wishdir.z * airControlCoefficient;

            playerVelocity.Normalize();
        }

        //Reapply the previous speed with the newly adjusted direction
        playerVelocity.x *= speed;
        playerVelocity.z *= speed;
        playerVelocity.y = yVel; //reapply y velocity (unaffected by strafing)
    }
    #endregion

    #region Ground movement and friction
    /**
     * Called every frame when the engine detects that the player is on the ground
     */
    void GroundMove()
    {
        //No friction midair, full friction on ground, 10% friction crouched
        float frictionRate = 0f;

        Vector3 horizontalVel = new Vector3(playerVelocity.x, 0, playerVelocity.z); //Get player's horizontal velocity
        float speed = horizontalVel.magnitude;

        if (!wishJump && !crouched)
            frictionRate = 1f;
        else if (crouched)
            frictionRate = 0.1f;

        ApplyFriction(frictionRate);

        //Required for orientation purposes
        wishdir = transform.TransformDirection(wishdir).normalized;

        //Get magnitude of input (-1, 0, or 1) and multiply it by the movespeed
        float wishspeed = wishdir.magnitude * moveSpeed;
        float speedFactor = (crouched && !OnSlope()) ? 0.5f : 1f; //if you're crouched and not on a slope, your speed gets cut in half

        Accelerate(wishdir, wishspeed * speedFactor, runAcceleration);
    }

    /**
     * Applies friction to the player, called in both the air and on the ground
     */
    public void ApplyFriction(float frictionRate)
    {
        Vector3 horizontalVel = new Vector3(playerVelocity.x, 0, playerVelocity.z); //Get player's horizontal velocity
        float speed = horizontalVel.magnitude; //Player velocity magnitude
        float control = speed < moveSpeed ? runDeceleration : speed; //If velocity > deceleration, then friction is higher, otherwise this value = deceleration -> slow down quickly when over max run speed
        float drop = control * frictionValue * Time.deltaTime * frictionRate; // Deceleration * friction * Time required to reach 0 (friction and decel compound)
        float newSpeed = speed - drop; //speed - drop

        if (newSpeed < 0)
        {
            newSpeed = 0; //Cannot have negative speed (backwards accel)
        }

        if (speed > 0)
        {
            newSpeed /= speed; //Divide by original speed so it is a % reduction rather than a flat rate
        }

        playerVelocity.x *= newSpeed;
        playerVelocity.z *= newSpeed;

        //playerVelocity.x -= frictionValue * frictionRate * playerVelocity.x * Time.deltaTime;
        //playerVelocity.z -= frictionValue * frictionRate * playerVelocity.z * Time.deltaTime;
    }
    #endregion

    #region Movement function
    //Movement function - Requires direction vector, current speed value and acceleration value
    public void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float accelspeed = accel * Time.deltaTime * moveSpeed;
        float desiredspeed = Vector3.Dot(playerVelocity, wishdir);

        //Desired speed minus the current speed
        float addspeed = wishspeed - desiredspeed;

        //Do nothing if the desired speed is less than the dot product of the current and desired direction vectors
        if (addspeed <= 0)
            return;

        if (accelspeed > addspeed)
            accelspeed = addspeed;

        //Apply acceleration
        playerVelocity += accelspeed * wishdir;
    }
    #endregion
}