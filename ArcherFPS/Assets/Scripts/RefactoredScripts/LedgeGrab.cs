using System.Collections;
using UnityEngine;

public class LedgeGrab : MonoBehaviour
{
    [Header("Player Components")]
    private PlayerControllerV2 playerController;
    [SerializeField] private Transform leftHandCheck;
    [SerializeField] private Transform rightHandCheck;
    private float originalGrav;
    public float jumpBufferTime = 0.2f;
    public LayerMask wallMask;

    [Header("Ledge Grab Components")]
    [SerializeField] private float ledgeHangTime = 1f;
    [SerializeField] private float ledgeJumpHeight = 15f;
    [SerializeField] private float grabRange = 0.1f;
    [SerializeField] private float handCheckRadius = 0.3f;
    private float ledgeHangTimer;
    private float grabCooldown = 0.1f;
    private bool canGrab;
    private bool grabInput;
    private bool isGrabbingLedge;
    private bool ledgeJustGrabbed;

    [Header("Vault Components")]
    [SerializeField] private float vaultForwardBoost = 10f;
    [SerializeField] private float vaultDetectRange = 0.6f;
    private bool vaultBuffered = false;
    private Coroutine vaultBufferCoroutine;

    [Header("Swing Settings")]
    [SerializeField] private float swingSpeed = 30f;
    private bool isSwinging = false;
    private Vector3 swingPoint;

    void Awake()
    {
        playerController = GetComponent<PlayerControllerV2>();
        originalGrav = playerController.gravity;
    }

    void Update()
    {
        print(CheckForVault());
        GetInput();

        if (CheckForVault() && vaultBuffered)
        {
            LedgeJump();
        }
        else if(CheckForSwingPole())
        {
            if(!isSwinging)
            {
                StartSwing();
            }
            else if(grabInput)
            {
                HandleSwing();
            }    

        }
        else if (!ledgeJustGrabbed && CheckForLedge(grabRange, Vector3.down, Vector3.up, false))
        {
            if (!isGrabbingLedge)
            {
                StartLedgeGrab();
            }
            else if (grabInput)
            {
                HandleLedgeHang();
            } 
        }
        else
        {
            Detach();
        }
    }

    Vector3 ledgePoint;
    RaycastHit leftHit, rightHit;

    bool CheckForLedge(float range, Vector3 down, Vector3 up, bool isVaultCheck)
    {
        RaycastHit leftTopHit, rightTopHit;

        //ignore wall mask if vaulting
        int layerMask = isVaultCheck ? ~wallMask : Physics.DefaultRaycastLayers;

        bool leftValid = Physics.Raycast(leftHandCheck.position, down, out leftHit, range, layerMask) && !Physics.SphereCast(leftHandCheck.position, handCheckRadius, up, out leftTopHit, range);
        bool rightValid = Physics.Raycast(rightHandCheck.position, down, out rightHit, range, layerMask) && !Physics.SphereCast(rightHandCheck.position, handCheckRadius, up, out rightTopHit, range);

        if (!leftValid || !rightValid) return false;

        ledgePoint = ((leftHit.point - leftHandCheck.position + rightHit.point - rightHandCheck.position) / 2) + new Vector3(0, 0.2f, 0);

        return true;
    }

    bool CheckForVault()
    {
        float forwardSpeed = Vector3.Dot(playerController.playerVelocity, transform.forward);
        float blendAmount = Mathf.Clamp01(forwardSpeed / playerController.playerTopVelocity);
        Vector3 angledDown = Vector3.Slerp(Vector3.down, (Vector3.down + transform.forward).normalized, blendAmount);
        Vector3 angledUp = Vector3.Slerp(Vector3.up, (Vector3.up + transform.forward).normalized, blendAmount);

        return CheckForLedge(vaultDetectRange, angledDown, angledUp, true);
    }

    bool CheckForSwingPole()
    {
        return Physics.SphereCast(leftHandCheck.position, handCheckRadius, transform.forward, out leftHit, handCheckRadius)
            || Physics.SphereCast(rightHandCheck.position, handCheckRadius, transform.forward, out rightHit, handCheckRadius);
    }

    void GetInput()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            grabInput = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            grabInput = false;
            vaultBuffered = true;
            if (vaultBufferCoroutine != null)
            {
                StopCoroutine(vaultBufferCoroutine);
            }

