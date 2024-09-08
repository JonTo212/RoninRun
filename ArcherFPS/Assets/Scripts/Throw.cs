using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

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
    [SerializeField] LayerMask playerMask;
    [SerializeField] LayerMask ignoreRaycastMask;
    [SerializeField] LayerMask thrownObjectMask;
    [SerializeField] ParticleSystem smokePuff;
    LayerMask excludedLayers;

    Shuriken currentStar;
    GameObject indicator;
    public bool isAiming; //Public for animator
    bool fire;
    float throwPower;
    Camera cam;
    float startingFOV;
    Vector3 destination;
    public int[] starCount;
    public int selectionIndex;
    public Vector3 finalPos;
    public bool noThrow;

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
        indicator.transform.GetChild(selectionIndex).gameObject.SetActive(true);
        excludedLayers = playerMask | ignoreRaycastMask | thrownObjectMask;
        excludedLayers = ~excludedLayers;
    }

    void Update()
    {
        GetAimInput();
        ChooseShurikenType();
        SetIndicatorObjectsActive();

        if (isAiming)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 80f, Time.deltaTime * zoomSpeed);
            throwPower = (startingFOV - cam.fieldOfView) * 2f;
            AimIndicator(throwPower);
        }
        else if (fire)
        {
            if (starCount[selectionIndex] > 0)
            {
                FireShuriken(throwPower);
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
        if (Input.GetMouseButtonDown(0) && !isAiming)
        {
            isAiming = true;
        }
        else if(Input.GetMouseButtonUp(1)) //for animator
        {
            noThrow = false;
        }

        if (Input.GetMouseButtonUp(0) && isAiming)
        {
            isAiming = false;
            fire = true;
        }
        else if(Input.GetMouseButtonDown(1) && isAiming) //for animator
        {
            isAiming = false;
            fire = false;
            noThrow = true;
        }
    }

    void AimIndicator(float projectileSpeed)
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

        Vector3 velocity = (destination - starSpawnPoint.position).normalized * projectileSpeed;

        // Predicted angular velocity (same as what will be applied to the object after instantiation)
        Vector3 predictedAngularVelocity = Vector3.zero;
        if (selectionIndex != 3)
        {
            predictedAngularVelocity = starPrefab[selectionIndex].transform.forward * projectileSpeed;
        }

        float simulationTimeStep = 0.02f; //Simulation increment, make larger for less accurate increments and smaller for more accurate increments
        float maxSimulationDistance = 200f; //Distance in units that the arc will simulate to

        // Simulate trajectory
        List<Vector3> trajectoryPoints = new List<Vector3>();
        Vector3 currentPosition = lineStartPos.position;
        Quaternion currentRotation = Quaternion.Euler(90, 0, 45); // Initial rotation

        for (int i = 0; i < maxSimulationDistance; i++)
        {
            trajectoryPoints.Add(currentPosition);

            // Apply physics to calculate next position
            currentPosition += velocity * simulationTimeStep;
            velocity += Physics.gravity * simulationTimeStep; // Adjust for gravity
            currentRotation *= Quaternion.Euler(predictedAngularVelocity * simulationTimeStep); // Simulate rotation

            // Check for collision with a box collider
            if (Physics.CheckBox(currentPosition, new Vector3(0.45f, 0.45f, 0.075f), currentRotation, excludedLayers)) //added extra size to detection checkbox for more accuracy
            {
                trajectoryPoints.Add(currentPosition); //idk if I should keep this, it sometimes adds an extra point which makes it inaccurate
                finalPos = currentPosition;
                break; // Stop simulating on collision
            }
            else
            {
                finalPos = Vector3.zero;
            }
        }

        // Apply the calculated trajectory points to the LineRenderer
        lineRenderer.positionCount = trajectoryPoints.Count;
        lineRenderer.SetPositions(trajectoryPoints.ToArray());

        // Show/hide indicator
        if (trajectoryPoints.Count > 1)
        {
            indicator.SetActive(true);
            indicator.transform.position = trajectoryPoints[trajectoryPoints.Count - 1]; // Position indicator at the last point
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
            indicator.SetActive(false);
        }
    }

    /*void AimIndicator(float projectileSpeed) //legacy function, works well but the hitbox is not accounted for
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

        Vector3 velocity = (destination - starSpawnPoint.position).normalized * projectileSpeed;

        float simulationTimeStep = 0.05f; //Simulation increment, make larger for less accurate increments and smaller for more accurate increments
        float maxSimulationDistance = 100f; //Distance in units that the arc will simulate to

        // Simulate trajectory
        List<Vector3> trajectoryPoints = new List<Vector3>();
        Vector3 currentPosition = lineStartPos.position;

        for (int i = 0; i < maxSimulationDistance; i++)
        {
            trajectoryPoints.Add(currentPosition);

            // Apply physics to calculate next position
            currentPosition += velocity * simulationTimeStep;
            velocity += Physics.gravity * simulationTimeStep; // Adjust for gravity

            //Stop simulating arc if aiming at object
            if (Physics.Raycast(currentPosition, velocity, out hit, velocity.magnitude * simulationTimeStep))
            {
                trajectoryPoints.Add(hit.point); // Add the hit point
                indicator.transform.rotation = Quaternion.LookRotation(-hit.normal) * Quaternion.Euler(90f, 0f, 45f);
                break; // Stop simulating on collision
            }
        }

        // Apply the calculated trajectory points to the LineRenderer
        lineRenderer.positionCount = trajectoryPoints.Count;
        lineRenderer.SetPositions(trajectoryPoints.ToArray());

        // Show/hide indicator
        if (trajectoryPoints.Count > 1)
        {
            indicator.SetActive(true);
            indicator.transform.position = trajectoryPoints[trajectoryPoints.Count - 1]; // Position indicator at the last point
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
            indicator.SetActive(false);
        }
    }*/


    void SetIndicatorObjectsActive()
    {
        for (int i = 0; i < indicator.transform.childCount; i++)
        {
            GameObject child = indicator.transform.GetChild(i).gameObject;

            if (i == selectionIndex)
            {
                child.SetActive(true);
            }
            else
            {
                child.SetActive(false);
            }
        }
        
        //Rotate indicator to face forward
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward);
        indicator.transform.rotation = targetRotation * Quaternion.Euler(90, 45, 0);
    }


    void FireShuriken(float projectileSpeed)
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
        currentStar.transform.SetParent(null);
        currentStar.GetComponent<Rigidbody>().velocity = (destination - starSpawnPoint.position).normalized * projectileSpeed;
        currentStar.finalPos = finalPos;

        if (!currentStar.grappleStar)
        {
            currentStar.GetComponent<Rigidbody>().AddTorque(currentStar.transform.forward * projectileSpeed, ForceMode.VelocityChange);
        }

        if(currentStar.teleportStar)
        {
            currentStar.player = this.gameObject;
            currentStar.smokePuff = smokePuff;
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
        if (Input.GetKeyDown(KeyCode.F))
        {
            do
            {
                selectionIndex = (selectionIndex + 1) % starCount.Length;
            }
            while (starCount[selectionIndex] == 0);
        }
    }
}
