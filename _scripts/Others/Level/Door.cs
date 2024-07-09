using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    private Animator animator;
    private Scene currentScene;

    public bool canEnter;

    void Awake()
    {
        animator = GetComponent<Animator>();
        currentScene = SceneManager.GetActiveScene();
    }

    private void Start()
    {
        if(currentScene.name == "Lobby")
        {
            canEnter = true;
        }
        else
        {
            canEnter = false;
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if(PlayerProgress.tutorialFinished && canEnter && coll.gameObject.tag == "Player")
        {
            SFXManager.Instance.PlaySFXClip(SFXManager.Instance.doorOpen, transform);
            animator.SetBool("isOpen", true);
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        SFXManager.Instance.PlaySFXClip(SFXManager.Instance.doorClose, transform);
        animator.SetBool("isOpen", false);
    }
}
