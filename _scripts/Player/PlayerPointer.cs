using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointer : MonoBehaviour
{
    [SerializeField] private GameObject pointer;
    [SerializeField] private Transform target;

    void Update()
    {
        if (target)
        {
            pointer.transform.LookAt(target);
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        pointer.SetActive(true);
    }

    public void ClearTarget()
    {
        target = null;
        pointer.SetActive(false);
    }
}
