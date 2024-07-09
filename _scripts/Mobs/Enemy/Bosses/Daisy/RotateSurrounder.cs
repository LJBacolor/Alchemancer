using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSurounder : MonoBehaviour
{
    private Transform ToRotate;
    private bool RotationStarted = false;
    public float Speed = 50.0f;
    public Vector3 Axis;

    private void Start()
    {
       //StartRotation(transform);
    }

    public void StartRotation(Transform TargetT)
    {
        ToRotate = TargetT;
        RotationStarted = true;
    }
    
    void Update()
    {
        if (RotationStarted)
        {
            //ToRotate.Rotate( ToRotate.up,Speed * Time.deltaTime);
            ToRotate.Rotate( Axis,Speed*Time.deltaTime);
        }
    }
}
