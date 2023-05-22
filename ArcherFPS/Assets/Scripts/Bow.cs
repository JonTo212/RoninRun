using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [SerializeField]
    Arrow arrowPrefab;

    [SerializeField]
    Transform arrowSpawnPoint;

    [SerializeField]
    float delay;

    [SerializeField]
    float arrowPower;

    [SerializeField]
    [Range(0, 3)]
    float zoomSpeed;

    Arrow currentArrow;
    bool isAiming;
    bool zoomedIn;
    bool fire;
    Camera cam;
    float startingFOV;

    void Start()
    {
        cam = Camera.main;
        startingFOV = cam.fieldOfView;
    }

    void Update()
    {
        GetAimInput();

        if (isAiming)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 80f, Time.deltaTime * zoomSpeed);
        }
        else if (fire)
        {
            FireArrow();
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, startingFOV, 0.1f);
        }

    }

    void GetAimInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isAiming = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isAiming = false;
            fire = true;
        }
    }

    void FireArrow()
    {
        currentArrow = Instantiate(arrowPrefab, arrowSpawnPoint);
        currentArrow.Fly(arrowSpawnPoint.forward * arrowPower);
        fire = false;
    }
}
