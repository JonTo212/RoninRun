using System.Collections;
using UnityEngine;

public class LedgeGrab : MonoBehaviour
{
    private PlayerControllerV2 playerController;

    public float ledgeHangTime = 1.5f;
    public float ledgeJumpMultiplier = 2f;
    public float vaultForwardBoost = 5f;
    private bool canGrab;
    private bool grabInput;
    private bool isGrabbingLedge;
    private bool ledgeJustGrabbed;
    private Coroutine bufferCoroutine;

    private float ledgeHangTimer;
    private float originalGrav;
    public float jumpBufferTime = 0.2f;
    private float jumpBufferTimer = 0f;
    private bool vaultBuffered = false;
    private Vector3 handOffset;

    [SerializeField] private Transform leftHandCheck;
    [SerializeField] private Transform rightHandCheck;
    [SerializeField] private float grabRange = 0.1f;
    [SerializeField] private float vaultDetectRange = 0.6f;
    [SerializeField] private float handCheckRadius = 0.3f;

    void Awake()
    {
        playerController = GetComponent<PlayerControllerV2>();
        originalGrav = playerController.gravity;
        handOffset = (leftHandCheck.localPosition + rightHandCheck.localPosition) / 2f;
    }

    void Update()
    {
        GetInput();

        if(CheckForVault() && vaultBuffered)
        {
            LedgeJump();
        }
        else if (CheckForLedge(grabRange, Vector3.down))
        {
            if (!isGrabbingLedge)
            {
                StartLedgeGrab();
            }
            else if (grabInput)
            {
                HandleLedgeHang();
            }
            else
            {
                Detach();
            }
        }
    }

    Vector3 ledgePoint;
    RaycastHit leftHit, rightHit;

    bool CheckForLedge(float range, Vector3 down)
    {
        RaycastHit leftTopHit, rightTopHit;

        bool leftValid = Physics.Raycast(leftHandCheck.position, down, out leftHit, range) && !Physics.SphereCast(leftHandCheck.position, handCheckRadius, Vector3.up, out leftTopHit);
        bool rightValid = Physics.Raycast(rightHandCheck.position, down, out rightHit, range) && !Physics.SphereCast(rightHandCheck.position, handCheckRadius, Vector3.up, out rightTopHit);

        if (!leftValid || !rightValid) return false;

        ledgePoint = ((leftHit.point - leftHandCheck.position + rightHit.point - rightHandCheck.position) / 2) + new Vector3(0, 0.2f, 0);

        return true;
    }

    bool CheckForVault()
    {
        float forwardSpeed = Vector3.Dot(playerController.playerVelocity, transform.forward);
        float blendAmount = Mathf.Clamp01(forwardSpeed / playerController.playerTopVelocity);
        Vector3 angledDown = Vector3.Slerp(Vector3.down, (Vector3.down + transform.forward).normalized, blendAmount);

        return CheckForLedge(vaultDetectRange, angledDown);
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
            if (bufferCoroutine != null)
            {
                StopCoroutine(bufferCoroutine);
            }

            bufferCoroutine = StartCoroutine(VaultBuffer());
        }
    }

    private IEnumerator VaultBuffer()
    {
        yield return new WaitForSeconds(jumpBufferTime);
        vaultBuffered = false;
        bufferCoroutine = null;
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
    void HandleLedgeHang()
    {
        playerController.gravity = 0;
        playerController.playerVelocity = Vector3.zero;

        ledgeHangTimer += Time.deltaTime;

        if (ledgeHangTimer > ledgeHangTime || playerController.wishdir.z < 0)
        {
            Detach();
            //return;
        }

        if (!canGrab)
        {
            LedgeJump();
        }
    }

    void LedgeJump()
    {
        Vector3 jumpDir = Vector3.up * ledgeJumpMultiplier;

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
        playerController.gravity = originalGrav;
    }

    void OnDrawGizmosSelected()
    {
        if (!leftHandCheck || !rightHandCheck) return;

        float blendAmount = Application.isPlaying && playerController
            ? Mathf.Clamp01(Vector3.Dot(playerController.playerVelocity, transform.forward) / playerController.playerTopVelocity)
            : 0f;

        Vector3 angledDownDebug = Vector3.Slerp(Vector3.down, (Vector3.down + transform.forward).normalized, blendAmount);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(leftHandCheck.position, Vector3.down * grabRange);
        Gizmos.DrawRay(rightHandCheck.position, Vector3.down * grabRange);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(leftHandCheck.position, angledDownDebug * vaultDetectRange);
        Gizmos.DrawRay(rightHandCheck.position, angledDownDebug * vaultDetectRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(leftHandCheck.position + Vector3.up * handCheckRadius, handCheckRadius);
        Gizmos.DrawWireSphere(rightHandCheck.position + Vector3.up * handCheckRadius, handCheckRadius);
    }
}
