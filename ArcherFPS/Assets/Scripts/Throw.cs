using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    [SerializeField]
    Shuriken starPrefab;

    [SerializeField]
    Transform starSpawnPoint;

    [SerializeField]
    float delay;

    [SerializeField]
    [Range(0, 3)]
    float zoomSpeed;

    [SerializeField]
    PlayerControllerV2 playerController;

    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform lineStartPos;
    [SerializeField] float lineRendererWidth;
    [SerializeField] GameObject indicatorObj;

    Shuriken currentArrow;
    GameObject indicator;
    public bool isAiming; //Public for animator
    bool fire;
    Camera cam;
    float startingFOV;
    Vector3 destination;
    public bool useWallRunStars;
    public float wallRunStars;
    public float regularStars;

    void Start()
    {
        cam = Camera.main;
        startingFOV = cam.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineRendererWidth;
        lineRenderer.endWidth = lineRendererWidth;
        indicator = Instantiate(indicatorObj);
        indicator.SetActive(false);
    }

    void Update()
    {
        GetAimInput();
        ChooseArrowType();

        if (isAiming)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 80f, Time.deltaTime * zoomSpeed);
            AimIndicator();

            if(wallRunStars == 0 && regularStars == 0)
            {
                useWallRunStars = false;
            }
            else if(useWallRunStars && wallRunStars == 0)
            {
                useWallRunStars = false;
            }
            else if(!useWallRunStars && regularStars == 0)
            {
                useWallRunStars = true;
            }
        }
        else if (fire)
        {
            float arrowPower = (startingFOV - cam.fieldOfView) * 5f;
            if (wallRunStars > 0 && useWallRunStars || regularStars > 0 && !useWallRunStars)
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
            indicator.SetActive(false);
            lineRenderer.enabled = false;
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

    void AimIndicator()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        lineRenderer.SetPosition(0, lineStartPos.position);

        if (Physics.Raycast(ray, out hit))
        {
            lineRenderer.SetPosition(1, hit.point);
            indicator.SetActive(true);
            lineRenderer.enabled = true;
            indicator.transform.position = hit.point;
        }
        else
        {
            lineRenderer.enabled = false;
            indicator.SetActive(false);
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

        if (useWallRunStars)
        {
            wallRunStars--;
        }
        else
        {
            regularStars--;
        }

        currentArrow = Instantiate(starPrefab, starSpawnPoint);
        currentArrow.wallRunArrow = useWallRunStars;
        currentArrow.GetComponent<Rigidbody>().velocity = (destination - starSpawnPoint.position).normalized * arrowPower;
        currentArrow.GetComponent<Rigidbody>().AddTorque(transform.right * playerController.playerVelocity.x);
        currentArrow.transform.SetParent(null);

        fire = false;
    }

    void ChooseArrowType()
    {
        if(wallRunStars > 0)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                useWallRunStars = !useWallRunStars;
            }
        }
        else
        {
            useWallRunStars = false;
        }
    }
}
