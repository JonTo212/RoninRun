using UnityEngine;

public class SlingshotShuriken : ShurikenBaseClass
{
    public Transform grapplePoint;
    public void ApplyPull(PlayerControllerV2 player, float pullSpeed, float gravity)
    {
        Vector3 direction = (grapplePoint.position - player.transform.position).normalized;
        float distance = Vector3.Distance(grapplePoint.position, player.transform.position);

        player.gravity = 0;
        player.playerVelocity += direction * pullSpeed * distance * Time.deltaTime;
    }
}
