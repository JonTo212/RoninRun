using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuriken : MonoBehaviour
{
    [SerializeField] float torque;
    [SerializeField] Rigidbody rb;
    [SerializeField] float destroyTimer;
    [SerializeField] Material wallRunMat;
    [SerializeField] Material wallRunArrowMat;
    [SerializeField] Material regArrowMat;
    float arrowLength;
    BoxCollider boxCol;
    MeshCollider meshCol;
    bool hit;
    public int originalLayer;
    public Material originalMat;
    public GameObject collidedObj;
    public bool wallRunArrow;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>();
        meshCol = GetComponent<MeshCollider>();
        arrowLength = transform.localScale.z / 2f;
        if(wallRunArrow)
        {
            //GetComponent<Renderer>().material = wallRunArrowMat;
        }
        else
        {
            //GetComponent<Renderer>().material = regArrowMat;
        }
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
        //boxCol.enabled = true;
        //meshCol.enabled = false;

        //Rotate arrow to be parallel to wall
        ContactPoint contact = collision.contacts[0];
        Vector3 collisionNormal = contact.normal;
        Quaternion targetRotation = Quaternion.LookRotation(-collisionNormal);
        transform.rotation = targetRotation * Quaternion.Euler(90f, 0f, 0f);
        transform.position = contact.point + collisionNormal * arrowLength;

        if (wallRunArrow)
        {
            collidedObj = collision.gameObject;
            originalLayer = collision.gameObject.layer;
            originalMat = collision.gameObject.GetComponent<Renderer>().material;
            collision.gameObject.layer = LayerMask.NameToLayer("WallRun");
            collision.gameObject.GetComponent<Renderer>().material = wallRunMat;
            boxCol.enabled = true;
            meshCol.enabled = false;
        }

        //StartCoroutine(DelayedDestroy());
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(destroyTimer);
        collidedObj.layer = originalLayer;
        collidedObj.GetComponent<Renderer>().material = originalMat;
        Destroy(gameObject);
    }

    public void ResetWallRunObj()
    {
        collidedObj.layer = originalLayer;
        collidedObj.GetComponent<Renderer>().material = originalMat;
        Destroy(gameObject);
    }
}
