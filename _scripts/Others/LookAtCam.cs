using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCam : MonoBehaviour
{
    public GameObject cam;

    private void Update()
    {
        if (cam)
        {
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
        }
        else
        {
            cam = GameObject.Find("Main Camera");
        }
    }
}
