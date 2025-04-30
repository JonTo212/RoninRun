using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShurikenCount : MonoBehaviour
{
    [SerializeField] Button fadeOut;
    [SerializeField] ShurikenManager manager;
    [SerializeField] TMP_Text shurikenCountText;
    [SerializeField] TMP_Text shurikenTypeText;
    [SerializeField] Image currentImage;
    int[] originalStarCounts;
    [SerializeField] Sprite[] shurikenImages;
    [SerializeField] string[] shurikenTypes;

    // Start is called before the first frame update
    void Start()
    {
        originalStarCounts = (int[])manager.shurikenCounts.Clone();
    }

    // Update is called once per frame
    void Update()
    {
        shurikenCountText.text = $"{manager.shurikenCounts[manager.selectedIndex]} / {originalStarCounts[manager.selectedIndex]}";
        shurikenTypeText.text = $"{ shurikenTypes[manager.selectedIndex]} Shuriken";
        currentImage.sprite = shurikenImages[manager.selectedIndex];
        currentImage.SetNativeSize();

        if (manager.shurikenCounts[manager.selectedIndex] <= 0)
        {
            fadeOut.interactable = false;
        }
        else
        {
            fadeOut.interactable = true;
        }
    }
}
