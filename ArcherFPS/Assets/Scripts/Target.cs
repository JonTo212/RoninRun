using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float value;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Arrow")
        {
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<ScoreManager>().totalScore += value;
            Destroy(this.gameObject);
        }
    }
}
