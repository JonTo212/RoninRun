using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RazeController : MonoBehaviour
{
    public GameObject blastPackPrefab;
    public Camera cam;
    public Transform spawnPoint;
    public bool hasSpawned;
    public float throwForce = 5f;
    public float throwUpwardForce = 5f;
    PlayerController player;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && !hasSpawned)
        {
            GameObject blastPack = Instantiate(blastPackPrefab, spawnPoint.position, spawnPoint.rotation);

            Rigidbody rb = blastPack.GetComponent<Rigidbody>();

            Vector3 forceDirection = cam.transform.forward;

            Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;  //+ player.playerVelocity;

            rb.AddForce(forceToAdd, ForceMode.Impulse);

            hasSpawned = true;
        }
    }
}
