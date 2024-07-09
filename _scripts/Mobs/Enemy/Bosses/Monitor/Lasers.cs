using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lasers : MonoBehaviour
{
    private int collisionCount = 0;

    void Update()
    {
        if(collisionCount > 0)
        {
            transform.localScale -= new Vector3(0, 0, 0.1f);
            //Debug.Log("Hitting");
        }
        else
        {
            transform.localScale += new Vector3(0, 0, 0.1f);
            //Debug.Log("Not Hitting");
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Obstacle")
        {
            collisionCount++;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if(col.gameObject.tag == "Obstacle")
        {
            collisionCount--;
            if (collisionCount < 0)
            {
                collisionCount = 0;
            }
        }
    }
}
