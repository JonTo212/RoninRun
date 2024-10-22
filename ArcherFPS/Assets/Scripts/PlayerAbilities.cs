using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    //Dash particles
    ParticleSystem forwardDashParticles;
    ParticleSystem backwardDashParticles;
    ParticleSystem rightDashParticles;
    ParticleSystem leftDashParticles;

    //Updraft particles
    ParticleSystem updraftParticles;

    //Dashing variables
    public bool isDashing;
    public bool dashCooldownActive;
    float dashStartTime;
    Vector3 currentForward;
    Vector3 currentVel;
    public float dashPower = 30f;
    public float dashDelay = 2;
    public float dashCooldown = 0;
    public bool hasDashed;

    //Updraft variables
    public bool canUpdraft;
    public bool isUpdrafting;
    float updraftStartTime = 0f;
    public float updraftHeight = 15f;
    public bool updraftInput;

    PlayerControllerV2 playerController;
    CharacterController characterController;
    public ParticleSystem currentParticles;

    //WallRun variables
    float wallDistance = 1f;
    public float minimumJumpHeight = 2f;
    public Vector2 wallJumpForce;
    Vector3 wallRunJumpDirection;
    Vector3 jumpAccel;
    Vector3 wallStick;
    public bool canWallBounce;
    bool hasWallBounced;

    public bool wallLeft;
    public bool wallRight;
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    public bool wallHit;
    public bool wallRunning;

    public float camTilt = 15f;
    float desiredTilt;
    public float tiltSpeed = 25f;
    public float currentTilt;
    public LayerMask wallMask;
    public bool wallRun;
    [SerializeField] float autoRunForce;

    void Start()
    {
        playerController = GetComponent<PlayerControllerV2>();
        characterController = GetComponent<CharacterController>();
        canUpdraft = true;
    }
    void Update()
    {
        GetParticles();
        HandleDash();
        HandleUpdraft();

        //dev tool
        if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            ResetDash();
        }

        if (playerController.isGrounded || !wallHit)
        {
            canWallBounce = false;
            hasWallBounced = false;
        }

        CheckWall();

        if (CanWallRun())
        {
            if (wallLeft || wallRight)
            {
                WallRunning();
            }
            else
            {
                desiredTilt = 0;
                wallRunning = false;
            }
        }
        else
        {
            wallRunning = false;
            desiredTilt = 0;
        }

        currentTilt = Mathf.MoveTowards(currentTilt, desiredTilt, tiltSpeed * Time.deltaTime);
    }

    void GetParticles()
    {
        forwardDashParticles = GameObject.FindGameObjectWithTag("ForwardDashParticles").GetComponent<ParticleSystem>();
        backwardDashParticles = GameObject.FindGameObjectWithTag("BackwardDashParticles").GetComponent<ParticleSystem>();
        rightDashParticles = GameObject.FindGameObjectWithTag("RightDashParticles").GetComponent<ParticleSystem>();
        leftDashParticles = GameObject.FindGameObjectWithTag("LeftDashParticles").GetComponent<ParticleSystem>();
        updraftParticles = GameObject.FindGameObjectWithTag("UpdraftParticles").GetComponent<ParticleSystem>();
    }

    #region Dashing
    void HandleDash()
    {
        bool isTryingToDash = Input.GetKeyDown(KeyCode.E);
        dashCooldown += Time.deltaTime;

        if (isTryingToDash && !isDashing && dashCooldown > dashDelay)
        {
            OnStartDash();
            dashCooldown = 0;
        }
    }

    void ResetDash()
    {
        hasDashed = false;
    }

    void OnStartDash()
    {
        playerController.playerVelocity.z = 0;
        isDashing = true;
        dashStartTime = Time.time;
        currentForward = transform.forward;
        currentVel = playerController.wishdir;
        dashCooldownActive = false;
        hasDashed = true;
        canUpdraft = true;
        PlayDashParticles();
        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        float dashDuration = 0.4f;

        while (Time.time - dashStartTime <= dashDuration)
        {
            if (currentVel.Equals(Vector3.zero))
            {
                //Dash forward when 0 input
                characterController.Move(currentForward * dashPower * Time.deltaTime);
            }
            else
            {
                characterController.Move(currentVel * dashPower * Time.deltaTime);
            }

            yield return null;
        }

        OnDashEnd();
    }

    void OnDashEnd()
    {
        isDashing = false;
        dashStartTime = 0;
        dashCooldownActive = true;
    }

    void PlayDashParticles()
    {
        Vector3 inputVector = playerController.inputVector;

        if (inputVector.z > 0 && Mathf.Abs(inputVector.x) <= inputVector.z)
        {
            //Forward and forward-diagonal dashes play the forward dash particles
            forwardDashParticles.Play();
            return;
        }

        if (inputVector.z < 0 && Mathf.Abs(inputVector.x) >= inputVector.z)
        {
            //Backward and backward-diagonal dashes play the backward dash particles
            backwardDashParticles.Play();
            return;
        }

        if (inputVector.x > 0)
        {
            rightDashParticles.Play();
            return;
        }

        if (inputVector.x < 0)
        {
            leftDashParticles.Play();
            return;
        }

        forwardDashParticles.Play();
    }
    #endregion

    #region Updrafting
    public void HandleUpdraft()
    {
        updraftInput = Input.GetKeyDown(KeyCode.Q);

        if (updraftInput && canUpdraft && !playerController.isGrounded)
        {
            OnUpdraftStart();
        }

        if (isUpdrafting)
        {
            if (Time.time - updraftStartTime < 0.4f)
            {
                Updraft();
            }
            else
            {
                OnUpdraftEnd();
            }
        }
    }

    void Updraft()
    {
        playerController.playerVelocity.y = updraftHeight;
    }

    void OnUpdraftStart()
    {
        isUpdrafting = true;
        updraftStartTime = Time.time;
        updraftParticles.Play();
        canUpdraft = false;
    }

    void OnUpdraftEnd()
    {
        isUpdrafting = false;
        updraftInput = false;
    }
    #endregion

    #region WallRunning

    bool CanWallRun() //Can wall run if you're above the minimum jump height
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight); //&& playerController.canWallRun; //for spacebar hold wallrunning
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallDistance, wallMask);
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallDistance, wallMask);

        wallHit = wallLeft || wallRight;
    }

    void WallRunning()
    {
        wallRunning = true;

        if (playerController.playerVelocity.y < 0) //cut effects of gravity in 1/2 during wallrunning
        {
            playerController.playerVelocity = new Vector3(playerController.playerVelocity.x, playerController.playerVelocity.y / 2, playerController.playerVelocity.z);
        }

        //Cancel sideways input if there is a wall, to cancel accidental unsticking
        if (playerController.inputVector.x != 0)
        {
            playerController.inputVector.x = 0;
        }

        //Apply friction if there's no movement input
        if (playerController.wishdir == Vector3.zero)
        {
            playerController.ApplyFriction(0.1f);
        }

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        //Apply small force to stick to wall
        wallStick = wallNormal * 5f * Time.deltaTime;
        playerController.playerVelocity -= wallStick;

        if (wallLeft)
        {
            //Set camera tilt
            desiredTilt = -camTilt;
            playerController.playerVelocity += wallForward * autoRunForce * Time.deltaTime;
        }

        else if (wallRight)
        {
            desiredTilt = camTilt;
            playerController.playerVelocity -= wallForward * autoRunForce * Time.deltaTime;

        }

        //Walljumping
        if (!hasWallBounced && Input.GetKeyDown(KeyCode.Space)) // change to GetKeyUp for space hold
        {
            Vector3 wallJumpHorizontal = wallNormal * wallJumpForce.x; // Horizontal component based on wall normal
            Vector3 wallJumpVertical = transform.up * wallJumpForce.y;

            jumpAccel = wallJumpHorizontal + wallJumpVertical;

            Vector3 wallParallelVelocity = Vector3.ProjectOnPlane(playerController.playerVelocity, wallNormal);

            // Cancel out previous velocity if it's in the opposite direction of the jump
            CancelOpposingVelocity(ref playerController.playerVelocity, jumpAccel);

            playerController.playerVelocity = wallParallelVelocity + jumpAccel;
            canWallBounce = false;
            hasWallBounced = true;
            wallRunning = false;
        }
    }

    void CancelOpposingVelocity(ref Vector3 velocity, Vector3 jumpAcceleration)
    {
        if (Vector3.Dot(velocity, jumpAcceleration) < 0) // If velocity is in the opposite direction of jumpAcceleration
        {
            velocity = Vector3.ProjectOnPlane(velocity, jumpAcceleration.normalized); // Cancel perpendicular velocity
        }
    }
    #endregion
}