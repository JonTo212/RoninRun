using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    Transform followLoc;
    Camera currentCam;
    // Start is called before the first frame update
    void Start()
    {
        currentCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCam == null)
        {
            currentCam = Camera.current;
        }

        currentCam.transform.position = followLoc.position;
    }
}
