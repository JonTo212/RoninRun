using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwap : MonoBehaviour
{
    public GameObject ThirdPersonCamera;
    public GameObject FirstPersonCamera;
    public int cam;
    [SerializeField] Material invisShadowMat;
    [SerializeField] Material originalMat;
    [SerializeField] GameObject hatObj;

    private void Start()
    {
        hatObj.GetComponent<Renderer>().material = invisShadowMat;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && Time.timeScale == 1)
        {
            if (cam == 1)
            {
                cam = 0;
            }
            else
            {
                cam += 1;
            }
            StartCoroutine(CameraChange());
        }
    }

    IEnumerator CameraChange()
    {
        yield return new WaitForSeconds(0.01f);
        if (cam == 0)
        {
            ThirdPersonCamera.SetActive(false);
            FirstPersonCamera.SetActive(true);
            hatObj.GetComponent<Renderer>().material = invisShadowMat;
        }
        if (cam == 1)
        {
            ThirdPersonCamera.SetActive(true);
            FirstPersonCamera.SetActive(false);
            hatObj.GetComponent<Renderer>().material = originalMat;
        }
    }
}
