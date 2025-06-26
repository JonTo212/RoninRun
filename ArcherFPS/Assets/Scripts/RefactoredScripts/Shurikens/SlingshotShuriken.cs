using UnityEngine;

public class SlingshotShuriken : ShurikenBaseClass
{
    public Transform grapplePoint;
    float minPullSpeed = 450f;
    float maxPullSpeed = 600f;

    public void ApplyPull(PlayerControllerV2 player, float pullSpeed, float gravity)
    {
        Vector3 direction = (grapplePoint.position - player.transform.position).normalized;
        float distance = Vector3.Distance(grapplePoint.position, player.transform.position);
        float effectiveDistance = Mathf.Max(distance, 3f);

        //hooke's law - linear spring force (F = -k * x)
        //k = spring stiffness, x = distance from grapple point
        Vector3 rawForce = direction * pullSpeed * distance;
        float clampedMagnitude = Mathf.Clamp(rawForce.magnitude, minPullSpeed, maxPullSpeed);
        Vector3 springForce = direction * clampedMagnitude;

        //damping to reduce overshoot
        //critical damping = 2 * sqrt(stiffness) -> the damping force to return the system to equilibrium as fast as possible without oscillation
        Vector3 dampingForce = player.playerVelocity * (2 * Mathf.Sqrt(pullSpeed));

        Vector3 acceleration = springForce - dampingForce;
        player.playerVelocity += acceleration * Time.deltaTime;
    }
}
