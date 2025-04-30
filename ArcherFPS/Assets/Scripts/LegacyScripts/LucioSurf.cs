using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LucioSurf : MonoBehaviour
{
    PlayerController playerController;
    float wallDistance = 1f;
    public float minimumJumpHeight = 2f;
    public float surfBoostForce = 15f;
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
    //public LayerMask playerMask;
    public LayerMask wallMask;
    public bool wallRun;
    //public float wallBounceActivateTimer; //activate if spacebar hold
    //float timer;    

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

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        //playerMask = ~playerMask; //allows for wallrunning on all surfaces
    }

    void Update()
    {
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
        
        /*if (timer >= wallBounceActivateTimer)
        {
            canWallBounce = true; //activate for spacebar hold
        }*/

        WallBounce();

        currentTilt = Mathf.Lerp(currentTilt, 0, camTiltTime * Time.deltaTime);
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallDistance, wallMask);
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallDistance, wallMask);

        if (wallLeft || wallRight)
        {
            wallHit = true;
        }
        else
        {
            wallHit = false;
        }
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

        canWallBounce = true; //remove if spacebar hold
    }

    void WallBounce()
    {
        if (canWallBounce && !hasWallBounced)
        {
            if (wallLeft)
            {
                //Wall jump
                if (Input.GetKeyDown(KeyCode.Space)) //change to getkeyup for space hold
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
                if (Input.GetKeyDown(KeyCode.Space))
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
