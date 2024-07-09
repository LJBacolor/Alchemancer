using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DaisyAreaBullet : NetworkBehaviour
{
    [SerializeField] private GameObject areaCollider;
    [SerializeField] private GameObject areaHit;

    public void Shoot(Vector3 target)
    {
        StartCoroutine(Fire(target));
    }

    IEnumerator Fire(Vector3 target)
    {
        yield return new WaitForSeconds(3f);
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 directionToPlayer = (target - transform.position).normalized;
        rb.AddForce(directionToPlayer * 20, ForceMode.Impulse);
    }

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
