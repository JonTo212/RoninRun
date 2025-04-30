using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Slingshot : MonoBehaviour
{
    public float grapplingRange = 50f;
    public float hookSpeed = 30f;
    PlayerControllerV2 playerController;
    public bool isGrappling;
    Vector3 grapplePoint;
    LineRenderer lineRenderer;
    float originalGrav;
    [SerializeField] float releaseJumpForce;
    [SerializeField] Transform lineRendererStartPos;
    GrapplingHook grapple;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        playerController = GetComponent<PlayerControllerV2>();
        grapple= GetComponent<GrapplingHook>();
        originalGrav = playerController.gravity;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isGrappling && !playerController.isGrounded && !grapple.isGrappling)
        {
            CheckForGrapple();
        }

        if ((Input.GetMouseButtonUp(1) || playerController.isGrounded) && isGrappling)
        {
            StopGrapple();
        }

        if (isGrappling && Vector3.Distance(transform.position, grapplePoint) <= grapplingRange)
        {
            Swing();
        }
        else if (isGrappling && Vector3.Distance(transform.position, grapplePoint) > grapplingRange)
        {
            StopGrapple();
        }
    }

    void CheckForGrapple()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, grapplingRange);
        Shuriken closestShuriken = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hitCollider in hitColliders)
        {
            Shuriken shuriken = hitCollider.GetComponent<Shuriken>();

            if (shuriken != null && shuriken.canSlingshot)
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestShuriken = shuriken;
                }
            }
        }

        if (closestShuriken != null)
        {
            grapplePoint = closestShuriken.transform.position;
            playerController.playerVelocity.y = 0;
            isGrappling = true;
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        lineRenderer.positionCount = 0;
        playerController.gravity = originalGrav;
    }

    void Swing() //this version pulls you towards the grapple point
    {
        // Calculate direction from player to grapple point
        Vector3 directionToGrapple = (grapplePoint - transform.position).normalized;
        float distanceToGrapple = Vector3.Distance(transform.position, grapplePoint);

        // Move player towards the grapple point
        playerController.gravity = originalGrav / 2;
        playerController.playerVelocity += directionToGrapple * hookSpeed * distanceToGrapple * Time.deltaTime;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }
}
