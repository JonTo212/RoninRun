using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashCooldown : MonoBehaviour
{
    [SerializeField] Image imageCooldown;

    [SerializeField] JettController abilityController;

    // Start is called before the first frame update
    void Start()
    {
        imageCooldown.fillAmount = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyCooldown();
    }

    void ApplyCooldown()
    {
        imageCooldown.fillAmount = abilityController.dashCooldown / abilityController.dashDelay;
    }
}