            vaultBufferCoroutine = StartCoroutine(VaultBuffer());
        }

        if (playerController.isGrounded)
        {
            ledgeJustGrabbed = false;
        }
    }

    private IEnumerator VaultBuffer()
    {
        yield return new WaitForSeconds(jumpBufferTime);
        vaultBuffered = false;
        vaultBufferCoroutine = null;
    }

    private IEnumerator LedgeGrabCooldown()
    {
        yield return new WaitForSeconds(grabCooldown);
        ledgeJustGrabbed = false;
    }

    void StartLedgeGrab()
    {
        ledgeHangTimer = 0f;
        canGrab = true;
        isGrabbingLedge = true;

        playerController.enabled = false;
        Vector3 ledgeSnapPoint = (leftHandCheck.position + rightHandCheck.position) / 2f;
        transform.position = new Vector3(transform.position.x, ledgeSnapPoint.y - 0.8f - grabRange, ledgeSnapPoint.z - transform.forward.z * 0.8f) + ledgePoint;
        playerController.enabled = true;
    }

    void StartSwing()
    {
        isSwinging = true;
        swingPoint = (leftHandCheck.position + rightHandCheck.position) / 2f;
        playerController.gravity = 0f;
    }
    void HandleLedgeHang()
    {
        playerController.gravity = 0;
        playerController.playerVelocity = Vector3.zero;

        ledgeHangTimer += Time.deltaTime;

        if (ledgeHangTimer > ledgeHangTime || playerController.wishdir.z < 0)
        {
            Detach();
        }
    }

    void HandleSwing()
    {
        if (swingPoint == Vector3.zero)
        {
            Detach();
            return;
        }

        Vector3 dirToGrapple = swingPoint - transform.position;
        float distance = dirToGrapple.magnitude;

        float period = Mathf.PI * 2 * Mathf.Sqrt(distance / originalGrav);
        float angularVelocity = 2 * Mathf.PI / period;

        Vector3 tangential = Vector3.Cross(dirToGrapple, -transform.right).normalized;
        float tangentialVelocity = angularVelocity * distance;

        playerController.playerVelocity += tangential * tangentialVelocity * Time.deltaTime;
        playerController.gravity = originalGrav / 2f;
        playerController.playerVelocity += dirToGrapple.normalized * swingSpeed * Time.deltaTime;
    }

    void LedgeJump()
    {
        Vector3 jumpDir = Vector3.up * ledgeJumpHeight;

        if (playerController.wishdir.z > 0.1f)
        {
            jumpDir += transform.forward * vaultForwardBoost;
        }

        playerController.playerVelocity = jumpDir;
        vaultBuffered = false;
        Detach();
    }

    void Detach()
    {
        isGrabbingLedge = false;
        isSwinging = false;
        swingPoint = Vector3.zero;
        playerController.gravity = originalGrav;
        ledgeJustGrabbed = true;

        StartCoroutine(LedgeGrabCooldown());
    }


    void OnDrawGizmosSelected()
    {
        if (!leftHandCheck || !rightHandCheck) return;

        float blendAmount = Application.isPlaying && playerController
            ? Mathf.Clamp01(Vector3.Dot(playerController.playerVelocity, transform.forward) / playerController.playerTopVelocity)
            : 0f;

        Vector3 angledDown = Vector3.Slerp(Vector3.down, (Vector3.down + transform.forward).normalized, blendAmount);
        Vector3 angledUp = Vector3.Slerp(Vector3.up, (Vector3.up + transform.forward).normalized, blendAmount);

        // Ledge check (down)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(leftHandCheck.position, Vector3.down * grabRange);
        Gizmos.DrawRay(rightHandCheck.position, Vector3.down * grabRange);

        // Vault ray (angled down-forward)
        Gizmos.color = Color.green;
        Gizmos.DrawRay(leftHandCheck.position, angledDown * vaultDetectRange);
        Gizmos.DrawRay(rightHandCheck.position, angledDown * vaultDetectRange);

        // Vault block check (angled up-forward)
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(leftHandCheck.position, angledUp * vaultDetectRange);
        Gizmos.DrawRay(rightHandCheck.position, angledUp * vaultDetectRange);
        Gizmos.DrawWireSphere(leftHandCheck.position + angledUp * (vaultDetectRange - handCheckRadius / 2), handCheckRadius);
        Gizmos.DrawWireSphere(rightHandCheck.position + angledUp * (vaultDetectRange - handCheckRadius / 2), handCheckRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(leftHandCheck.position * handCheckRadius, handCheckRadius);
        Gizmos.DrawWireSphere(rightHandCheck.position * handCheckRadius, handCheckRadius);
    }
}
