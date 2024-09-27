using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuriken : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    //[SerializeField] float destroyTimer;
    [SerializeField] Material wallRunMat;
    public GameObject player;
    public bool hit;
    public bool canGrapple;
    BoxCollider boxCol;
    int originalLayer;
    Material originalMat;
    GameObject collidedObj;
    public bool wallRunStar;
    public bool teleportStar;
    public bool grappleStar;
    float additionalSpin;
    [SerializeField] float spinRate;
    public Vector3 finalPos;
    public ParticleSystem smokePuff;

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

        if (!hit)
        {
            LookDir();
        }
    }

    void LookDir() //rotates object to face arc's forward, but gets rid of spin
    {
        Vector3 fallDirection = rb.velocity.normalized;
        Quaternion initialRotation = Quaternion.Euler(90, 0, 45);
        Quaternion targetRotation = Quaternion.LookRotation(fallDirection, Vector3.up) * initialRotation;

        if (!grappleStar)
        {
            Quaternion additionalRot = Quaternion.Euler(0, 0, additionalSpin);
            transform.rotation = targetRotation * additionalRot;
            additionalSpin += spinRate;
        }

        else
        {
            transform.rotation = targetRotation;
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

        //Rotate shuriken to be perpendicular to wall
        ContactPoint contact = collision.contacts[0];
        Vector3 collisionNormal = contact.normal;
        Quaternion targetRotation = Quaternion.LookRotation(-collisionNormal);
        transform.rotation = targetRotation * Quaternion.Euler(90f, 0f, 45f);

        if (finalPos != Vector3.zero)
        {
            transform.position = finalPos;
        }

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
        smokePuff.Play();
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = transform.position;
        player.GetComponent<CharacterController>().enabled = true;
        Destroy(gameObject);
    }
}
