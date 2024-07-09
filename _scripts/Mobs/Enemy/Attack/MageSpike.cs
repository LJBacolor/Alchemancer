using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MageSpike : MonoBehaviour
{
    [SerializeField] private float spikeTimer = 2f;
    [SerializeField] private float spikeCooldown = 3f;
    [SerializeField] private MeshRenderer rd;
    [SerializeField] private GameObject warning;
    private float startpos;
    private GameObject player;
    private Rigidbody rb;
    private PlayerHealth playerHealth;

    public bool imPlant;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        rb = GetComponent<Rigidbody>();
        startpos = transform.position.y;
    }

    private void Start()
    {
        StartCoroutine(startSpawn());
    }

    private void Update()
    {
        if (transform.position.y >= startpos + 1.8f)
        {
            rb.velocity = Vector3.zero;
        }
    }

    IEnumerator startSpawn()
    {
        yield return new WaitForSeconds(spikeTimer);
        rd.enabled = false;
        rb.velocity = new Vector3(0,10,0);
        
        Destroy(warning, spikeCooldown);
    }
}