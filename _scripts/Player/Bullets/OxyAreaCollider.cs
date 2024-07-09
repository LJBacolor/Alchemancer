using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxyAreaCollider : MonoBehaviour
{
    private float damage;
    private float reactionDmg;
    private bool critReact;
    private bool devolved;

    public void setDamage(float dmg, float reactDmg,bool critR,bool dev)
    {
        damage = dmg;
        reactionDmg = reactDmg;
        critReact = critR;
        devolved = dev;
    }

    private void OnTriggerEnter(Collider col) 
    {
        if (col.tag == "Obstacle")
        {
            Destroy(gameObject);
        }
        else if (col.tag == "Ground")
        {
            Destroy(gameObject);
        }
        else if (col.tag == "Acid")
        {
            Destroy(gameObject);
        }
        else if (col.tag == "Dummy")
        {
            DummyStatus dummyStatus = col.gameObject.GetComponent<DummyStatus>();
            dummyStatus.setReactionDmg(reactionDmg);
            DummyHealth dummyHealth = col.gameObject.GetComponent<DummyHealth>();
            dummyHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (col.tag == "Enemy")
        {
            EnemyStatus enemyStatus = col.gameObject.GetComponent<EnemyStatus>();
            enemyStatus.setReactionDmg(reactionDmg,critReact,devolved);
            EnemyHealth enemyHealth = col.gameObject.GetComponent<EnemyHealth>();
            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (col.gameObject.tag == "Worm")
        {
            EnemyHealth wormHealth = GameObject.Find("Worm").GetComponent<EnemyHealth>();
            wormHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (col.gameObject.tag == "Daisy")
        {
            EnemyHealth daisyHealth = GameObject.Find("Daisy").GetComponent<EnemyHealth>();
            daisyHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if(col.gameObject.tag == "Monitor")
        {
            MonitorHealth monitorHealth = GameObject.Find("Monitor").GetComponent<MonitorHealth>();
            monitorHealth.TakeDamage(damage, "O");
            Destroy(gameObject);
        }
    }
}
