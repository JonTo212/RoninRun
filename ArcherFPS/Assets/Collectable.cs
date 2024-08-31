using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] int typeIndex;
    [SerializeField] int value;

    private void Update()
    {
        transform.Rotate(0, 1, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Throw>().starCount[typeIndex] += value;
            Destroy(gameObject);
        }
    }
}
