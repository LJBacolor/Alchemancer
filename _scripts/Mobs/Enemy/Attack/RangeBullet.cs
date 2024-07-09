using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeBullet : MonoBehaviour
{
    public bool imInsect;
    public bool imRobot;

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
            // DummyHealth dummyHealth = col.gameObject.GetComponent<DummyHealth>();
            // dummyHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
