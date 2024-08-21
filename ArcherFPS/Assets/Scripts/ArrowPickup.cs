using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ArrowPickup : MonoBehaviour
{
    [SerializeField] Bow bow;
    [SerializeField] TMP_Text pickupText;

    private void Start()
    {
        pickupText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Arrow>() != null)
        {
            Arrow arrow = other.gameObject.GetComponent<Arrow>();
            pickupText.gameObject.SetActive(true);

            if (arrow.wallRunArrow)
            {
                pickupText.text = "Press F to Pick Up Arrow (Wall Run)";
            }
            else if (!arrow.wallRunArrow)
            {
                pickupText.text = "Press F to Pick Up Arrow (Platform)";
            }

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.GetComponent<Arrow>() != null)
        {
            Arrow arrow = other.gameObject.GetComponent<Arrow>();

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (arrow.wallRunArrow)
                {
                    bow.wallRunArrows++;
                }
                else if (!arrow.wallRunArrow)
                {
                    bow.regularArrows++;
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
