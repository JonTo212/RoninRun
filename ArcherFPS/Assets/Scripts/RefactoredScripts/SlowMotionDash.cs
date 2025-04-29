using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlowMotionDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private int maxPoints = 3;
    [SerializeField] private float pointMaxDistance = 5f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float slowMoTimeScale = 0.2f;
    [SerializeField] private float pointSelectTimeout = 3f; // Max seconds to pick points
    [SerializeField] private float dashPauseDuration;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerControllerV2 playerController;
    public LineRenderer lineRenderer; // Optional for preview
    private ParticleSystem forwardDashParticles;

    private List<Vector3> dashPoints = new List<Vector3>();
    private bool isSelectingPoints = false;
    [HideInInspector] public bool isDashing = false;
    private float pointSelectTimer = 0f;

    [SerializeField] private Throw throwScript;
    private float originalXMouseSensitivity;
    private float originalYMouseSensitivity;

    private void Start()
    {
        originalXMouseSensitivity = playerController.xMouseSensitivity;
        originalYMouseSensitivity = playerController.yMouseSensitivity;
        forwardDashParticles = GameObject.FindGameObjectWithTag("ForwardDashParticles").GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isDashing && !isSelectingPoints)
        {
            StartSlowMoDash();
        }

        if (isSelectingPoints)
        {
            UpdatePointSelection();
            UpdateVisualIndicator();
        }
    }

    void StartSlowMoDash()
    {
        dashPoints.Clear();
        isSelectingPoints = true;
        pointSelectTimer = 0f;
        throwScript.enabled = false;

        Time.timeScale = slowMoTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    void UpdatePointSelection()
    {
        pointSelectTimer += Time.unscaledDeltaTime; // Important: unscaled because time is slowed

        if (Input.GetMouseButtonDown(0) && dashPoints.Count < maxPoints)
        {
            Vector3 targetPos = GetTargetPoint();
            dashPoints.Add(targetPos);
        }

        // Auto-end if enough points or timeout
        if (dashPoints.Count >= maxPoints || pointSelectTimer >= pointSelectTimeout || Input.GetMouseButtonDown(1))
        {
            EndPointSelection();
        }
    }

    void EndPointSelection()
    {
        isSelectingPoints = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        lineRenderer.positionCount = 0; // Clear line preview

        StartDash();
    }

    void StartDash()
    {
        isDashing = true;
        playerController.playerVelocity.y = 0;
        playerController.enabled = false;

        StartCoroutine(ExecuteDash());
    }

    IEnumerator ExecuteDash()
    {
        Vector3 finalDashDir = Vector3.zero;

        for (int i = 0; i < dashPoints.Count; i++)
        {
            Vector3 point = dashPoints[i];
            forwardDashParticles.Play();
            isDashing = true;

            //small buffer so you don't get stuck in walls
            while (Vector3.Distance(transform.position, point) > 0.5f)
            {
                Vector3 direction = (point - transform.position).normalized;
                Vector3 accel = direction * dashSpeed * Time.deltaTime;
                transform.forward = Vector3.Lerp(transform.forward, direction, 5f * Time.deltaTime);
                playerController.playerView.forward = Vector3.Lerp(playerController.playerView.forward, direction, 5f * Time.deltaTime);
                characterController.Move(accel);

                //save final direction to apply velocity
                if (i == dashPoints.Count - 1)
                {
                    finalDashDir = direction;
                }

                yield return null;
            }

            if (i < dashPoints.Count - 1)
            {
                isDashing = false;
                yield return new WaitForSeconds(dashPauseDuration);
            }
        }

        //continue with the final direction's velocity
        playerController.playerVelocity = finalDashDir * dashSpeed;
        EndDash();

    }
    void EndDash()
    {
        ClearVisualIndicator();
        isDashing = false;
        throwScript.enabled = true;
        playerController.enabled = true;
    }


    Vector3 GetTargetPoint()
    {
        Vector3 rayOrigin;

        if (dashPoints.Count == 0)
        {
            // First point: cast from player
            rayOrigin = transform.position;
        }
        else
        {
            // Later points: cast from the last dash point
            rayOrigin = dashPoints[dashPoints.Count - 1];
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Direction from origin toward the ray direction
        Vector3 aimDirection = ray.direction.normalized;

        if (Physics.Raycast(rayOrigin, aimDirection, out RaycastHit hit, pointMaxDistance))
        {
            Vector3 dir = hit.point - rayOrigin;

            if (dir.magnitude < pointMaxDistance)
            {
                return hit.point + hit.normal; // Accept the exact clicked point
            }
            else
            {
                Vector3 clampedDir = dir.normalized * pointMaxDistance;
                return transform.position + clampedDir;
            }
        }
        else
        {
            Vector3 point = rayOrigin + ray.direction.normalized * pointMaxDistance;
            return point;
        }
    }


    void UpdateVisualIndicator()
    {
        List<Vector3> previewPoints = new List<Vector3> { transform.position };
        previewPoints.AddRange(dashPoints);

        lineRenderer.positionCount = previewPoints.Count;
        lineRenderer.SetPositions(previewPoints.ToArray());
    }

    void ClearVisualIndicator()
    {
        lineRenderer.positionCount = 0;
    }
}
