using TMPro;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
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
    Slingshot slingshot;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        playerController = GetComponent<PlayerControllerV2>();
        slingshot = GetComponent<Slingshot>();
        originalGrav = playerController.gravity;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isGrappling && !playerController.isGrounded && !slingshot.isGrappling)
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

        if (isGrappling && Vector3.Distance(transform.position, grapplePoint) <= grapplingRange)
        {
            Swing();
        }
        else if(isGrappling && Vector3.Distance(transform.position, grapplePoint) > grapplingRange)
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

            if (shuriken != null && shuriken.canGrapple)
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

    void Swing()
    {
        //Direction + distance from grapple point (for swing velocity)
        Vector3 directionToGrapple = grapplePoint - transform.position;
        float distanceFromGrapplePoint = directionToGrapple.magnitude;

        //Pendulum calculations
        float T = Mathf.PI * 2 * Mathf.Sqrt(distanceFromGrapplePoint / playerController.gravity); //Time it takes for one full pendulum cycle
        float angularVelocity = 2 * Mathf.PI / T; //Velocity it takes to create pendulum cycle in T seconds

        //Player swings around X axis (-transform.right), this calculates the swing force direction
        Vector3 tangentialDirection = Vector3.Cross(directionToGrapple, -transform.right).normalized;

        //Calculate velocity it takes to create pendulum cycle multiplied by distance from grapple creates the velocity at each point of the pendulum and apply to player
        float tangentialVelocityMagnitude = angularVelocity * distanceFromGrapplePoint;
        playerController.playerVelocity += tangentialDirection * tangentialVelocityMagnitude * Time.deltaTime;

        //Reduce gravity, pull player towards grapple point
        playerController.gravity = originalGrav / 2;
        playerController.playerVelocity += directionToGrapple.normalized * hookSpeed * Time.deltaTime;

        //Update line renderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, lineRendererStartPos.position);    
        lineRenderer.SetPosition(1, grapplePoint);
    }


    /*void Swing() //this version pulls you towards the grapple point
    {
        // Calculate direction from player to grapple point
        Vector3 directionToGrapple = (grapplePoint - transform.position).normalized;

        // Move player towards the grapple point
        playerController.gravity = originalGrav / 2;
        playerController.playerVelocity += directionToGrapple * hookSpeed * Time.deltaTime;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }


    void Swing() //this version is a static pendulum/spin around grapple point
    {
        //Direction + distance of player to grapple point
        Vector3 directionToGrapple = grapplePoint - transform.position;
        float distanceFromGrapplePoint = directionToGrapple.magnitude;

        //
        float T = Mathf.PI * 2 * Mathf.Sqrt(distanceFromGrapplePoint / playerController.gravity);
        float angularVelocity = 2 * Mathf.PI / T;

        //Linear speed of the player at any point in the circular path
        float tangentialVelocityMagnitude = angularVelocity * distanceFromGrapplePoint;

        // Calculate the tangential direction (perpendicular to the direction to grapple)
        Vector3 tangentialDirection = Vector3.Cross(directionToGrapple, transform.right).normalized;

        // Apply the tangential velocity to the player's velocity
        playerController.playerVelocity = tangentialDirection * -tangentialVelocityMagnitude;

        // Ensure the player remains at a fixed distance from the grapple point
        Vector3 lockedPosition = grapplePoint - directionToGrapple.normalized * distanceFromGrapplePoint;
        Vector3 correctionVector = lockedPosition - transform.position;

        // Apply the correction vector to maintain the fixed distance
        playerController.playerVelocity += correctionVector * Time.deltaTime;

        // Update the line renderer for visual feedback (optional)
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }*/
}
