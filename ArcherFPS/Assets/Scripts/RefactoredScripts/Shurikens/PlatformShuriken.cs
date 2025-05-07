using UnityEngine;

public class PlatformShuriken : ShurikenBaseClass
{
    protected override void AttachToSurface(Collision collision)
    {
        //double the size of the shuriken
        base.AttachToSurface(collision);
        transform.localScale *= 2f;
        
        //offset it a bit because of the size change
        float offsetAmount = GetComponentInChildren<Renderer>().bounds.extents.magnitude * 0.175f;
        transform.position += offsetAmount * collision.contacts[0].normal;

        //additional rotation so it is a more effective platform
        Quaternion contactRotation = Quaternion.LookRotation(-collision.contacts[0].normal);
        transform.rotation = contactRotation * Quaternion.Euler(90, 45, 45);
        boxCol.enabled = true;
    }
}