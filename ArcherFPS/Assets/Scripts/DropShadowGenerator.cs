using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropShadowGenerator : MonoBehaviour
{
    [SerializeField] GameObject shadow;
    [SerializeField] LayerMask groundLayer;
    Quaternion originalRot;

    // Start is called before the first frame update
    void Start()
    {
        originalRot = Quaternion.Euler(90, 0, 45);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            shadow.transform.position = hit.point + (Vector3.up * 0.01f);
            shadow.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * originalRot;
        }
    }
}