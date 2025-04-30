using UnityEngine;

public class PlatformShuriken : ShurikenBaseClass
{
    protected override void AttachToSurface(Collision collision)
    {
        //only thing that happens is on collision, double the size of the shuriken
        base.AttachToSurface(collision);
        transform.localScale *= 2f;
        boxCol.enabled = true;
    }
}