using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(ShurikenManager))]
public class ShurikenPickup : MonoBehaviour
{
    /*
     * 0 - Platform
     * 1 - WallRun
     * 2 - Teleport
     * 3 - Grapple
     * 4 - Slingshot
     */

    [SerializeField] ShurikenManager manager;
    [SerializeField] TMP_Text pickupText;
    [SerializeField] float pickupRadius = 5f;
    private void Start()
    {
        pickupText.gameObject.SetActive(false);
        manager = GetComponent<ShurikenManager>();
    }

    private void Update()
    {
        DetectAndPickupShurikens();
    }

    private void DetectAndPickupShurikens()
    {
        //spherecast for shurikens within radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRadius);
        List<ShurikenBaseClass> nearbyShurikens = new List<ShurikenBaseClass>();
        
        //detect number of each shuriken for text
        int wallRunCount = 0;
        int grappleCount = 0;
        int slingshotCount = 0;
        int platformCount = 0;

        // Loop through the colliders to find any Shuriken objects
        foreach (Collider collider in colliders)
        {
            ShurikenBaseClass shuriken = collider.gameObject.GetComponent<ShurikenBaseClass>();
            if (shuriken != null && collider is CapsuleCollider && shuriken.hit)
            {
                nearbyShurikens.Add(shuriken);

                // Count each type of shuriken
                if (shuriken is WallRunShuriken)
                {
                    wallRunCount++;
                }
                else if (shuriken is GrappleShuriken)
                {
                    grappleCount++;
                }
                else if (shuriken is SlingshotShuriken)
                {
                    slingshotCount++;
                }
                else if (shuriken is PlatformShuriken)
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

    private void PickupShurikens(List<ShurikenBaseClass> shurikens)
    {
        foreach (ShurikenBaseClass shuriken in shurikens)
        {
            if (shuriken is PlatformShuriken)
            {
                manager.AddShuriken(0, 1);
            }
            else if (shuriken is WallRunShuriken wallRun)
            {
                manager.AddShuriken(1, 1);
                wallRun.ResetWallRunObject();
            }
            else if (shuriken is GrappleShuriken)
            {
                manager.AddShuriken(3, 1);
            }
            else if(shuriken is SlingshotShuriken)
            {
                manager.AddShuriken(4, 1);
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
