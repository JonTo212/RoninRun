using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShurikenCount : MonoBehaviour
{
    [SerializeField] Button fadeOut;
    [SerializeField] Throw shurikenThrow;
    [SerializeField] TMP_Text shurikenCountText;
    [SerializeField] TMP_Text shurikenTypeText;
    [SerializeField] Image currentImage;
    int[] starCounts;
    [SerializeField] Sprite[] shurikenImages;
    [SerializeField] string[] shurikenTypes;

    // Start is called before the first frame update
    void Start()
    {
        starCounts = new int[shurikenThrow.starCount.Length];
        for(int i = 0; i < starCounts.Length; i++)
        {
            starCounts[i] = shurikenThrow.starCount[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        shurikenCountText.text = $"{shurikenThrow.starCount[shurikenThrow.selectionIndex]} / {starCounts[shurikenThrow.selectionIndex]}";
        shurikenTypeText.text = $"{ shurikenTypes[shurikenThrow.selectionIndex]} Shuriken";
        currentImage.sprite = shurikenImages[shurikenThrow.selectionIndex];
        currentImage.SetNativeSize();

        if (shurikenThrow.starCount[shurikenThrow.selectionIndex] <= 0)
        {
            fadeOut.interactable = false;
        }
        else
        {
            fadeOut.interactable = true;
        }
    }
}
