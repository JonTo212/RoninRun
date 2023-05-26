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
    [Range(0, 3)]
    float zoomSpeed;

    [SerializeField]
    PlayerController playerController;

    Arrow currentArrow;
    bool isAiming;
    bool zoomedIn;
    bool fire;
    Camera cam;
    float startingFOV;
    Vector3 destination;


    void Start()
    {
        cam = Camera.main;
        startingFOV = cam.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
            float arrowPower = (startingFOV - cam.fieldOfView) * 5f;
            FireArrow(arrowPower);
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

    void FireArrow(float arrowPower)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            destination = hit.point;
        }
        else
        {
            destination = ray.GetPoint(1000);
        }

        currentArrow = Instantiate(arrowPrefab, arrowSpawnPoint);
        currentArrow.GetComponent<Rigidbody>().velocity = (destination - arrowSpawnPoint.position).normalized * arrowPower;
        currentArrow.GetComponent<Rigidbody>().AddTorque(transform.right * playerController.playerVelocity.x);
        currentArrow.transform.SetParent(null);
        //currentArrow.Fly((destination.normalized) * arrowPower);
        fire = false;
    }
}
