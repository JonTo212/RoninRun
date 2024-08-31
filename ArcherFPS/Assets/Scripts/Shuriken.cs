using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuriken : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    //[SerializeField] float destroyTimer;
    [SerializeField] Material wallRunMat;
    public GameObject player;
    bool hit;
    public bool canGrapple;
    BoxCollider boxCol;
    public int originalLayer;
    public Material originalMat;
    public GameObject collidedObj;
    public bool wallRunStar;
    public bool teleportStar;
    public bool grappleStar;
    public Vector3 angularVel;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>();
        rb.maxAngularVelocity = Mathf.Infinity;
    }

    private void Update()
    {
        if(teleportStar)
        {
            if (Input.GetMouseButtonDown(1))
            {
                TeleportStar();
            }
        }

       //LookDir();
    }

    void LookDir() //rotates object to face arc's forward, but gets rid of spin
    {
        Vector3 fallDirection = rb.velocity.normalized;
        Quaternion initialRotation = Quaternion.Euler(90, 0, 45);
        Quaternion targetRotation = Quaternion.LookRotation(fallDirection, Vector3.up) * initialRotation;
        transform.rotation = targetRotation;
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

        if (teleportStar)
        {
            TeleportStar();
            boxCol.enabled = false;
            return;
        }
        else if(grappleStar)
        {
            canGrapple = true;
            boxCol.enabled = false;
        }
        else if (wallRunStar)
        {
            WallRunStar(collision);
            boxCol.enabled = false;
        }
        else
        {
            boxCol.size *= 2;
        }

        //Rotate shuriken to be perpendicular to wall
        ContactPoint contact = collision.contacts[0];
        Vector3 collisionNormal = contact.normal;
        Quaternion targetRotation = Quaternion.LookRotation(-collisionNormal);
        transform.rotation = targetRotation * Quaternion.Euler(90f, 0f, 45f);
        transform.position = contact.point + collisionNormal * boxCol.size.z / 2;

        //StartCoroutine(DelayedDestroy());
    }

    /*IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(destroyTimer);
        collidedObj.layer = originalLayer;
        collidedObj.GetComponent<Renderer>().material = originalMat;
        Destroy(gameObject);
    }*/

    public void ResetWallRunObj()
    {
        collidedObj.layer = originalLayer;
        collidedObj.GetComponent<Renderer>().material = originalMat;
        Destroy(gameObject);
    }

    void WallRunStar(Collision collision)
    {
        collidedObj = collision.gameObject;
        originalLayer = collision.gameObject.layer;
        originalMat = collision.gameObject.GetComponent<Renderer>().material;
        collision.gameObject.layer = LayerMask.NameToLayer("WallRun");
        collision.gameObject.GetComponent<Renderer>().material = wallRunMat;
    }

    void TeleportStar()
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = transform.position;
        player.GetComponent<CharacterController>().enabled = true;
        Destroy(gameObject);
    }
}
