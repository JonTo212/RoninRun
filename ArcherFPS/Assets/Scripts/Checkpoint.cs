using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Respawn respawn;

    private void OnTriggerEnter(Collider other)
    {
        respawn.spawnPoint = this.transform;
    }
}
