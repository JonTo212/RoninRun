using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwap : MonoBehaviour
{
    public GameObject ThirdPersonCamera;
    public GameObject FirstPersonCamera;
    public int cam;

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
        }
        if (cam == 1)
        {
            ThirdPersonCamera.SetActive(true);
            FirstPersonCamera.SetActive(false);
        }
    }
}
