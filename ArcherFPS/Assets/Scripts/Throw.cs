using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    /*
    0 = Regular shuriken
    1 = Wallrun shuriken
    2 = Teleport shuriken
    3 = Grapple shuriken
    */

    [SerializeField] Shuriken[] starPrefab;
    [SerializeField] Transform starSpawnPoint;
    [SerializeField][Range(0, 3)] float zoomSpeed;
    [SerializeField] PlayerControllerV2 playerController;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform lineStartPos;
    [SerializeField] float lineRendererWidth;
    [SerializeField] GameObject indicatorObj;

    Shuriken currentStar;
    GameObject indicator;
    public bool isAiming; //Public for animator
    bool fire;
    Camera cam;
    float startingFOV;
    Vector3 destination;
    public int[] starCount;
    public int selectionIndex;

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
        ChooseShurikenType();

        if (isAiming)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 80f, Time.deltaTime * zoomSpeed);
            AimIndicator();
        }
        else if (fire)
        {
            float arrowPower = (startingFOV - cam.fieldOfView) * 5f;

            if (starCount[selectionIndex] > 0)
            {
                FireArrow(arrowPower);
            }
            else
            {
                fire = false;
                print("out of this type of shuriken");
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

        if (Physics.Raycast(ray, out hit) && !hit.transform.CompareTag("Ground"))
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

        currentStar = Instantiate(starPrefab[selectionIndex], starSpawnPoint);
        currentStar.GetComponent<Rigidbody>().velocity = (destination - starSpawnPoint.position).normalized * arrowPower;
        currentStar.GetComponent<Rigidbody>().AddTorque(transform.right * playerController.playerVelocity.x);
        currentStar.transform.SetParent(null);

        if(currentStar.teleportStar)
        {
            currentStar.player = this.gameObject;
        }

        starCount[selectionIndex]--;

        fire = false;
    }

    void ChooseShurikenType()
    {
        // Check if any shuriken is available
        bool shurikenAvailable = false;
        for (int i = 0; i < starCount.Length; i++)
        {
            if (starCount[i] > 0)
            {
                shurikenAvailable = true;
                break;
            }
        }

        // If no shuriken is available, break
        if (!shurikenAvailable)
        {
            print("out of shurikens");
            return;
        }

        // Cycle through available shuriken types
        if (Input.GetKeyDown(KeyCode.E))
        {
            do
            {
                selectionIndex = (selectionIndex + 1) % starCount.Length;
            }
            while (starCount[selectionIndex] == 0);
        }
    }
}
