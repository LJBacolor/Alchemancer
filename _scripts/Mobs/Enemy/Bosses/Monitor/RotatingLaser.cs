using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingLaser : MonoBehaviour
{
    [SerializeField] private MonitorAttack monitorAttack;
    [SerializeField] private GameObject[] lasers;
    [SerializeField] private Vector3 rotateDirection;

    private void Awake()
    {
        monitorAttack = GameObject.Find("Monitor").GetComponent<MonitorAttack>();
    }

    private void Start()
    {
        Destroy(gameObject, 15);
    }

    private void Update()
    {
        RotateLaser();
    }

    private void RotateLaser()
    {
        transform.Rotate(rotateDirection * monitorAttack.rotateSpeed * Time.deltaTime);
    }
}
