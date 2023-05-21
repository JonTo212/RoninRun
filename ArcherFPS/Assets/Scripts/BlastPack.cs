using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastPack : MonoBehaviour
{
    public float radius = 3f;
    public float blastPower = 10f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ForcePush();
            GameObject.Find("Player").GetComponent<RazeController>().hasSpawned = false;
            Destroy(this.gameObject);
        }
    }

    void ForcePush()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hitColliders)
        {
            if (hit.TryGetComponent(out PlayerController playerController))
            {
                playerController.ApplyForce(blastPower, playerController.transform.position - transform.position);
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
