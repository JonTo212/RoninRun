using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdraftCooldown : MonoBehaviour
{
    [SerializeField] TMP_Text updraftCount;
    [SerializeField] Button fadeOut;

    [SerializeField] JettController abilityController;
    [SerializeField] PlayerAbilities playerAbilities;

    // Update is called once per frame
    void Update()
    {
        if (abilityController != null)
        {
            updraftCount.text = (abilityController.maxUpdrafts - abilityController.updraftAttempts).ToString();
        }

        else if (playerAbilities != null)
        {
            if (playerAbilities.canUpdraft)
            {
                fadeOut.interactable = true;
            }
            else
            {
                fadeOut.interactable = false;
            }
        }
    }
}
