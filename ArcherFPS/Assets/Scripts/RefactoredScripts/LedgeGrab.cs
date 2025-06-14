using System.Collections;
using UnityEngine;

public class LedgeGrab : MonoBehaviour
{
    private PlayerControllerV2 playerController;

    public float ledgeHangTime = 1.5f;
    public float ledgeJumpMultiplier = 2f;
    public float vaultForwardBoost = 5f;
    public float climbSpeed = 2f;
    public float maxClimbTime = 1.2f;
    public float wallClimbCheckRadius = 0.5f;
    public float wallClimbCheckDistance = 0.6f;
    public LayerMask wallMask;

    private bool isGrabbingLedge;
    private bool ledgeJustGrabbed;
    private bool isClimbingWall;
    private bool wallClimbed;
    private bool canGrab;

    private float ledgeGrabTimer;
    private float grabCooldownTimer = 0f;
    private float jumpBufferedTime = -1f;
    private float climbTimer = 0f;
    private float originalGrav;

    public float grabCooldown = 0.2f;

    [SerializeField] private Transform leftHandCheck;
    [SerializeField] private Transform rightHandCheck;
    [SerializeField] private float handRayLength = 1.0f;
    [SerializeField] private float jumpBufferWindow = 0.2f;
    [SerializeField] private float predictiveRayLength = 1.2f;
    [SerializeField] private float ledgeDetectAngle = 35f;

    void Awake()
    {
        playerController = GetComponent<PlayerControllerV2>();
        originalGrav = playerController.gravity;
    }

    void Update()
    {
        canGrab = Input.GetKey(KeyCode.Space);

        if(playerController.isGrounded)
        {
            wallClimbed = false;
            grabCooldownTimer = 0f;
            ledgeJustGrabbed = false;
            grabCooldownTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferedTime = Time.time;
        }

        if (ledgeJustGrabbed)
        {
            grabCooldownTimer += Time.deltaTime;
            if (grabCooldownTimer >= grabCooldown)
            {
                ledgeJustGrabbed = false;
                grabCooldownTimer = 0f;
            }
            return;
        }

        if (!isGrabbingLedge && !isClimbingWall)
        {
            CheckForWallClimb();
            CheckForLedge();
        }
        else if (isGrabbingLedge)
        {
            HandleLedgeHang();
        }
        else if (isClimbingWall)
        {
            HandleWallClimb();
        }
    }

    void CheckForWallClimb()
    {
        if (!Input.GetKey(KeyCode.Space)) return;
        if (playerController.isGrounded) return;

        Vector3 origin = transform.position + Vector3.up * 1.0f;

        if (Physics.SphereCast(origin, wallClimbCheckRadius, transform.forward, out RaycastHit hit, wallClimbCheckDistance) && !wallClimbed)
        {
            StartWallClimb();
        }
    }

    void StartWallClimb()
    {
        isClimbingWall = true;
        climbTimer = 0f;
        playerController.gravity = 0f;
        playerController.playerVelocity = Vector3.zero;
    }

    void HandleWallClimb()
    {
        if (!Input.GetKey(KeyCode.Space))
        {
            DetachFromLedge();
            return;
        }

        climbTimer += Time.deltaTime;
        if (climbTimer >= maxClimbTime)
        {
            LedgeJump();
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 1.0f;
        if (!Physics.SphereCast(origin, wallClimbCheckRadius, transform.forward, out RaycastHit hit, wallClimbCheckDistance))
        {
            LedgeJump();
            return;
        }

        playerController.playerVelocity = new Vector3(playerController.playerVelocity.x, climbSpeed, playerController.playerVelocity.z);
    }

    void CheckForLedge()
    {
        RaycastHit leftHit, rightHit;
        bool isBufferedJump = Time.time - jumpBufferedTime <= jumpBufferWindow;

        Vector3 leftDir, rightDir;
        float rayLength;

        if (isBufferedJump)
        {
            Vector3 angledDir = Quaternion.AngleAxis(ledgeDetectAngle, transform.right) * -transform.up;
            Vector3 predictiveDir = (angledDir + transform.forward * 0.3f).normalized;
            leftDir = rightDir = predictiveDir;
            rayLength = predictiveRayLength;
        }
        else
        {
            leftDir = rightDir = Vector3.down;
            rayLength = handRayLength;
        }

        bool leftDetected = Physics.Raycast(leftHandCheck.position, leftDir, out leftHit, rayLength);
        bool rightDetected = Physics.Raycast(rightHandCheck.position, rightDir, out rightHit, rayLength);

        if (!leftDetected || !rightDetected) return;

        if (isBufferedJump)
        {
            LedgeJump();
            return;
        }

        StartLedgeGrab();
    }

    void StartLedgeGrab()
    {
        isGrabbingLedge = true;
        ledgeJustGrabbed = true;
        ledgeGrabTimer = 0f;
        playerController.gravity = 0;
        playerController.playerVelocity = Vector3.zero;
    }

    void HandleLedgeHang()
    {
        ledgeGrabTimer += Time.deltaTime;

        if (playerController.playerVelocity.y < 0)
        {
            playerController.playerVelocity = new Vector3(playerController.playerVelocity.x, 0, playerController.playerVelocity.z);
        }

        if (ledgeGrabTimer > ledgeHangTime || playerController.wishdir.z < 0)
        {
            DetachFromLedge();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            LedgeJump();
        }
    }

    void LedgeJump()
    {
        Vector3 jumpDir = Vector3.up * ledgeJumpMultiplier;
        bool canVault = playerController.wishdir.z > 0.1f;

        if (canVault)
        {
            jumpDir += transform.forward * vaultForwardBoost;
        }

        playerController.playerVelocity = jumpDir;
        DetachFromLedge();
    }

    void DetachFromLedge()
    {
        isGrabbingLedge = false;
        ledgeJustGrabbed = true;
        isClimbingWall = false;
        wallClimbed = true;
        playerController.gravity = originalGrav;
    }

    void OnDrawGizmosSelected()
    {
        if (!leftHandCheck || !rightHandCheck) return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(leftHandCheck.position, Vector3.down * handRayLength);
        Gizmos.DrawRay(rightHandCheck.position, Vector3.down * handRayLength);

        Vector3 angledDir = Quaternion.AngleAxis(ledgeDetectAngle, transform.right) * -transform.up;
        Vector3 predictiveDir = (angledDir + transform.forward * 0.3f).normalized;

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(leftHandCheck.position, predictiveDir * predictiveRayLength);
        Gizmos.DrawRay(rightHandCheck.position, predictiveDir * predictiveRayLength);

        Gizmos.color = Color.cyan;
        Vector3 sphereOrigin = transform.position + Vector3.up * 1.0f;
        Gizmos.DrawRay(sphereOrigin, transform.forward * wallClimbCheckDistance);
        Gizmos.DrawWireSphere(sphereOrigin + transform.forward * wallClimbCheckDistance, wallClimbCheckRadius);
    }
}
