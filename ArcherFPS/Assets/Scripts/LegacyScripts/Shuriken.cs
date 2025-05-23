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
    public bool canSlingshot;
    BoxCollider boxCol;
    int originalLayer;
    Material originalMat;
    GameObject collidedObj;
    public bool wallRunStar;
    public bool teleportStar;
    public bool grappleStar;
    public bool slingshotStar;
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

        if (!grappleStar && !slingshotStar)
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

        AttachToWall(collision);

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
        else if(slingshotStar)
        {
            canSlingshot = true;
            boxCol.enabled = false;
        }
        else if (wallRunStar)
        {
            WallRunStar(collision);
            boxCol.enabled = false;
        }
        else
        {
            transform.localScale *= 2;
            //boxCol.size *= 2;
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

    void AttachToWall(Collision collision)
    {
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
    }


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

    /*
    void TeleportStar()
    {
        if (!isBlinking)
        {
            StartCoroutine(BlinkPlayer());
        }
    }

    IEnumerator BlinkPlayer()
    {
        smokePuff.Play();

        // Disable the CharacterController to prevent movement inputs during the blink
        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;

        Vector3 startPosition = player.transform.position;
        Vector3 targetPosition = transform.position;

        float distance = Vector3.Distance(startPosition, targetPosition);
        float travelTime = distance / blinkSpeed; // Calculate how long it should take based on speed

        float elapsedTime = 0f;
        isBlinking = true;

        // Move the player smoothly to the target position
        while (elapsedTime < travelTime)
        {
            // Calculate the new position based on time elapsed
            player.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / travelTime);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the player ends up exactly at the target position
        player.transform.position = targetPosition;

        // Re-enable the CharacterController and destroy the blink object
        controller.enabled = true;
        Destroy(gameObject);

        isBlinking = false;
    }
    */
}
