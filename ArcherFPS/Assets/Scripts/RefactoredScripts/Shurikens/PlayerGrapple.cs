using UnityEngine;

public class PlayerGrapple : MonoBehaviour
{
    public float grapplingRange = 50f;
    public float swingHookSpeed = 30f;
    public float slingshotHookSpeed = 30f;
    public float releaseJumpForce;
    public float aimAssistRadius = 5f;
    public Camera playerCamera;
    [SerializeField] private Transform lineRendererStartPos;

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
            if (dist > grapplingRange)
            {
                StopGrapple();
            }
            else
            {
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
    }

    void TryStartGrapple()
    {
        RaycastHit[] hits = Physics.SphereCastAll(playerCamera.transform.position, aimAssistRadius, playerCamera.transform.forward, grapplingRange);

        float closestAngle = Mathf.Infinity;
        ShurikenBaseClass target = null;

        foreach (var hit in hits)
        {
            if (!hit.collider.TryGetComponent<ShurikenBaseClass>(out var shuriken)) continue;
            if (!(shuriken is GrappleShuriken) && !(shuriken is SlingshotShuriken)) continue;

            Vector3 toTarget = (shuriken.transform.position - playerCamera.transform.position).normalized;
            float angle = Vector3.Angle(playerCamera.transform.forward, toTarget);

            if (angle < closestAngle)
            {
                closestAngle = angle;
                target = shuriken;
            }
        }

        if (target != null)
        {
            activeShuriken = target;
            grapplePoint = target.transform.position;
            playerController.playerVelocity.y = 0;
            isGrappling = true;
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        lineRenderer.positionCount = 0;
        playerController.gravity = originalGravity;
        activeShuriken = null;
    }

    void UpdateLine()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, lineRendererStartPos.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }
}
