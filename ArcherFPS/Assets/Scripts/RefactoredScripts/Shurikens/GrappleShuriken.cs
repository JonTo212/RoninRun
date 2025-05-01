using UnityEngine;

public class GrappleShuriken : ShurikenBaseClass
{
    public Transform grapplePoint;
    public void ApplySwing(PlayerControllerV2 player, float swingSpeed, float gravity)
    {
        Vector3 dirToGrapple = transform.position - player.transform.position;
        float distance = dirToGrapple.magnitude;

        float period = Mathf.PI * 2 * Mathf.Sqrt(distance / gravity);
        float angularVelocity = 2 * Mathf.PI / period;

        Vector3 tangentialDirection = Vector3.Cross(dirToGrapple, -player.transform.right).normalized;
        float tangentialVelocityMagnitude = angularVelocity * distance;

        player.playerVelocity += tangentialDirection * tangentialVelocityMagnitude * Time.deltaTime;
        player.gravity = gravity / 2f;
        player.playerVelocity += dirToGrapple.normalized * swingSpeed * Time.deltaTime;
    }
}
