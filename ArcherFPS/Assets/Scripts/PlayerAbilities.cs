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
    float lastTimeUpdrafted = 0f;
    float updraftHeight = 15f;
    float updraftDelay = 0.2f;
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

    float camTilt = 30f;
    float camTiltTime = 3f;
    public float currentTilt;
    public LayerMask wallMask;
    public bool wallRun;
    //public float wallBounceActivateTimer; //activate if spacebar hold
    //float timer; 

    void Start()
    {
        playerController = GetComponent<PlayerControllerV2>();
        characterController = GetComponent<CharacterController>();
    }
    void Update()
    {
        HandleDash();
        HandleUpdraft();

        if (playerController.isGrounded || !wallHit)
        {
            canWallBounce = false;
            hasWallBounced = false;
            //canUpdraft = false;
            //timer = 0;
        }

        CheckWall();

        if (CanWallRun())
        {
            if (wallLeft)
            {
                currentTilt = Mathf.Lerp(currentTilt, -camTilt, camTiltTime * Time.deltaTime);
                WallRunning(leftWallHit);
                //timer += Time.deltaTime;
            }
            else if (wallRight)
            {
                currentTilt = Mathf.Lerp(currentTilt, camTilt, camTiltTime * Time.deltaTime);
                WallRunning(rightWallHit);
                //timer += Time.deltaTime;
            }
            else
            {
                wallRunning = false;
                //timer = 0;
            }
        }

        currentTilt = Mathf.Lerp(currentTilt, 0, camTiltTime * Time.deltaTime);
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
                if (currentVel.Equals(Vector3.zero))
                {
                    //No input, dash forward
                    characterController.Move(currentForward * dashPower * Time.deltaTime);
                }
                else
                {
                    characterController.Move(currentVel * dashPower * Time.deltaTime);
                }
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

        if (updraftInput && canUpdraft)
        {
            if (Time.time - lastTimeUpdrafted < updraftDelay)
            {
                if (isUpdrafting)
                {
                    OnUpdraftEnd();
                }
                return;
            }
            OnUpdraftStart();
            Updraft();
        }
    }

    void Updraft()
    {
        playerController.playerVelocity.y = updraftHeight;
    }

    void OnUpdraftStart()
    {
        isUpdrafting = true;
        lastTimeUpdrafted = Time.time;
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
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight) && playerController.canWallRun; //for spacebar hold wallrunning
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallDistance, wallMask);
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallDistance, wallMask);

        wallHit = wallLeft || wallRight;
    }

    void WallRunning(RaycastHit wallInfo)
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


        //Apply small force to stick to wall
        wallStick = wallInfo.normal * 5f * Time.deltaTime;
        playerController.playerVelocity -= wallStick;

        //Walljumping
        if (!hasWallBounced && Input.GetKeyUp(KeyCode.Space)) // change to GetKeyUp for space hold
        {
            Vector3 wallNormal = wallInfo.normal;
            wallRunJumpDirection = transform.up + wallNormal;
            jumpAccel = wallRunJumpDirection * wallJumpForce;

            // Cancel out previous velocity if it's in the opposite direction of the jump
            CancelOpposingVelocity(ref playerController.playerVelocity, jumpAccel);

            playerController.playerVelocity += jumpAccel;
            canWallBounce = false;
            hasWallBounced = true;
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