using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField]
    float torque;

    [SerializeField]
    Rigidbody rb;

    bool hit;

    public void Fly(Vector3 force)
    {
        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(transform.right * torque);
        transform.SetParent(null);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (hit)
        {
            return;
        }

        hit = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.SetParent(collider.transform);
    }
}
