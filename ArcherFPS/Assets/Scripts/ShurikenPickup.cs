using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShurikenPickup : MonoBehaviour
{
    [SerializeField] Throw starThrow;
    [SerializeField] TMP_Text pickupText;

    private void Start()
    {
        pickupText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Shuriken>() != null)
        {
            Shuriken shuriken = other.gameObject.GetComponent<Shuriken>();
            pickupText.gameObject.SetActive(true);

            if (shuriken.wallRunStar)
            {
                pickupText.text = "Press F to Pick Up Wall-Run Shuriken";
            }
            else if(shuriken.grappleStar)
            {
                pickupText.text = "Press F to Pick Up Grapple Shuriken";
            }
            else
            {
                pickupText.text = "Press F to Pick Up Platform Shuriken";
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.GetComponent<Shuriken>() != null)
        {
            Shuriken shuriken = other.gameObject.GetComponent<Shuriken>();

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (shuriken.wallRunStar)
                {
                    starThrow.starCount[1]++;
                    shuriken.ResetWallRunObj();
                }
                else if (shuriken.grappleStar)
                {
                    starThrow.starCount[3]++;
                }
                else
                {
                    starThrow.starCount[0]++;
                }

                Destroy(other.gameObject);
                pickupText.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        pickupText.gameObject.SetActive(false);
    }
}
