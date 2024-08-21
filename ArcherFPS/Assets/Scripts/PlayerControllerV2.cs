using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerV2 : MonoBehaviour
{
    public Transform playerView;     // Camera
    public float xMouseSensitivity = 30.0f;
    public float yMouseSensitivity = 30.0f;
    //
    /*Frame occuring factors*/
    public float gravity = 25f;
    public float friction = 5f; //Ground friction

    /* Movement stuff */
    public float moveSpeed = 10.0f;                // Ground move speed
    public float maxVelocity = 15.0f;
    float minVelocity;
    public float runAcceleration = 7.5f;         // Ground accel
    float runDeacceleration = 7.5f;       // Deacceleration that occurs when running on the ground
    float airAcceleration = 1.0f;          // Air accel

    float airDeceleration = 1.0f;
    float airControl = 0.3f;               // How precise air control is
    float sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed
    float sideStrafeSpeed = 1.0f;          // Max speed to generate when side strafing
    public float jumpSpeed = 8.0f;                // The speed at which the character's up axis gains when hitting jump

    public bool unlockMouse = true;
    public Vector3 inputVector = Vector3.zero;

    //Crouching
    public bool crouched;
    public bool slide;
    public Vector3 wishdir;
    public bool canWallRun;

    Vector3 udp;

    CharacterController characterController;
    CapsuleCollider capsule;

    // Camera rotations
    public float rotX;
    public float rotY;

    Vector3 moveDirectionNorm = Vector3.zero;
    public Vector3 playerVelocity = Vector3.zero;
    public float playerTopVelocity = 0.0f;


    // Queue the next jump just before you hit the ground
    public bool wishJump = false;


    public bool useGravity = true;
    public bool isGrounded;
    float playerHeight;

    //Slope variables
    float groundRayDistance = 1f;
    RaycastHit slopeHit;
    float slopeForce;
    Vector3 slopeNormal;
    float slopeAngle;

    RaycastHit roofHit;
    RaycastHit groundHit;

    [SerializeField] CameraSwap camSwap;
    [SerializeField] PlayerAbilities playerAbilities;

    void Start()
    {
        // Hide the cursor
        Cursor.visible = false;

        //Set camera
        if (playerView == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                playerView = mainCamera.gameObject.transform;
        }

        //Set up static variables
        characterController = GetComponent<CharacterController>();
        capsule = GetComponent<CapsuleCollider>();
        camSwap = GetComponent<CameraSwap>();
        minVelocity = -maxVelocity;
        slopeForce = 3f * -jumpSpeed;
        canWallRun = true;
    }

    void Update()
    {
        //Set camera
        playerView = Camera.main.gameObject.transform;

        CameraMovement();
        SetMovementDir();

        isGrounded = characterController.isGrounded;
        playerHeight = characterController.height;

        //Movement
        QueueJump();
        if (isGrounded)
        {
            GroundMove();
        }
        else if (!isGrounded)
        {
            AirMove();
        }
        Gravity();
        Crouch();

        //Apply downward force to smooth slope movement
        if (OnSlope())
        {
            HandleSlope();
        }

        characterController.Move(playerVelocity * Time.deltaTime);

        /* Calculate top velocity */
        udp = playerVelocity;
        udp.y = 0.0f;
        if (udp.magnitude > playerTopVelocity)
            playerTopVelocity = udp.magnitude;

    }

    #region Detection 
    private void GetSlopeNormal()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + groundRayDistance))
        {
            slopeNormal = slopeHit.normal;
            slopeAngle = Vector3.Angle(transform.up, slopeNormal);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.tag == "AntiGravity")
        {
            useGravity = false;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.transform.tag == "AntiGravity")
        {
            useGravity = true;
        }
    }

    private bool OnSlope()
    {
        if (!isGrounded)
        {
            return false;
        }

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + groundRayDistance))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    public bool CanStandUp()
    {
        if (Physics.SphereCast(transform.position, 0.01f, transform.up, out roofHit, 1f))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    #endregion

    #region Input
    //Set movement direction based on player input
    void SetMovementDir()
    {
        inputVector.z = Input.GetAxisRaw("Vertical");
        inputVector.x = Input.GetAxisRaw("Horizontal");
        wishdir = new Vector3(inputVector.x, 0, inputVector.z);
    }

    //Queues jumps to allow for bhopping
    void QueueJump()
    {
        if (Input.GetButtonDown("Jump") && !wishJump)
        {
            wishJump = true;
            slopeForce = jumpSpeed;
            canWallRun = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            wishJump = false;
            slopeForce = 3f * -jumpSpeed;
            canWallRun = false;
        }
    }
    void CameraMovement()
    {
        if (unlockMouse)
        {
            //Lock vertical mouse movement if 3rd person
            if (camSwap.cam == 0)
            {
                rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity * 0.02f;

                // Clamp vertical rotation of camera
                if (rotX < -90)
                    rotX = -90;
                else if (rotX > 90)
                    rotX = 90;
            }
            else if (camSwap.cam == 1)
            {
                rotX = 15;
            }

            //Get horizontal mouse movement
            rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * 0.02f;

            //Rotate player
            this.transform.rotation = Quaternion.Euler(0, rotY, 0);

            float cameraTilt = playerAbilities.currentTilt;
            //Rotate camera
            playerView.rotation = Quaternion.Euler(rotX, rotY, cameraTilt);
        }
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
            playerVelocity.y = jumpSpeed;
            wishJump = false;
            if (playerAbilities.canSlideJump)
            {
                playerAbilities.updraftInput = true;
            }
        }
    }

    void Gravity()
    {
        if (useGravity)
        {
            playerVelocity.y -= gravity * Time.deltaTime;
        }
        else if (!useGravity)
        {
            if (playerVelocity.y < 0)
            {
                playerVelocity.y = Mathf.Lerp(playerVelocity.y, 0, 0.1f);
            }
            playerVelocity.y += gravity * Time.deltaTime;
        }
    }
    #endregion

    #region Crouching/sliding
    void Crouch()
    {
        //Check for crouching input
        crouched = Input.GetKey(KeyCode.LeftControl);

        //If crouch input is detected, set collider height
        if (crouched)
        {
            characterController.height = Mathf.Lerp(0.6f, playerHeight, Time.deltaTime);
            Vector3 horizontalVel = new Vector3(playerVelocity.x, 0, playerVelocity.z);
            if (playerVelocity.magnitude < maxVelocity && horizontalVel.magnitude > 0.5f && !playerAbilities.hasSlid)
            {
                playerAbilities.canSlide = true;
            }
        }

        //Reset collider height if there is no crouch input
        else if (CanStandUp())
        {
            characterController.height = Mathf.Lerp(1.8f, playerHeight, Time.deltaTime);
            playerAbilities.canSlide = false;
            //playerAbilities.isSliding = false;
            playerAbilities.hasSlid = false;
        }

        capsule.height = characterController.height;
    }

    void HandleSlope()
    {
        GetSlopeNormal();
        float slopeAccel = 0.5f * Mathf.Sin(slopeAngle * Mathf.Deg2Rad); //Accelerate faster depending on slope angle
        if (crouched)
        {
            if (slopeNormal.z != 0)
            {
                if (slopeNormal.z > 0)
                {
                    playerVelocity.z += slopeAccel;
                }
                else if (slopeNormal.z < 0)
                {
                    playerVelocity.z -= slopeAccel;
                }
            }
            if (slopeNormal.x != 0)
            {
                if (slopeNormal.x > 0)
                {
                    playerVelocity.x += slopeAccel;
                }
                else if (slopeNormal.x < 0)
                {
                    playerVelocity.x -= slopeAccel;
                }
            }
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

        //Set the direction of the desired direction vector to be the local direction of the player
        wishdir = transform.TransformDirection(wishdir);

        //Retain direction of direction vector but set magnitude to 1
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        //Save direction vector magnitude value, multiplying it by the movespeed for acceleration value
        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        //Accelerate if strafing in same direction, decelerate if different
        float wishspeed2 = wishspeed;
        float accel;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
        {
            accel = airDeceleration;
        }
        else
        {
            accel = airAcceleration;
        }

        //Change acceleration value if there is sideways input for sideways strafing
        if (inputVector.x != 0)
        {
            if (wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        if (crouched && playerVelocity.magnitude < 0.5f)
        {
            Accelerate(wishdir, wishspeed / 3, runAcceleration);
        }
        else
        {
            Accelerate(wishdir, wishspeed, accel);
        }

        if (airControl > 0)
            AirControl(wishdir, wishspeed2);

    }

    /**
     * Air control occurs when the player is in the air, it allows
     * players to move side to side much faster rather than being
     * 'sluggish' when it comes to cornering.
     */
    void AirControl(Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if (Mathf.Abs(inputVector.z) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;
        zspeed = playerVelocity.y;
        playerVelocity.y = 0;
        // Next two lines are equivalent to idTech's VectorNormalize()
        speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        dot = Vector3.Dot(playerVelocity, wishdir);
        k = 32;
        k *= airControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
            playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

            playerVelocity.Normalize();
            moveDirectionNorm = playerVelocity;
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zspeed; // Note this line
        playerVelocity.z *= speed;
    }
    #endregion

    #region Ground movement and friction
    /**
     * Called every frame when the engine detects that the player is on the ground
     */
    void GroundMove()
    {
        // Do not apply friction if the player is queueing up the next jump - Allows for bhopping
        if (!wishJump && !crouched)
        {
            ApplyFriction(1.0f);
        }
        else if (crouched)
        {
            ApplyFriction(0.25f);
        }
        else
        {
            ApplyFriction(0);
        }


        //Save movement vector direction, but set magnitude to 1
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        //Required for orientation purposes
        wishdir = transform.TransformDirection(wishdir);

        //Get magnitude of input (-1, 0, or 1) and multiply it by the movespeed
        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        if (!crouched || OnSlope())
        {
            Accelerate(wishdir, wishspeed, runAcceleration);
        }
        else if (crouched && !OnSlope())
        {
            Accelerate(wishdir, wishspeed / 2, runAcceleration);
        }

        //Reset gravity velocity to smooth falling
        playerVelocity.y = -gravity * Time.deltaTime;

        Jump();
    }

    /**
     * Applies friction to the player, called in both the air and on the ground
     */
    public void ApplyFriction(float frictionRate)
    {
        Vector3 horizontalVel = new Vector3(playerVelocity.x, 0, playerVelocity.z); //Get player's horizontal velocity
        float speed = horizontalVel.magnitude; //Player velocity magnitude
        float newSpeed; //speed - drop
        float control = speed < runDeacceleration ? runDeacceleration : speed; //If velocity > deceleration, then friction is higher, otherwise this value = deceleration
        float drop = control * friction * Time.deltaTime * frictionRate; // Deceleration * friction * Time required to reach 0 (friction and decel compound)

        newSpeed = speed - drop;

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

        //Truncate velocity
        playerVelocity.x = Mathf.Clamp(playerVelocity.x, minVelocity, maxVelocity);
        playerVelocity.z = Mathf.Clamp(playerVelocity.z, minVelocity, maxVelocity);
    }
    #endregion
}