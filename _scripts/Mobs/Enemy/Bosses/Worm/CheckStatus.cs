using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckStatus : MonoBehaviour
{
    [SerializeField] private GameObject digVfx;
    private WormStatus wormStatus;
    private EnemyHealth enemyHealth;
    private float cooldownTimer;

    public bool hasOxygen = false;
    public bool hasHydrogen = false;
    public bool hasNitrogen = false;
    public bool hasCarbon = false;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        wormStatus = GameObject.Find("Worm").GetComponent<WormStatus>();
    }

    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider col) 
    {
        wormStatus.ApplyElementServerRpc(col.tag);

        if(col.gameObject.tag == "Ground")
        {
            Instantiate(digVfx, transform.position, Quaternion.identity);
        }
    }
}