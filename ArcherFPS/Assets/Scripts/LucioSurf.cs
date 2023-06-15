using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LucioSurf : MonoBehaviour
{
    PlayerController playerController;
    float wallDistance = 1f;
    float minimumJumpHeight = 2f;
    public float surfBoostForce = 15f;
    Vector3 wallRunJumpDirection;
    Vector3 jumpAccel;
    Vector3 wallStick;
    bool canWallBounce;
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
    public LayerMask playerMask;
    public bool wallRun;

    bool CanWallRun() //Can wall run if you're above the minimum jump height
    {
        if (!wallRun)
        {
            return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight) && playerController.canWallRun;
        }
        else if (wallRun)
        {
            return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
        }
        return false;
    }

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerMask = ~playerMask;
    }

    void Update()
    {
        if (playerController.isGrounded || !wallHit)
        {
            canWallBounce = false;
            hasWallBounced = false;
        }

        CheckWall();

        if (CanWallRun())
        {
            if (wallLeft)
            {
                wallRunning = true;
                WallRunning();
            }
            else if (wallRight)
            {
                wallRunning = true;
                WallRunning();
            }
            else
            {
                wallRunning = false;
            }
        }

        WallBounce();

        currentTilt = Mathf.Lerp(currentTilt, 0, camTiltTime * Time.deltaTime);
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallDistance, playerMask);
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallDistance, playerMask);

        if (wallLeft || wallRight)
        {
            wallHit = true;
        }
        else
        {
            wallHit = false;
        }
    }

    void WallRunning()
    {
        if (playerController.playerVelocity.y < 0)
        {
            playerController.playerVelocity = new Vector3(playerController.playerVelocity.x, playerController.playerVelocity.y / 1.5f, playerController.playerVelocity.z);
        }

        if (wallLeft)
        {
            //Calculate and apply camera tilt
            currentTilt = Mathf.Lerp(currentTilt, -camTilt, camTiltTime * Time.deltaTime);

            //Apply small force to stick to wall
            wallStick = leftWallHit.normal * 5f * Time.deltaTime;
            playerController.playerVelocity -= wallStick;

            //Cancel sideways input if there is a wall, to cancel accidental unsticking
            if (playerController.inputVector.x != 0)
            {
                playerController.inputVector.x = 0;
            }
            canWallBounce = true;
        }

        else if (wallRight)
        {
            currentTilt = Mathf.Lerp(currentTilt, camTilt, camTiltTime * Time.deltaTime);

            wallStick = rightWallHit.normal * 1f * Time.deltaTime;
            playerController.playerVelocity -= wallStick;

            if (playerController.inputVector.x != 0)
            {
                playerController.inputVector.x = 0;
            }
            canWallBounce = true;
        }

        //Apply friction if there's no movement input
        if (playerController.wishdir == Vector3.zero)
        {
            playerController.ApplyFriction(0.1f);
        }
    }

    void WallBounce()
    {
        if (canWallBounce && !hasWallBounced)
        {
            if (wallLeft)
            {
                //Wall jump
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    //wallRunJumpDirection = transform.up + transform.right + transform.forward;
                    wallRunJumpDirection = transform.up + leftWallHit.normal; //Diagonal vector between the up and wall normal
                    jumpAccel = wallRunJumpDirection * surfBoostForce;

                    //Cancel out velocity from previous wall jumps (cannot be != 0 due to calculation rounding errors)
                    if (Mathf.Sign(playerController.playerVelocity.x) != Mathf.Sign(jumpAccel.x) && (jumpAccel.x > 0.01f || jumpAccel.x < -0.01f))
                    {
                        playerController.playerVelocity.x = 0;
                    }
                    else if (Mathf.Sign(playerController.playerVelocity.z) != Mathf.Sign(jumpAccel.z) && (jumpAccel.z > 0.01f || jumpAccel.z < -0.01f))
                    {
                        playerController.playerVelocity.z = 0;
                    }
                    playerController.playerVelocity += jumpAccel;
                    canWallBounce = false;
                    hasWallBounced = true;
                }
            }

            if (wallRight)
            {
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    //wallRunJumpDirection = transform.up - transform.right + transform.forward;
                    wallRunJumpDirection = transform.up + rightWallHit.normal;
                    jumpAccel = wallRunJumpDirection * surfBoostForce;

                    if (Mathf.Sign(playerController.playerVelocity.x) != Mathf.Sign(jumpAccel.x) && (jumpAccel.x > 0.01f || jumpAccel.x < -0.01f))
                    {
                        playerController.playerVelocity.x = 0;
                    }
                    else if (Mathf.Sign(playerController.playerVelocity.z) != Mathf.Sign(jumpAccel.z) && (jumpAccel.z > 0.01f || jumpAccel.z < -0.01f))
                    {
                        playerController.playerVelocity.z = 0;
                    }
                    playerController.playerVelocity += jumpAccel;
                    canWallBounce = false;
                    hasWallBounced = true;
                }
            }
        }
    }
}
