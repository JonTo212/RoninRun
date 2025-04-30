using UnityEngine;

public class SlingshotShuriken : ShurikenBaseClass
{
    public void ApplyPull(PlayerControllerV2 player, float pullSpeed, float gravity)
    {
        Vector3 direction = (transform.position - player.transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.transform.position);
        float clampedDistance = Mathf.Max(2f, distance);

        player.gravity = gravity / 2f;
        player.playerVelocity += direction * pullSpeed * clampedDistance * Time.deltaTime;
    }
}
