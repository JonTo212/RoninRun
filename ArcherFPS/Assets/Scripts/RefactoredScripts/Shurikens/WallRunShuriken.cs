using UnityEngine;

public class WallRunShuriken : ShurikenBaseClass
{
    [SerializeField] private Material wallRunMaterial;

    private int originalLayer;
    private Material originalMaterial;
    private GameObject collidedObject;

    protected override void AttachToSurface(Collision collision)
    {
        base.AttachToSurface(collision);
        
        //get references
        collidedObject = collision.gameObject;
        originalLayer = collidedObject.layer;

        //set collided object to wallrunnable layer
        collidedObject.layer = LayerMask.NameToLayer("WallRun");

        //set material to wallrun material (brown) for visual indication
        var renderer = collidedObject.GetComponent<Renderer>();
        originalMaterial = collidedObject.GetComponent<Renderer>().material;
        renderer.material = wallRunMaterial;
    }

    public void ResetWallRunObject()
    {
        //to be called when picking up shuriken
        collidedObject.layer = originalLayer;
        var renderer = collidedObject.GetComponent<Renderer>();
        renderer.material = originalMaterial;
    }
}
