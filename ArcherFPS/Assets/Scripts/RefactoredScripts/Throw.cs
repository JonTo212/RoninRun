using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShurikenManager))]
public class Throw : MonoBehaviour
{
    [SerializeField] private Transform starSpawnPoint;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private PlayerControllerV2 playerController;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform lineStartPos;
    [SerializeField] private float lineRendererWidth;
    [SerializeField] private GameObject indicatorObj;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask ignoreRaycastMask;
    [SerializeField] private LayerMask thrownObjectMask;
    [SerializeField] private ParticleSystem smokePuff;
    [SerializeField] private ShurikenManager shurikenManager;

    private LayerMask excludedLayers;
    private GameObject indicator;
    //private bool fire;
    private float throwPower;
    private Camera cam;
    private float startingFOV;
    private Vector3 destination;
    public Vector3 finalPos;
    private float originalGrav;
    public float desiredFOV;
    public float currentZoomSpeed;

    public bool isAiming;
    public bool stopThrow;
    public bool canThrow;

    void Start()
    {
        canThrow = true;
        cam = Camera.main;
        startingFOV = cam.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineRendererWidth;
        lineRenderer.endWidth = lineRendererWidth;
        indicator = Instantiate(indicatorObj);
        indicator.transform.GetChild(shurikenManager.selectedIndex).gameObject.SetActive(true);
        excludedLayers = ~(playerMask | ignoreRaycastMask | thrownObjectMask);
        originalGrav = playerController.gravity;
        shurikenManager = GetComponent<ShurikenManager>();
    }

    /*void Update()
    {
        if (canThrow)
        {
            GetAimInput();
            ChooseShurikenType();
            SetIndicatorObjectsActive();

            if (isAiming)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, Time.deltaTime * zoomSpeed);
                throwPower = (startingFOV - cam.fieldOfView) * 2f;
                AimIndicator(throwPower);
                playerController.gravity = originalGrav / 2;
            }
            else if (fire)
            {
                if (shurikenManager.CanThrowSelected())
                {
                    FireShuriken(throwPower);
                }
                else
                {
                    fire = false;
                    isAiming = false;
                    stopThrow = true;
                    Debug.Log("Out of this type of shuriken");
                }
                playerController.gravity = originalGrav;
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, startingFOV, 0.1f);
                indicator.SetActive(false);
                lineRenderer.enabled = false;
            }
        }
        else
        {
            ResetThrowingInput();
        }
    }*/


    public bool CheckThrow()
    {
        return Input.GetMouseButtonDown(0);
    }

    public void ReleaseThrow()
    {
        if (shurikenManager.CanThrowSelected())
        {
            FireShuriken(throwPower);
            StopThrow(false);
        }
        else
        {
            StopThrow(true);
            Debug.Log("Out of this type of shuriken");
        }
    }

    public void StopThrow(bool cancel)
    {
        isAiming = false;
        stopThrow = cancel;
        playerController.gravity = originalGrav;
        indicator.SetActive(false);
        lineRenderer.enabled = false;

        desiredFOV = startingFOV;
        currentZoomSpeed = 0.1f;
    }

    public void HandleThrowPower()
    {
        isAiming = true;
        desiredFOV = 60f;
        currentZoomSpeed = zoomSpeed * Time.deltaTime;
        throwPower = (startingFOV - cam.fieldOfView) * 2f;
        AimIndicator(throwPower);
        playerController.gravity = originalGrav / 2;
    }

