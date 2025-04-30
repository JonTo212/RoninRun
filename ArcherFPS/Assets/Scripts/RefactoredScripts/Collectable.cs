using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] int typeIndex;
    [SerializeField] int value;

    private void Update()
    {
        transform.Rotate(1, 1, 1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<ShurikenManager>().AddShuriken(typeIndex, value);
            Destroy(gameObject);
        }
    }
}
