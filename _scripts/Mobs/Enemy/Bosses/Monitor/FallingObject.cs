using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private LayerMask layer;
    [SerializeField] private GameObject Shadow;
    [SerializeField] private float scaleRate;
    [SerializeField] private float scaleLimit;

    private GameObject tempShadow;

    private void Start()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layer))
        {
            tempShadow = Instantiate(Shadow,hit.point + (Vector3.up * 0.1f),Quaternion.identity);
        }
    }

    private void Update()
    {
        if (tempShadow != null)
        {
            if(tempShadow.transform.localScale.z < scaleLimit)
            {
                Vector3 tempScale = tempShadow.transform.localScale;
                tempScale = new Vector3(tempScale.x + scaleRate,tempScale.y,tempScale.z + scaleRate);
                tempShadow.transform.localScale = tempScale;
            }
        }
    }

    private void OnTriggerEnter(Collider col) 
    {
        if(col.gameObject.tag == "Ground" || col.gameObject.tag == "Player")
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.fallingBreak, transform);
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(tempShadow);
            Destroy(gameObject);
        }
        
    }
}
