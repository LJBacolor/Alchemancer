using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarbonSurroundSkill : MonoBehaviour
{
    public static CarbonSurroundSkill Instance;
    [SerializeField] private GameObject surrounderObjPrefab;
    [SerializeField] private float AppearWaitDuration = 0.3f;
    private int surrounderObjCount;

    private GameObject followGameObject;
    private bool canAttack = true;

    private void Awake()
    {
        Instance = this;
        followGameObject = GameObject.Find("Player");
    }

    void Start()
    {
        surrounderObjCount = PlayerStats.carbonSkillCount;
        StartCoroutine(Surround());
    }

    void Update()
    {
        if(followGameObject != null)
        {
            transform.position = followGameObject.transform.position;
        }

        if(!PlayerInventory.usingCarbonSkill)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Surround()
    {
        yield return new WaitForSeconds(AppearWaitDuration);

        float AngleStep = 360.0f / surrounderObjCount;

        for (int i = 0; i < surrounderObjCount; i++)
        {
            GameObject newSurrounderObject = Instantiate(surrounderObjPrefab, transform);
            
            newSurrounderObject.transform.RotateAround(transform.position,Vector3.up,AngleStep * i);

            yield return new WaitForSeconds(AppearWaitDuration);
        }
        GetComponent<RotateSurounder>().StartRotation(transform);
    }
}
