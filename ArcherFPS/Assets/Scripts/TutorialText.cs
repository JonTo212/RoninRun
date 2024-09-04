using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    [SerializeField] GameObject tutorialText;
    [SerializeField] [TextArea] string text;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Ensure your player has the "Player" tag
        {
            tutorialText.SetActive(true);
            tutorialText.GetComponent<TMP_Text>().text = text;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialText.SetActive(false);
            tutorialText.GetComponent<TMP_Text>().text = "";
        }
    }
}
