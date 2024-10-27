using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShurikenPickup : MonoBehaviour
{
    [SerializeField] Throw starThrow;
    [SerializeField] TMP_Text pickupText;
    [SerializeField] float pickupRadius = 5f;
    private void Start()
    {
        pickupText.gameObject.SetActive(false);
    }

    private void Update()
    {
        DetectAndPickupShurikens();
    }

    private void DetectAndPickupShurikens()
    {
        //spherecast for shurikens within radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius);
        List<Shuriken> nearbyShurikens = new List<Shuriken>();
        
        //detect number of each shuriken for text
        int wallRunCount = 0;
        int grappleCount = 0;
        int slingshotCount = 0;
        int platformCount = 0;

        // Loop through the colliders to find any Shuriken objects
        foreach (Collider collider in colliders)
        {
            Shuriken shuriken = collider.gameObject.GetComponent<Shuriken>();
            if (shuriken != null && collider is CapsuleCollider && shuriken.hit)
            {
                nearbyShurikens.Add(shuriken);

                // Count each type of shuriken
                if (shuriken.wallRunStar)
                {
                    wallRunCount++;
                }
                else if (shuriken.grappleStar)
                {
                    grappleCount++;
                }
                else if (shuriken.slingshotStar)
                {
                    slingshotCount++;
                }
                else
                {
                    platformCount++;
                }
            }
        }

        // If shurikens are nearby, display the pickup text
        if (nearbyShurikens.Count > 0)
        {
            pickupText.gameObject.SetActive(true);
            string pickupMessage = "Press X to Pick Up:\n";
            // Add counts for each type of shuriken
            if (wallRunCount > 0)
                pickupMessage += $"{wallRunCount} Wall-Run Shuriken\n";
            if (grappleCount > 0)
                pickupMessage += $"{grappleCount} Grapple Shuriken\n";
            if (slingshotCount > 0)
                pickupMessage += $"{slingshotCount} Slingshot Shuriken\n";
            if (platformCount > 0)
                pickupMessage += $"{platformCount} Platform Shuriken\n";

            pickupText.text = pickupMessage;

            if (Input.GetKeyDown(KeyCode.X))
            {
                PickupShurikens(nearbyShurikens);
            }
        }
        else
        {
            pickupText.gameObject.SetActive(false);
        }
    }

    private void PickupShurikens(List<Shuriken> shurikens)
    {
        foreach (Shuriken shuriken in shurikens)
        {
            if (shuriken.wallRunStar)
            {
                starThrow.starCount[1]++;
                shuriken.ResetWallRunObj(); // Reset any special properties of the shuriken
            }
            else if (shuriken.grappleStar)
            {
                starThrow.starCount[3]++;
            }
            else
            {
                starThrow.starCount[0]++;
            }

            // Destroy the picked-up shuriken
            Destroy(shuriken.gameObject);
        }

        pickupText.gameObject.SetActive(false); // Hide the pickup text after collection
    }

    // Optional: to visualize the pickup radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
