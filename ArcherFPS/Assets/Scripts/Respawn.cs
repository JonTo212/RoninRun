using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public Transform spawnPoint; //Add empty gameobject as spawnPoint
    public GameObject player; //Add your player
    public CharacterController characterController;
    public PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        characterController.enabled = false;
        player.transform.position = spawnPoint.position;
        playerController.rotY = spawnPoint.rotation.eulerAngles.y;
        playerController.rotX = 0;
        characterController.enabled = true;
        playerController.playerVelocity = Vector3.zero;
        playerController.useGravity = true;
        player.transform.SetParent(null);
    }
}
