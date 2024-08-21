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
    PlayerControllerV2 playerController;

    Arrow currentArrow;
    public bool isAiming; //Public for animator
    bool fire;
    Camera cam;
    float startingFOV;
    Vector3 destination;
    public bool useWallRunArrows;
    public float wallRunArrows;
    public float regularArrows;

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
        ChooseArrowType();

        if (isAiming)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 80f, Time.deltaTime * zoomSpeed);
            if(wallRunArrows == 0 && regularArrows == 0)
            {
                useWallRunArrows = false;
            }
            else if(useWallRunArrows && wallRunArrows == 0)
            {
                useWallRunArrows = false;
            }
            else if(!useWallRunArrows && regularArrows == 0)
            {
                useWallRunArrows = true;
            }
        }
        else if (fire)
        {
            float arrowPower = (startingFOV - cam.fieldOfView) * 5f;
            if (wallRunArrows > 0 && useWallRunArrows || regularArrows > 0 && !useWallRunArrows)
            {
                FireArrow(arrowPower);
            }
            else
            {
                fire = false;
                print("out of arrows");
            }
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

        if (useWallRunArrows)
        {
            wallRunArrows--;
        }
        else
        {
            regularArrows--;
        }

        currentArrow = Instantiate(arrowPrefab, arrowSpawnPoint);
        currentArrow.wallRunArrow = useWallRunArrows;
        currentArrow.GetComponent<Rigidbody>().velocity = (destination - arrowSpawnPoint.position).normalized * arrowPower;
        currentArrow.GetComponent<Rigidbody>().AddTorque(transform.right * playerController.playerVelocity.x);
        currentArrow.transform.SetParent(null);

        fire = false;
    }

    void ChooseArrowType()
    {
        if(wallRunArrows > 0)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                useWallRunArrows = !useWallRunArrows;
            }
        }
        else
        {
            useWallRunArrows = false;
        }
    }
}
