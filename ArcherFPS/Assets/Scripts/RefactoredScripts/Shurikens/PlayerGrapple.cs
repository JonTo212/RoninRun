using UnityEngine;

public class PlayerGrapple : MonoBehaviour
{
    public float grapplingRange = 50f;
    public float grapplingDetachRange = 5f;
    public float swingHookSpeed = 30f;
    public float slingshotHookSpeed = 30f;
    public float releaseJumpForce;
    public float aimAssistRadius = 5f;
    public Camera playerCamera;
    [SerializeField] private Transform lineRendererStartPos;
    [SerializeField] float grappleFOVAngle = 30f;

    private PlayerControllerV2 playerController;
    private bool isGrappling;
    private Vector3 grapplePoint;
    private LineRenderer lineRenderer;
    private float originalGravity;
    private ShurikenBaseClass activeShuriken;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        playerController = GetComponent<PlayerControllerV2>();
        originalGravity = playerController.gravity;
    }

    private void Update()
    {
        print(isGrappling);
    }

    /*private void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isGrappling && !playerController.isGrounded)
        {
            TryStartGrapple();
        }

        if ((Input.GetMouseButtonUp(1) || playerController.isGrounded) && isGrappling)
        {
            StopGrapple();
            if (playerController.playerVelocity.y > 0)
            {
                playerController.playerVelocity.y += releaseJumpForce;
            }
        }

        if (isGrappling && activeShuriken != null)
        {
            float dist = Vector3.Distance(transform.position, grapplePoint);

            if (dist > grapplingRange || dist < grapplingDetachRange)
            {
                StopGrapple();
            }
            else
            {
                if (playerController.useGrav)
                {
                    playerController.playerVelocity = new Vector3(playerController.playerVelocity.x, 0, playerController.playerVelocity.z);
                    playerController.useGrav = false;
                }

                if (activeShuriken is GrappleShuriken grapple)
                {
                    grapple.ApplySwing(playerController, swingHookSpeed, originalGravity);
                }
                else if (activeShuriken is SlingshotShuriken slingshot)
                {
                    slingshot.ApplyPull(playerController, slingshotHookSpeed, originalGravity);
                }

                UpdateLine();
            }
        } 
    }*/

    public bool CheckGrapple()
    {
        return Input.GetMouseButtonDown(1) && TryStartGrapple() && !playerController.isGrounded;
    }

    public bool IsInValidGrappleRange()
    {
        return IsWithinGrappleRange(transform.position, grapplePoint, grapplingDetachRange, grapplingRange);
    }

    bool IsWithinGrappleRange(Vector3 from, Vector3 to, float minRange, float maxRange)
    {
        float dist = Vector3.Distance(from, to);
        return dist < maxRange && dist > minRange;
    }

    public void HandleGrapplePull()
    {
        float dist = Vector3.Distance(transform.position, grapplePoint);

        if (playerController.useGrav)
        {
            playerController.playerVelocity = new Vector3(playerController.playerVelocity.x, 0, playerController.playerVelocity.z);
            playerController.useGrav = false;
        }
        if (activeShuriken is SlingshotShuriken slingshot)
        {
            slingshot.ApplyPull(playerController, slingshotHookSpeed, originalGravity);
        }

        UpdateLine();
    }

    bool TryStartGrapple()
    {
        isGrappling = false;

        RaycastHit[] hits = Physics.SphereCastAll(playerCamera.transform.position, aimAssistRadius, playerCamera.transform.forward, grapplingRange - aimAssistRadius);

        float closestAngle = Mathf.Infinity;
        ShurikenBaseClass target = null;

        //check if it has the shuriken base class and is the grapple/slingshot shuriken
        //also check if the angle between your look direction and the grapple point is within the 
        foreach (var hit in hits)
        {
            if (!hit.collider.TryGetComponent<ShurikenBaseClass>(out var shuriken)) continue;
            if (!(shuriken is GrappleShuriken) && !(shuriken is SlingshotShuriken)) continue;

            Vector3 toTarget = (shuriken.transform.position - playerCamera.transform.position).normalized;
            float angle = Vector3.Angle(playerCamera.transform.forward, toTarget);

            if (angle > grappleFOVAngle)
                continue;

            else if (angle < closestAngle)
            {
                closestAngle = angle;
                target = shuriken;
            }
        }


        //if there's a valid shuriken, check if it's within range, then save it as the grapple point and start grappling
        if (target != null)
        {
            Vector3 point = (target as GrappleShuriken)?.grapplePoint.position
                     ?? (target as SlingshotShuriken)?.grapplePoint.position
                     ?? target.transform.position;

            if (IsWithinGrappleRange(transform.position, point, grapplingDetachRange, grapplingRange))
            {
                activeShuriken = target;
                grapplePoint = point;

                playerController.playerVelocity.y = 0;
                isGrappling = true;
            }
        }

        return isGrappling;
    }

    public void StopGrapple()
    {
        isGrappling = false;
        lineRenderer.positionCount = 0;
        playerController.gravity = originalGravity;
        activeShuriken = null;
        playerController.useGrav = true;
    }

    public void ReleaseGrapple()
    {
        StopGrapple();
        playerController.playerVelocity.y += releaseJumpForce;
    }

    void UpdateLine()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, lineRendererStartPos.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = playerCamera.transform.position;
        Vector3 forward = playerCamera.transform.forward;

        Quaternion leftRot = Quaternion.AngleAxis(-grappleFOVAngle, playerCamera.transform.up);
        Quaternion rightRot = Quaternion.AngleAxis(grappleFOVAngle, playerCamera.transform.up);

        Gizmos.DrawRay(origin, leftRot * forward * grapplingRange);
        Gizmos.DrawRay(origin, rightRot * forward * grapplingRange);
    }
}
