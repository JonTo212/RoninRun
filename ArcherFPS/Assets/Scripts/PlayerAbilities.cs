using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    //Slide particles
    [SerializeField] ParticleSystem forwardSlideParticles;
    [SerializeField] ParticleSystem backwardSlideParticles;
    [SerializeField] ParticleSystem rightSlideParticles;
    [SerializeField] ParticleSystem leftSlideParticles;

    //Updraft particles
    [SerializeField] ParticleSystem updraftParticles;

    //Floating particles
    [SerializeField] ParticleSystem floatingParticles;

    //Dashing variables
    public bool canSlide;
    public bool hasSlid;
    public bool isSliding;
    float slideStartTime;
    Vector3 currentForward;
    Vector3 currentVel;
    public float slidePower = 30f;
    public bool canSlideJump;
    public float slideJumpHorizontalForce;
    public float slideJumpVerticalForce;

    //Updraft variables
    public bool isUpdrafting = false;
    float lastTimeUpdrafted = 0f;
    float updraftHeight = 15f;
    float updraftDelay = 0.2f;
    public float updraftAttempts = 0;
    public float maxUpdrafts = 2;
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

    public bool wallLeft = false;
    public bool wallRight = false;
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
        HandleSlide();
        HandleUpdraft();

        if (playerController.isGrounded || !wallHit)
        {
            canWallBounce = false;
            hasWallBounced = false;
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

    #region Sliding
    public void HandleSlide()
    {
        if (!isSliding && playerController.isGrounded && !hasSlid && canSlide)
        {
            OnStartSlide();
        }

        if (isSliding)
        {
            if (Time.time - slideStartTime <= 0.4f)
            {
                if (currentVel.Equals(Vector3.zero))
                {
                    //No input, slide forward
                    characterController.Move(currentForward * slidePower * Time.deltaTime);
                }
                else
                {
                    characterController.Move(currentVel * slidePower * Time.deltaTime);
                }
            }
            else
            {
                OnSlideEnd();
            }
        }
    }

    void OnStartSlide()
    {
        playerController.playerVelocity.z = 0;
        isSliding = true;
        slideStartTime = Time.time;
        currentForward = transform.forward;
        currentVel = playerController.wishdir;
        canSlideJump = true;
        PlaySlideParticles();
    }

    public void OnSlideEnd()
    {
        isSliding = false;
        slideStartTime = 0;
        hasSlid = true;
        canSlide = false;
        canSlideJump = false;
    }

    void PlaySlideParticles()
    {
        Vector3 inputVector = playerController.inputVector;

        if (inputVector.z > 0 && Mathf.Abs(inputVector.x) <= inputVector.z)
        {
            //Forward and forward-diagonal dashes play the forward dash particles
            forwardSlideParticles.Play();
            currentParticles = forwardSlideParticles;
            return;
        }

        if (inputVector.z < 0 && Mathf.Abs(inputVector.x) >= inputVector.z)
        {
            //Backward and backward-diagonal dashes play the backward dash particles
            backwardSlideParticles.Play();
            currentParticles = backwardSlideParticles;
            return;
        }

        if (inputVector.x > 0)
        {
            rightSlideParticles.Play();
            currentParticles = rightSlideParticles;
            return;
        }

        if (inputVector.x < 0)
        {
            leftSlideParticles.Play();
            currentParticles = leftSlideParticles;
            return;
        }

        forwardSlideParticles.Play();
        currentParticles = forwardSlideParticles;
    }

    #endregion

    #region Updrafting
    public void HandleUpdraft()
    {
        if (updraftInput)
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
    }

    void OnUpdraftEnd()
    {
        isUpdrafting = false;
        canSlideJump = false;
        updraftInput = false;
    }
    #endregion

    #region WallRunning

    bool CanWallRun() //Can wall run if you're above the minimum jump height
    {
        if (!wallRun)
        {
            return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight) && playerController.canWallRun; //for spacebar hold wallrunning
        }
        else if (wallRun)
        {
            return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
        }
        return false;
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
        if (!hasWallBounced && Input.GetKeyDown(KeyCode.Space)) // change to GetKeyUp for space hold
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