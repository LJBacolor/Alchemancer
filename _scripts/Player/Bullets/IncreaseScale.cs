using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseScale : MonoBehaviour
{
    // Speed at which the scale increases per second
    public float increaseSpeed = 0.1f;

    void Update()
    {
        // Calculate the new scale by increasing the x component
        float newScaleX = transform.localScale.x + increaseSpeed * Time.deltaTime;

        // Apply the new scale
        transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);
    }
}
