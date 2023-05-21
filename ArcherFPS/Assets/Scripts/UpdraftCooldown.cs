using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdraftCooldown : MonoBehaviour
{
    [SerializeField] TMP_Text updraftCount;

    [SerializeField] JettController abilityController;

    // Update is called once per frame
    void Update()
    {
        updraftCount.text = (abilityController.maxUpdrafts - abilityController.updraftAttempts).ToString();
    }
}
