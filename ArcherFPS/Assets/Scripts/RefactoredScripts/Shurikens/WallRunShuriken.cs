using UnityEngine;

public class WallRunShuriken : ShurikenBaseClass
{
    [SerializeField] private Material wallRunMaterial;

    private int originalLayer;
    private Material originalMaterial;
    private GameObject collidedObject;

    [SerializeField] private bool oneWallAtATime;

    private static WallRunShuriken currentShuriken;

    protected override void AttachToSurface(Collision collision)
    {
        base.AttachToSurface(collision);

        //this is only if you want the mark to reset upon collision
        if (currentShuriken != null && oneWallAtATime)
        {
            currentShuriken.ResetWallRunObject();
        }

        //get references
        collidedObject = collision.gameObject;
        originalLayer = collidedObject.layer;

        //set collided object to wallrunnable layer
        collidedObject.layer = LayerMask.NameToLayer("WallRun");

        //set material to wallrun material (brown) for visual indication
        var renderer = collidedObject.GetComponent<Renderer>();
        originalMaterial = collidedObject.GetComponent<Renderer>().material;
        renderer.material = wallRunMaterial;

        //also for the mark disable
        if (oneWallAtATime)
        {
            currentShuriken = this;
            gameObject.SetActive(false);
        }
    }

    public void ResetWallRunObject()
    {
        //for mark disable
        if (collidedObject == null) return;

        //to be called when picking up shuriken
        collidedObject.layer = originalLayer;
        var renderer = collidedObject.GetComponent<Renderer>();
        renderer.material = originalMaterial;

        //also for mark disable lol
        if (currentShuriken == this && oneWallAtATime)
        {
            currentShuriken = null;
        }
    }
}
