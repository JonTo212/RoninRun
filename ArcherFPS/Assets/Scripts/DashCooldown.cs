using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashCooldown : MonoBehaviour
{
    [SerializeField] Image imageCooldown;
    [SerializeField] Button fadeOut;

    [SerializeField] JettController abilityController;
    [SerializeField] PlayerAbilities playerAbilities;

    // Start is called before the first frame update
    void Start()
    {
        if (abilityController != null)
        {
            imageCooldown.fillAmount = 0.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(abilityController != null)
        {
            ApplyCooldown();
        }
        else if(playerAbilities != null)
        {
            if(!playerAbilities.hasDashed)
            {
                fadeOut.interactable = true;
            }
            else
            {
                fadeOut.interactable = false;
            }
        }
    }

    void ApplyCooldown()
    {
        imageCooldown.fillAmount = abilityController.dashCooldown / abilityController.dashDelay;
    }
}
