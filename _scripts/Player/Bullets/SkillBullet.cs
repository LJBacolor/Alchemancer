using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBullet : MonoBehaviour
{
    [SerializeField] private GameObject oxygenHit;
    [SerializeField] private GameObject oxyHealArea;
    private float damage;
    private GameObject hitVFX;

    private void OnTriggerEnter(Collider col) 
    {
        if(col.tag == "Obstacle")
        {
            if(gameObject.tag == "OxySkill")
            {
                Destroy(gameObject);
            }
        }
        else if(col.tag == "Ground")
        {
            CheckOxy();
            Destroy(gameObject);
        }
        else if(col.tag == "Dummy")
        {
            CheckOthers();
            DummyHealth dummyHealth = col.gameObject.GetComponent<DummyHealth>();
            dummyHealth.TakeDamage(damage);
        }
        else if(col.tag == "Enemy")
        {
            CheckOthers();
            EnemyHealth enemyHealth = col.gameObject.GetComponent<EnemyHealth>();
            enemyHealth.TakeDamage(damage);
        }
    }

    private void CheckOxy()
    {
        if(gameObject.tag == "OxySkill")
        {
            damage = 0f;
            hitVFX = oxygenHit;

            GameObject hit = Instantiate(hitVFX, transform.position, Quaternion.identity);
            Destroy(hit, 2f);
            GameObject areaDamage = GameObject.Find("Oxy Damage Area(Clone)");
            Destroy(areaDamage);
            Instantiate(oxyHealArea, transform.position, Quaternion.identity);
        }
    }

    private void CheckOthers()
    {
        if(gameObject.tag == "Hydrogen")
        {
            damage = (PlayerStats.hydrogenDamage * 5) + PlayerStats.extraSkillDamage;
        }
        else if(gameObject.tag == "Nitrogen")
        {
            damage = (PlayerStats.nitrogenDamage * 3) + PlayerStats.extraSkillDamage;
        }
        else if(gameObject.tag == "Carbon")
        {
            damage = (PlayerStats.carbonDamage * 2) + (PlayerStats.extraSkillDamage / 5);
        }
    }
}
