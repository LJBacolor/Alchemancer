using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    [SerializeField] private GameObject areaCollider;
    [SerializeField] private GameObject areaHit;

    private void OnTriggerEnter(Collider col) 
    {
        if(col.gameObject.tag == "WarningArea")
        {
            Destroy(col.gameObject);
        }
        if(col.gameObject.tag == "Ground")
        {
            GameObject aHit = Instantiate(areaHit, transform.position, Quaternion.identity);
            Destroy(aHit, 2);

            Instantiate(areaCollider, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        else if(col.gameObject.tag == "Player")
        {
            GameObject aHit = Instantiate(areaHit, transform.position, Quaternion.identity);
            Destroy(aHit, 2);

            Instantiate(areaCollider, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
