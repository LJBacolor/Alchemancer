using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    [SerializeField] private float destroyTime = 0f;
    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
