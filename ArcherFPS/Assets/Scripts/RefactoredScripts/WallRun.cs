using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    private PlayerControllerV2 playerController;

    private float wallDistance = 1f;
    public float minimumJumpHeight = 2f;
    public Vector2 wallJumpForce;
    [HideInInspector] public bool canWallBounce;
    private bool hasWallBounced;

    [HideInInspector] public bool wallLeft;
    [HideInInspector] public bool wallRight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    [HideInInspector] public bool wallHit;
    [HideInInspector] public bool wallRunning;
    private Vector3 wallNormal;
    private Vector3 wallForward;

    public float camTilt = 15f;
    private float desiredTilt;
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
        CameraTilt();
    }

    private void WallBounceReset()
    {
        if (playerController.isGrounded || !wallHit)
        {
            canWallBounce = false;
            hasWallBounced = false;
        }
    }

    private void WallCheck()
    {
        if (CanWallRun() && CheckWall())
        {
            HandleWallRun();
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

    private void CameraTilt()
    {
        currentTilt = Mathf.MoveTowards(currentTilt, desiredTilt, tiltSpeed * Time.deltaTime);
    }

    bool CanWallRun() //Can wall run if you're above the minimum jump height
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight); //&& playerController.canWallRun; //for spacebar hold wallrunning
    }

    bool CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallDistance, wallMask);
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallDistance, wallMask);

        return wallHit = wallLeft || wallRight;
    }

    void HandleWallRun()
    {
        wallRunning = true;

        playerController.inputVector.x = 0; //cancel sideways input so you don't unstick randomly

        //cut effects of gravity in half
        if (playerController.playerVelocity.y < 0)
        {
            float newGrav = playerController.playerVelocity.y / 2;
            playerController.playerVelocity = new Vector3(playerController.playerVelocity.x, newGrav, playerController.playerVelocity.z);
        }

        //apply friction if there's no movement input
        if (playerController.wishdir == Vector3.zero)
        {
            playerController.ApplyFriction(0.1f);
        }

        CalculateWallValues();
        HandleWallAutoMovement();

        if (!hasWallBounced && Input.GetKeyDown(KeyCode.Space)) // change to GetKeyUp for space hold
        {
            WallJump();
        }
    }
    void CalculateWallValues()
    {
        wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        wallForward = Vector3.Cross(wallNormal, transform.up);
    }

    void HandleWallAutoMovement()
    {
        //Tilt + auto-run
        desiredTilt = wallLeft ? -camTilt : camTilt;
        float forwardDir = wallLeft ? 1 : -1; //if there's a wall on the left, apply force in +Z, if right, -Z
        playerController.playerVelocity += forwardDir * wallForward * autoRunForce * Time.deltaTime;


        //Apply small force so you don't detach from wall
        Vector3 wallStick = wallNormal * 5f * Time.deltaTime;
        playerController.playerVelocity -= wallStick;
    }


    void WallJump()
    {
        Vector3 wallJumpHorizontal = wallNormal * wallJumpForce.x; // Horizontal component based on wall normal
        Vector3 wallJumpVertical = transform.up * wallJumpForce.y;

        Vector3 jumpAccel = wallJumpHorizontal + wallJumpVertical;
        Vector3 wallParallelVelocity = Vector3.ProjectOnPlane(playerController.playerVelocity, wallNormal);

        // Cancel out previous velocity if it's in the opposite direction of the jump
        CancelOpposingVelocity(ref playerController.playerVelocity, jumpAccel);

        playerController.playerVelocity = wallParallelVelocity + jumpAccel;
        canWallBounce = false;
        hasWallBounced = true;
        wallRunning = false;
    }

    void CancelOpposingVelocity(ref Vector3 velocity, Vector3 jumpAcceleration)
    {
        if (Vector3.Dot(velocity, jumpAcceleration) < 0) // If velocity is in the opposite direction of jumpAcceleration
        {
            velocity = Vector3.ProjectOnPlane(velocity, jumpAcceleration.normalized); // Cancel perpendicular velocity
        }
    }
}
