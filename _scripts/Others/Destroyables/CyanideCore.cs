using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyanideCore : MonoBehaviour
{
    [SerializeField] private GameObject CN;
    
    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Hydrogen")
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.CN, transform);
            Instantiate(CN, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
