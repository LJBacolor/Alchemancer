using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaCollider : MonoBehaviour
{
    public bool imPlant;

    private void OnTriggerEnter(Collider col) 
    {
        if(col.tag == "Obstacle")
        {
            Destroy(gameObject);
        }
        else if(col.tag == "Ground")
        {
            Destroy(gameObject);
        }
        else if(col.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