    public void HandleFOV()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFOV, currentZoomSpeed);
    }

    /*void GetAimInput()
    {
        if (Input.GetMouseButtonDown(0) && !isAiming)
        {
            isAiming = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            stopThrow = false;
            playerController.gravity = originalGrav;
        }

        if (Input.GetMouseButtonUp(0) && isAiming)
        {
            isAiming = false;
            fire = true;
            playerController.gravity = originalGrav;
        }
        else if (Input.GetMouseButtonDown(1) && isAiming)
        {
            isAiming = false;
            fire = false;
            stopThrow = true;
            playerController.gravity = originalGrav;
        }
    }*/

    void AimIndicator(float projectileSpeed)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        destination = Physics.Raycast(ray, out RaycastHit hit)
            ? hit.point
            : ray.GetPoint(1000);

        Vector3 velocity = (destination - starSpawnPoint.position).normalized * projectileSpeed;
        Vector3 predictedAngularVelocity = Vector3.zero;

        int selectedIndex = shurikenManager.selectedIndex;
        ShurikenBaseClass selectedPrefab = shurikenManager.GetSelectedShurikenPrefab();

        if (!(selectedPrefab is GrappleShuriken || selectedPrefab is SlingshotShuriken))
        {
            predictedAngularVelocity = selectedPrefab.transform.forward * projectileSpeed;
        }

        float simulationTimeStep = 0.01f;
        float maxSimulationDistance = 200f;
        List<Vector3> trajectoryPoints = new List<Vector3>();
        Vector3 currentPosition = lineStartPos.position;
        Quaternion currentRotation = Quaternion.Euler(90, 0, 45);

        for (int i = 0; i < maxSimulationDistance; i++)
        {
            trajectoryPoints.Add(currentPosition);
            currentPosition += velocity * simulationTimeStep;
            velocity += Physics.gravity * simulationTimeStep;
            currentRotation *= Quaternion.Euler(predictedAngularVelocity * simulationTimeStep);

            if (Physics.CheckBox(currentPosition, new Vector3(0.3f, 0.3f, 0.05f), currentRotation, excludedLayers))
            {
                trajectoryPoints.Add(currentPosition);
                finalPos = currentPosition;
                break;
            }
            else
            {
                finalPos = Vector3.zero;
            }
        }

        lineRenderer.positionCount = trajectoryPoints.Count;
        lineRenderer.SetPositions(trajectoryPoints.ToArray());

        if (trajectoryPoints.Count > 1)
        {
            indicator.SetActive(true);
            indicator.transform.position = trajectoryPoints[^1];
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
            indicator.SetActive(false);
        }
    }

    public void SetIndicatorObjectsActive()
    {
        int selectedIndex = shurikenManager.selectedIndex;

        for (int i = 0; i < indicator.transform.childCount; i++)
        {
            GameObject child = indicator.transform.GetChild(i).gameObject;
            child.SetActive(i == selectedIndex);
        }

        Quaternion targetRotation = Quaternion.LookRotation(transform.forward);
        indicator.transform.rotation = targetRotation * Quaternion.Euler(90, 45, 0);
    }

    void FireShuriken(float projectileSpeed)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        destination = Physics.Raycast(ray, out RaycastHit hit)
            ? hit.point
            : ray.GetPoint(1000);

        ShurikenBaseClass prefab = shurikenManager.GetSelectedShurikenPrefab();
        ShurikenBaseClass newShuriken = Instantiate(prefab, starSpawnPoint.position, starSpawnPoint.rotation);

        Rigidbody rb = newShuriken.GetComponent<Rigidbody>();
        Vector3 launchVelocity = (destination - starSpawnPoint.position).normalized * projectileSpeed;
        rb.velocity = launchVelocity;

        newShuriken.finalPos = finalPos;

        if (!(newShuriken is GrappleShuriken))
        {
            rb.AddTorque(newShuriken.transform.forward * projectileSpeed, ForceMode.VelocityChange);
        }

        if (newShuriken is TeleportShuriken teleport)
        {
            teleport.Initialize(this.gameObject, smokePuff);
        }

        shurikenManager.RemoveShuriken();
        //fire = false;
    }

    public void ChooseShurikenType()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                shurikenManager.SelectShuriken(i);
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            shurikenManager.CycleShuriken();
        }
    }

    /*void ResetThrowingInput()
    {
        isAiming = false;
        fire = false;
        stopThrow = true;
        playerController.gravity = originalGrav;
        indicator.SetActive(false);
        lineRenderer.enabled = false;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, startingFOV, 0.1f);
    }*/
}
