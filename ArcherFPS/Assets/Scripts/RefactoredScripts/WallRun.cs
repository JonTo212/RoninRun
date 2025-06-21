using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class WallRun : MonoBehaviour
{
    private PlayerControllerV2 playerController;

    private float wallDistance = 1f;
    public float minimumJumpHeight = 2f;
    public Vector2 wallJumpForce;
    [HideInInspector] public bool canWallBounce;
    [SerializeField] [Range(1,2)] private float backWallJumpMultiplier;

    RaycastHit closestWall;
    public Vector3 closestWallNormal;
    [HideInInspector] public bool wallLeft; //anim
    [HideInInspector] public bool wallRight; //anim
    [HideInInspector] public bool wallRunning;
    private Vector3 wallNormal;
    private Vector3 wallForward;

    public float camTilt = 15f;
    private float desiredTilt = 0;
    private float tiltSpeed = 25f;
    [HideInInspector] public float currentTilt;
    public LayerMask wallMask;
    [SerializeField] private float autoRunForce;

    void Awake()
    {
        playerController = GetComponent<PlayerControllerV2>();
    }

    void Update()
    {
        WallBounceReset();
        WallCheck();
        ApplyCameraTilt();
    }

    private void WallBounceReset()
    {
        if (playerController.isGrounded || !CheckWall())
        {
            canWallBounce = false;
        }
    }

    private void WallCheck()
    {
        if (CanWallRun() && CheckWall())
        {
            CalculateCameraTilt();
            HandleWallRun();

            if (canWallBounce && Input.GetKeyUp(KeyCode.Space))
            {
                WallJump();
            }
        }
        else
        {
            StopWallRunning();
        }
    }

    private void StopWallRunning()
    {
        wallRunning = false;
        desiredTilt = 0;
    }

    private void ApplyCameraTilt()
    {
        currentTilt = Mathf.MoveTowards(currentTilt, desiredTilt, tiltSpeed * Time.deltaTime);
    }

    bool CanWallRun() //Can wall run if you're above the minimum jump height
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight); //&& playerController.canWallRun; //for spacebar hold wallrunning
    }

    bool CheckWall()
    {
        Vector3[] directions = 
        {
            transform.forward,
            -transform.forward,
            -transform.right,
            transform.right
        };

        float closest = Mathf.Infinity;
        bool wallDetected = false;
        Vector3 closestDir = Vector3.zero;

        //raycast all 4 directions, if hit then save 
        foreach (var dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit tempHit, wallDistance, wallMask) && tempHit.distance < closest)
            {
                wallDetected = true;
                closest = tempHit.distance;
                closestWall = tempHit;
                wallNormal = tempHit.normal;
                wallForward = Vector3.Cross(wallNormal, transform.up);
                closestDir = dir;
            }
        }

        return wallDetected;
    }

    void HandleWallRun()
    {
        if (playerController.canWallRun)
        {
            //cut effects of gravity in half
            if (playerController.playerVelocity.y < 0)
            {
                float newGrav = playerController.playerVelocity.y / 2;
                playerController.playerVelocity = new Vector3(playerController.playerVelocity.x, newGrav, playerController.playerVelocity.z);
            }

            //apply friction if there's no movement input
            if (playerController.wishdir == Vector3.zero)
            {
                playerController.ApplyFriction(0.5f);
            }
        }

        canWallBounce = true;
    }

    void CalculateCameraTilt()
    {
        float dotRight = Vector3.Dot(-wallNormal, transform.right);
        float dotLeft = Vector3.Dot(-wallNormal, -transform.right);

        if (dotLeft > 0.5f)
        {
            desiredTilt = -camTilt;
            wallLeft = true;
            wallRight = false;
        }
        else if (dotRight > 0.5f)
        {
            desiredTilt = camTilt;
            wallLeft = false;
            wallRight = true;
        }
        else
        {
            desiredTilt = 0f;
            wallLeft = false;
            wallRight = false;
        }
    }

    void HandleWallAutoMovement()
    {
        //use dot product to determine if closest wall is left or right
        float dotLeft = Vector3.Dot(wallNormal, -transform.right);
        float dotRight = Vector3.Dot(wallNormal, transform.right);

        float forwardDir = dotLeft > dotRight ? 1f : -1f;
        playerController.playerVelocity += forwardDir * wallForward * autoRunForce * Time.deltaTime;


        //Apply small force so you don't detach from wall
        Vector3 wallStick = wallNormal * 5f * Time.deltaTime;
        playerController.playerVelocity -= wallStick;
    }


    void WallJump()
    {
        Vector2 force = wallJumpForce;
        playerController.playerVelocity = Vector3.zero;
            
        //dot product to determine if wall is behind you for large boost
        if (Vector3.Dot(wallNormal, transform.forward) > 0.5f)
        {
            force *= backWallJumpMultiplier;
        }
        else if (Vector3.Dot(wallNormal, -transform.forward) > 0.5f)
        {
            force = new Vector3(force.y, force.x);
        }

        Vector3 wallJumpHorizontal = wallNormal * force.x; //horizontal component based on wall normal
        Vector3 wallJumpVertical = transform.up * force.y;

        Vector3 jumpAccel = wallJumpHorizontal + wallJumpVertical;
        Vector3 wallParallelVelocity = Vector3.ProjectOnPlane(playerController.playerVelocity, wallNormal);

        //Cancel out previous velocity if it's in the opposite direction of the jump
        CancelOpposingVelocity(ref playerController.playerVelocity, jumpAccel);

        playerController.playerVelocity = wallParallelVelocity + jumpAccel;
        canWallBounce = false;
        wallRunning = false;
    }

    void CancelOpposingVelocity(ref Vector3 velocity, Vector3 jumpAcceleration)
    {
        if (Vector3.Dot(velocity, jumpAcceleration) < 0) //If velocity is in the opposite direction of jumpAcceleration
        {
            velocity = Vector3.ProjectOnPlane(velocity, jumpAcceleration.normalized); //Cancel perpendicular velocity
        }
    }
}
