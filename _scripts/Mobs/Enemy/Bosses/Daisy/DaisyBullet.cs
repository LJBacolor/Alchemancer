
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DaisyBullet : NetworkBehaviour
{
    public bool isInstant = true;

    private void Start()
    {
        if (isInstant)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.AddForce(gameObject.transform.forward * 5, ForceMode.Impulse);
        }
    }

    public void Shoot(Vector3 target)
    {
        StartCoroutine(Fire(target));
    }

    IEnumerator Fire(Vector3 target)
    {
        yield return new WaitForSeconds(3f);

        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 directionToPlayer = (target - transform.position).normalized;
        rb.AddForce(directionToPlayer * 15, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider col) 
    {
        if(col.gameObject.tag == "Ground")
        {
            Destroy(gameObject);
        }
        else if(col.gameObject.tag == "Obstacle")
        {
            Destroy(gameObject);
        }
        else if(col.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
