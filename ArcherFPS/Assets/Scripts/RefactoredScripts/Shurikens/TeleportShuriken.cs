using UnityEngine;

public class TeleportShuriken : ShurikenBaseClass
{
    private GameObject player;
    private ParticleSystem smokePuff;

    public void Initialize(GameObject playerRef, ParticleSystem puff)
    {
        player = playerRef;
        smokePuff = puff;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(1))
        {
            TeleportPlayer();
        }
    }

    protected override void AttachToSurface(Collision collision)
    {
        base.AttachToSurface(collision);
        TeleportPlayer();
    }

    private void TeleportPlayer()
    {
        if (smokePuff != null)
            smokePuff.Play();

        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = transform.position;
            controller.enabled = true;
        }

        Destroy(gameObject);
    }
}
