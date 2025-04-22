using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwap : MonoBehaviour
{
    public GameObject ThirdPersonCamera;
    public GameObject FirstPersonCamera;
    [SerializeField] Material invisShadowMat;
    [SerializeField] Material originalMat;
    [SerializeField] GameObject hatObj;
    [SerializeField] Throw throwScript;

    private void Start()
    {
        hatObj.GetComponent<Renderer>().material = invisShadowMat;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && Time.timeScale == 1)
        {
            if (ThirdPersonCamera.gameObject.activeInHierarchy)
            {
                ChangeCamera(invisShadowMat);
            }
            else
            {
                ChangeCamera(originalMat);
            } 
        }
    }

    void ChangeCamera(Material hatObjMat)
    {
        ThirdPersonCamera.SetActive(!ThirdPersonCamera.activeInHierarchy);
        FirstPersonCamera.SetActive(!FirstPersonCamera.activeInHierarchy);
        throwScript.canThrow = !throwScript.canThrow;
        hatObj.GetComponent<Renderer>().material = hatObjMat;
    }
}
