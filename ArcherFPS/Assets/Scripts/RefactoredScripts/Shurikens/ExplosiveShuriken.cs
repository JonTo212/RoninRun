using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ExplosiveShuriken : ShurikenBaseClass
{
    private static Queue<ExplosiveShuriken> placementQueue = new Queue<ExplosiveShuriken>();

    [SerializeField] private float minBlastForce;
    [SerializeField] private float maxBlastForce;
    [SerializeField] private float blastRadius;
    [SerializeField] private float activationDelay;
    [SerializeField] private float minDistance;
    [SerializeField] private ParticleSystem smokePuff;
    private float blastForce;

    private bool isFirstInQueue;

    protected override void Awake()
    {
        placementQueue.Enqueue(this);
        base.Awake();
        UpdateQueueStatus();
    }

    protected override void Update()
    {
        base.Update();

        // Only the oldest shuriken (head of queue) listens for input
        if (isFirstInQueue && Input.GetMouseButtonDown(1))
        {
            placementQueue.Dequeue();
            StartCoroutine(DetonationTimer());
        }
    }

    private IEnumerator DetonationTimer()
    {
        yield return new WaitForSeconds(activationDelay);
        ApplyExplosiveForce();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Clean up in case this was destroyed prematurely
        if (placementQueue.Contains(this))
        {
            Queue<ExplosiveShuriken> temp = new();
            while (placementQueue.Count > 0)
            {
                ExplosiveShuriken s = placementQueue.Dequeue();
                if (s != this)
                    temp.Enqueue(s);
            }
            placementQueue = temp;
        }

        // Update next shuriken’s input check
        UpdateQueueStatus();
    }

    private static void UpdateQueueStatus()
    {
        foreach (var s in placementQueue)
            s.isFirstInQueue = false;

        if (placementQueue.Count > 0)
            placementQueue.Peek().isFirstInQueue = true;
    }

    private void ApplyExplosiveForce()
    {
        bool hitPlayer = false;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, blastRadius);

        foreach (Collider col in hitColliders)
        {
            if (!col.TryGetComponent(out PlayerControllerV2 playerController)) continue;

            hitPlayer = true;
            Vector3 dist = playerController.transform.position - transform.position;
            Vector3 dir = dist.normalized;
            float distance = Mathf.Clamp(dist.magnitude, minDistance, blastRadius);
            float t = Mathf.InverseLerp(blastRadius, minDistance, distance);
            blastForce = Mathf.Lerp(minBlastForce, maxBlastForce, t);

            /*if (hit)
                dir = hitNormal;
            else
                dir = dist.normalized;*/

            if (playerController.playerVelocity.y < 0)
            {
                playerController.playerVelocity.y = 0;
            }

            float scalar = playerController.playerVelocity.magnitude;
            HandleParticles(playerController.transform, true, scalar);

            playerController.playerVelocity += blastForce * dir;
            playerController.blasted = true;
        }

        if(!hitPlayer)
        {
            HandleParticles(transform, false);
        }
    }

    private void HandleParticles(Transform spawnPos, bool playerSpawn = true, float scalar = 0)
    {
        ParticleSystem smoke;
        float multiplier;

        if (!playerSpawn)
        {
            smoke = Instantiate(smokePuff, spawnPos.position, Quaternion.identity);
            multiplier = 0.75f;
        }
        else
        {
            smoke = Instantiate(smokePuff, spawnPos.position + spawnPos.forward * scalar, Quaternion.identity);
            multiplier = 0.5f;
        }

        smoke = Instantiate(smokePuff, spawnPos.position, Quaternion.identity);
        var radius = smoke.shape.radius;
        float scale = blastRadius / radius;
        smoke.transform.localScale = Vector3.one * scale * multiplier;
        smoke.Play();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
