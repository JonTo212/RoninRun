using UnityEngine;

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
        if (!hit)
            RotateToFlightDirection();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (hit) return;
        AttachToWall(collision);
    }

    protected void RotateToFlightDirection()
    {
        Vector3 fallDirection = rb.velocity.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(fallDirection, Vector3.up) * Quaternion.Euler(90, 0, 45);
        transform.rotation = targetRotation * Quaternion.Euler(0, 0, additionalSpin);
        additionalSpin += spinRate;
    }

    protected void AttachToWall(Collision collision)
    {
        hit = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        boxCol.enabled = false;

        if (finalPos != Vector3.zero)
            transform.position = finalPos;

        Quaternion contactRotation = Quaternion.LookRotation(-collision.contacts[0].normal);
        transform.rotation = contactRotation * Quaternion.Euler(90, 0, 45);
    }
}

