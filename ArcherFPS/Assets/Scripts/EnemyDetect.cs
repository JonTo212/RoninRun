using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetect : MonoBehaviour
{
    public Transform player;
    public float detectionRadius;
    public LayerMask obstacleMask;
    float squaredRadius;
    public float detectionDelay;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        squaredRadius = detectionRadius * detectionRadius;
    }

    // Update is called once per frame
    void Update()
    {
        if(Detect())
        {
            print("yes");
        }
    }

    bool Detect()
    {
        if ((player.position - transform.position).sqrMagnitude <= squaredRadius)
        {
            Vector3 toOther = player.position - transform.position;

            if (Vector3.Dot(transform.forward, toOther.normalized) > Mathf.Cos(45 * Mathf.Deg2Rad))
            {
                if (!Physics.Raycast(transform.position, toOther.normalized, toOther.magnitude, obstacleMask))
                {
                    Debug.DrawRay(transform.position, toOther, Color.red);
                    return true;
                }
            }
        }
        return false;
    }
}
