using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [SerializeField]
    Arrow arrowPrefab;

    [SerializeField]
    Transform arrowSpawnPoint;

    [SerializeField]
    float delay;
    Arrow currentArrow;

    public void Fire(float firePower)
    {

    }
}
