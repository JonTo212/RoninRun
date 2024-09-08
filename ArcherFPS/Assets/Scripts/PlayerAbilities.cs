using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    //Dash particles
    [SerializeField] ParticleSystem forwardDashParticles;
    [SerializeField] ParticleSystem backwardDashParticles;
    [SerializeField] ParticleSystem rightDashParticles;
    [SerializeField] ParticleSystem leftDashParticles;

    //Updraft particles
    [SerializeField] ParticleSystem updraftParticles;

    //Dashing variables
    public bool isDashing;
    public bool dashCooldownActive;
    float dashStartTime;
    Vector3 currentForward;
    Vector3 currentVel;
    public float dashPower = 30f;
    public bool hasDashed;

    //Updraft variables
    public bool canUpdraft;
    public bool isUpdrafting;
    float updraftStartTime = 0f;
    float updraftHeight = 15f;
    public bool updraftInput;

    PlayerControllerV2 playerController;
    CharacterController characterController;
    public ParticleSystem currentParticles;

    //WallRun variables
    float wallDistance = 1f;
    public float minimumJumpHeight = 2f;
    public float wallJumpForce = 15f;
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

    float camTilt = 15f;
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
    }
    void Update()
    {
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

    #region Dashing
    void HandleDash()
    {
        bool isTryingToDash = Input.GetKeyDown(KeyCode.E);

        if (isTryingToDash && !isDashing && !hasDashed)
        {
            OnStartDash();
        }

        if (isDashing)
        {
            if (Time.time - dashStartTime <= 0.4f)
            {
                characterController.Move(currentForward * dashPower * Time.deltaTime);
                
                /*if (currentVel.Equals(Vector3.zero)) //Sideways 
                {
                    //No input, dash forward
                    characterController.Move(currentForward * dashPower * Time.deltaTime);
                }
                else
                {
                    characterController.Move(currentVel * dashPower * Time.deltaTime);
                }*/
            }
            else
            {
                OnDashEnd();
            }
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
            wallRunJumpDirection = transform.up + wallNormal;
            jumpAccel = wallRunJumpDirection * wallJumpForce;

            // Cancel out previous velocity if it's in the opposite direction of the jump
            CancelOpposingVelocity(ref playerController.playerVelocity, jumpAccel);

            playerController.playerVelocity += jumpAccel;
            canWallBounce = false;
            hasWallBounced = true;
            wallRunning = false;
        }
    }

    void CancelOpposingVelocity(ref Vector3 velocity, Vector3 jumpAcceleration)
    {
        if (Mathf.Sign(velocity.x) != Mathf.Sign(jumpAcceleration.x) && Mathf.Abs(jumpAcceleration.x) > 0.01f)
        {
            velocity.x = 0;
        }
        if (Mathf.Sign(velocity.z) != Mathf.Sign(jumpAcceleration.z) && Mathf.Abs(jumpAcceleration.z) > 0.01f)
        {
            velocity.z = 0;
        }
    }
    #endregion
}