using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles : MonoBehaviour
{

    [SerializeField] private Material[] mats  = new Material[3];
    void Start()
    {
        int random = Random.Range(1, 10);

        if (random < 8)
        {
            GetComponent<MeshRenderer>().material = mats[0];
        }
        else if (random < 9) 
        {
            GetComponent<MeshRenderer>().material = mats[1];
        }
        else
        {
            GetComponent<MeshRenderer>().material = mats[2];
        }
    }
}
