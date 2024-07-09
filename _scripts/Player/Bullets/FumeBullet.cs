using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FumeBullet : NetworkBehaviour
{
    [SerializeField] public float damageCooldown = 0.2f;
    private float nextDamageTime;
    public float hydroDmg;
    public float reactDmg;
    public bool critReact;
    public bool devolved;

    private void Start() 
    {
        nextDamageTime = Time.time;
    }

    private void OnTriggerStay(Collider col) 
    {
        if (Time.time >= nextDamageTime) 
        {
            if(col.tag == "Obstacle")
            {
                // Do something for obstacles
            }
            else if(col.tag == "Dummy")
            {
                DummyStatus dummyStatus = col.gameObject.GetComponent<DummyStatus>();
                dummyStatus.setReactionDmg(reactDmg);
                DummyHealth dummyHealth = col.gameObject.GetComponent<DummyHealth>();
                dummyHealth.TakeDamage(hydroDmg);
                Debug.Log(col.name);
            }
            else if(col.tag == "Enemy")
            {
                EnemyStatus enemyStatus = col.gameObject.GetComponent<EnemyStatus>();
                enemyStatus.setReactionDmg(reactDmg,critReact,devolved);
                EnemyHealth enemyHealth = col.gameObject.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(hydroDmg);
            }
            else if(col.gameObject.tag == "Worm")
            {
                EnemyHealth wormHealth = GameObject.Find("Worm").GetComponent<EnemyHealth>();
                wormHealth.TakeDamage(hydroDmg);
            }
            else if(col.gameObject.tag == "Daisy")
            {
                EnemyHealth daisyHealth = GameObject.Find("Daisy").GetComponent<EnemyHealth>();
                daisyHealth.TakeDamage(hydroDmg);
            }
            else if (col.gameObject.tag == "Monitor")
            {
                MonitorHealth monitorHealth = GameObject.Find("Monitor").GetComponent<MonitorHealth>();
                monitorHealth.TakeDamage(hydroDmg,"H");
            }

            nextDamageTime = Time.time + damageCooldown; // Set the next allowed damage time
        }
    }
}
