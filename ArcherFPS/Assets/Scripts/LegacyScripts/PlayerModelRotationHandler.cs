using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelRotationHandler : MonoBehaviour
{
    [SerializeField] PlayerControllerV2 player;
    [SerializeField] GameObject playerObj;
    [SerializeField] float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        if (player.wishdir != Vector3.zero)
        {
            Quaternion desiredRot = Quaternion.LookRotation(player.wishdir);
            Quaternion rot = desiredRot;
            playerObj.transform.rotation = Quaternion.Slerp(playerObj.transform.rotation, rot, rotationSpeed * Time.deltaTime);
        }
    }
}
