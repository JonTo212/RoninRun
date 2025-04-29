using UnityEngine;

//Class is abstract so you can't do "new ShurikenBaseClass()", this acts as a template for all shurikens
public abstract class ShurikenBaseClass : MonoBehaviour
{
    protected Rigidbody rb;
    protected BoxCollider boxCol;
    protected bool hit;
    [SerializeField] protected float spinRate;
    protected float additionalSpin;
    public Vector3 finalPos;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>();
        rb.maxAngularVelocity = Mathf.Infinity;
    }

    protected virtual void Update()
    {
        //follow arc if it's not attached to a wall
        if (!hit)
            RotateToFlightDirection();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (hit) return;
        AttachToSurface(collision);
    }

    protected void RotateToFlightDirection()
    {
        //rotates object to face the falling arc
        Vector3 fallDirection = rb.velocity.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(fallDirection, Vector3.up) * Quaternion.Euler(90, 0, 45);

        //apply spin to make it look natural
        transform.rotation = targetRotation * Quaternion.Euler(0, 0, additionalSpin);
        additionalSpin += spinRate;
    }

    protected virtual void AttachToSurface(Collision collision)
    {
        //disable physics components
        hit = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        boxCol.enabled = false;

        //finalPos is passed through from the aim indicator so the landing position lines up
        if (finalPos != Vector3.zero)
            transform.position = finalPos; 

        //attach perpendicular to collision point, the 45 is because of the model
        Quaternion contactRotation = Quaternion.LookRotation(-collision.contacts[0].normal);
        transform.rotation = contactRotation * Quaternion.Euler(90, 0, 45);
    }
}

