using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingSystem : MonoBehaviour
{
    public float grapplingRange = 50f;
    public float swingHookSpeed = 30f;
    public float slingshotHookSpeed = 30f;
    public float releaseJumpForce;
    public float aimAssistRadius = 5f; //buffer area for aiming forgiveness
    public Camera playerCamera;

    private PlayerControllerV2 playerController;
    private bool isGrappling;
    private Vector3 grapplePoint;
    private LineRenderer lineRenderer;
    private float originalGravity;
    [SerializeField] Transform lineRendererStartPos;

    private enum GrappleType { None, GrapplingHook, Slingshot }
    private GrappleType currentGrappleType = GrappleType.None;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        playerController = GetComponent<PlayerControllerV2>();
        originalGravity = playerController.gravity;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isGrappling && !playerController.isGrounded)
        {
            CheckForGrapple();
        }

        if ((Input.GetMouseButtonUp(1) || playerController.isGrounded) && isGrappling)
        {
            StopGrapple();
            if (playerController.playerVelocity.y > 0)
            {
                playerController.playerVelocity.y += releaseJumpForce;
            }
        }

        if (isGrappling)
        {
            if (Vector3.Distance(transform.position, grapplePoint) > grapplingRange)
            {
                StopGrapple();
            }
            else
            {
                if (currentGrappleType == GrappleType.GrapplingHook)
                {
                    Swing();
                }
                else if (currentGrappleType == GrappleType.Slingshot)
                {
                    PullToGrapple();
                }
            }
        }
    }

    void CheckForGrapple()
    {
        RaycastHit[] hits;
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        //use spherecast to 
        hits = Physics.SphereCastAll(rayOrigin, aimAssistRadius, rayDirection, grapplingRange);

        Shuriken aimedAt = null;
        float closestAngle = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            Shuriken shuriken = hit.collider.GetComponent<Shuriken>();
            if (shuriken != null && (shuriken.canGrapple || shuriken.canSlingshot))
            {
                //calculate angle between the player's view direction and the shuriken
                Vector3 toShuriken = (shuriken.transform.position - rayOrigin).normalized;
                float angle = Vector3.Angle(rayDirection, toShuriken);

                if (angle < closestAngle) //choose the closest one to the center of view
                {
                    closestAngle = angle;
                    aimedAt = shuriken;
                }
            }
        }

        if (aimedAt != null)
        {
            grapplePoint = aimedAt.transform.position;
            playerController.playerVelocity.y = 0;
            isGrappling = true;

            // Assign the grapple type based on the shuriken's properties
            if (aimedAt.canGrapple)
            {
                currentGrappleType = GrappleType.GrapplingHook;
            }
            else if (aimedAt.canSlingshot)
            {
                currentGrappleType = GrappleType.Slingshot;
            }
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        lineRenderer.positionCount = 0;
        playerController.gravity = originalGravity;
        currentGrappleType = GrappleType.None;
    }

    void Swing()
    {
        Vector3 directionToGrapple = grapplePoint - transform.position;
        float distanceFromGrapplePoint = directionToGrapple.magnitude;

        float T = Mathf.PI * 2 * Mathf.Sqrt(distanceFromGrapplePoint / playerController.gravity);
        float angularVelocity = 2 * Mathf.PI / T;

        Vector3 tangentialDirection = Vector3.Cross(directionToGrapple, -transform.right).normalized;

        float tangentialVelocityMagnitude = angularVelocity * distanceFromGrapplePoint;
        playerController.playerVelocity += tangentialDirection * tangentialVelocityMagnitude * Time.deltaTime;

        playerController.gravity = originalGravity / 2;
        playerController.playerVelocity += directionToGrapple.normalized * swingHookSpeed * Time.deltaTime;

        UpdateLineRenderer();
    }

    void PullToGrapple()
    {
        Vector3 directionToGrapple = (grapplePoint - transform.position).normalized;
        float distanceToGrapple = Vector3.Distance(transform.position, grapplePoint);
        float clampedDistance = Mathf.Max(2f, distanceToGrapple);

        playerController.gravity = originalGravity / 2;
        playerController.playerVelocity += directionToGrapple * slingshotHookSpeed * clampedDistance * Time.deltaTime;

        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, lineRendererStartPos.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }
}
