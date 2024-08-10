using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] float torque;
    [SerializeField] Rigidbody rb;
    [SerializeField] float destroyTimer;
    BoxCollider boxCol;
    MeshCollider meshCol;
    bool hit;
    int originalLayer;
    GameObject collidedObj;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>();
        meshCol = GetComponent<MeshCollider>();
    }

    public void Fly(Vector3 force)
    {
        rb.isKinematic = false;
        boxCol.enabled = false;
        meshCol.enabled = true;
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(transform.right * torque);
        transform.SetParent(null);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hit)
        {
            return;
        }

        hit = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        boxCol.enabled = true;
        meshCol.enabled = false;
        collidedObj = collision.gameObject;
        originalLayer = collision.gameObject.layer;
        collision.gameObject.layer = LayerMask.NameToLayer("WallRun");
        StartCoroutine(DelayedDestroy());
        //transform.SetParent(collider.transform, true);

        //Destroy(this.gameObject);
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(destroyTimer);
        collidedObj.layer = originalLayer;
        Destroy(gameObject);
    }
}
