using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public float grapplingRange = 50f;
    public float hookSpeed = 30f;
    PlayerControllerV2 playerController;
    bool isGrappling;
    Vector3 grapplePoint;
    LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        playerController = GetComponent<PlayerControllerV2>();
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
        }

        if (isGrappling)
        {
            Swing();
        }
    }

    void CheckForGrapple()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, grapplingRange))
        {
            Shuriken shuriken = hit.collider.GetComponent<Shuriken>();

            if (shuriken != null && shuriken.canGrapple)
            {
                // Use the trigger collider on the shuriken for proximity detection
                CapsuleCollider collider = shuriken.GetComponent<CapsuleCollider>();
                if (collider != null)
                {
                    Vector3 closestPoint = collider.ClosestPoint(hit.point);
                    float distance = Vector3.Distance(hit.point, closestPoint);

                    // Check if the hit point is within the trigger collider
                    if (distance < 0.1f) // Adjust this threshold based on your needs
                    {
                        grapplePoint = hit.point;
                        isGrappling = true;
                    }
                }
            }
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        lineRenderer.positionCount = 0;
    }

    void Swing()
    {
        // Calculate direction from player to grapple point
        Vector3 directionToGrapple = grapplePoint - transform.position;
        directionToGrapple.Normalize();

        // Move player towards the grapple point
        playerController.playerVelocity += directionToGrapple * hookSpeed * Time.deltaTime;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }
}
