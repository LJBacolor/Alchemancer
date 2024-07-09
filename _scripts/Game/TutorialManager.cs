using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.VisualScripting;
using System;

public class TutorialManager : NetworkBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private TutorialInfo[] tutorialInfo; 

    [SerializeField] private TextMeshProUGUI textCmp;
    [SerializeField] private float textSpeed;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject objectivesPanel;
    [SerializeField] private GameObject[] objectives;

    [SerializeField] private GameObject[] wasdTasks;
    [SerializeField] private GameObject[] dashTasks;
    [SerializeField] private GameObject[] attackTasks;
    [SerializeField] private GameObject[] reactionTasks;

    private int index;
    private bool isDialogActive;
    private NPCInteract npcInteract;
    private  bool onDialog = false;

    public int progress = 0;
    public bool canProgress = false;
    private bool progressCalled = false;

    private PlayerInput playerInput;
    private InputAction leftClickAction;

    private bool useW, useA, useS, useD;
    [NonSerialized] public bool useO, useH, useN, useC;
    public bool useCO2, useH2O, useNO, useNH3, useCH4, useCN;
    [NonSerialized] public int dashes = 0;
    private bool isMoved = false;

    private void Awake()
    {
        Instance = this;
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions.FindActionMap("Inputs").Enable();
        leftClickAction = playerInput.actions["Attack"];
    }

    private void Start()
    {
        if(PlayerProgress.tutorialFinished)
        {
            FinishTutorial();
        }
        else
        {
            textCmp.text = string.Empty;
            StartDialogue();
        }
    }

    private void Update()
    {
        if(!PlayerProgress.tutorialFinished && !isMoved)
        {
            isMoved = true;
            GameObject.Find("Player(Clone)").transform.position = GameObject.Find("Tutorial Spawn").transform.position;
        }

        if(PlayerProgress.tutorialFinished) NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().healthCanvas.SetActive(true);
        else NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().healthCanvas.SetActive(false);
        

        if (isDialogActive && leftClickAction.triggered)// && !gameManager.isPaused)
        {
            if (textCmp.text == tutorialInfo[progress].dialogues[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textCmp.text = tutorialInfo[progress].dialogues[index];
            }
        }

        ProgressTasks();

        if(canProgress && progress < tutorialInfo.Length - 1)
        {
            canProgress = false;
            progress++;
            StartDialogue();
        }
    }

    public void StartDialogue()
    {
        onDialog = true;
        objectivesPanel.SetActive(false);

        PlayerController.Instance.canMove = false;
        PlayerController.Instance.canDash = false;
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerAttack>().enabled = false;
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEquips>().enabled = false;
        NetworkManager.LocalClient.PlayerObject.GetComponent<IsoAim>().enabled = false;

        textCmp.text = string.Empty;
        index = 0;
        isDialogActive = true;
        StartCoroutine(TypeLine());
        dialoguePanel.SetActive(true);
    }

    private void NextLine()
    {
        if (index < tutorialInfo[progress].dialogues.Length - 1)
        {
            index++;
            textCmp.text = string.Empty;

            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
            EnablePlayer();
        }
    }

    public void EndDialogue()
    {
        onDialog = false;
        PlayerController.Instance.canMove = true;
        if(progress <= 4) objectivesPanel.SetActive(true);
        else objectivesPanel.SetActive(false);

        StopAllCoroutines();
        isDialogActive = false;
        
        dialoguePanel.SetActive(false);

        textCmp.text = string.Empty;
        index = 0;

        for(int i = 0; i < objectives.Length; i++)
        {
            if(i == progress)
            {
                objectives[i].SetActive(true);
            }
            else
            {
                objectives[i].SetActive(false);
            }
        }
    }

    private void ProgressTasks()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            FinishTutorial();
            NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().healthCanvas.SetActive(true);
            DataManager.Instance.SaveGame();
        }

        if(!onDialog)
        {
            switch(progress)
            {
                case 0:
                    if(PlayerController.Instance.input.y == 1) useW = true;
                    if(PlayerController.Instance.input.x == -1) useA = true;
                    if(PlayerController.Instance.input.y == -1) useS = true;
                    if(PlayerController.Instance.input.x == 1) useD = true;

                    if(useW) wasdTasks[0].SetActive(true);
                    if(useA) wasdTasks[1].SetActive(true);
                    if(useS) wasdTasks[2].SetActive(true);
                    if(useD) wasdTasks[3].SetActive(true);
                    
                    if(useW && useA && useS && useD && !progressCalled) 
                    {
                        StartCoroutine(DelayedProgress());
                        progressCalled = true;
                    }
                    break;

                case 1:
                    if(dashes == 1) dashTasks[0].SetActive(true);
                    if(dashes == 2) dashTasks[1].SetActive(true);
                    if(dashes == 3) dashTasks[2].SetActive(true);

                    if(dashes >= 3 && !progressCalled)
                    {
                        StartCoroutine(DelayedProgress());
                        progressCalled = true;
                    }
                    break;

                case 2:
                    PlayerEquips.Instance.canUseWeapons = true;

                    if(useO) attackTasks[0].SetActive(true);
                    if(useH) attackTasks[1].SetActive(true);
                    if(useN) attackTasks[2].SetActive(true);
                    if(useC) attackTasks[3].SetActive(true);

                    if(useO && useH && useN && useC && !progressCalled) 
                    {
                        StartCoroutine(DelayedProgress());
                        progressCalled = true;
                    }
                    break;

                case 3:
                    if(useCO2) reactionTasks[0].SetActive(true);
                    if(useH2O) reactionTasks[1].SetActive(true);
                    if(useNO) reactionTasks[2].SetActive(true);
                    if(useNH3) reactionTasks[3].SetActive(true);
                    if(useCH4) reactionTasks[4].SetActive(true);
                    if(useCN) reactionTasks[5].SetActive(true);

                    if(useCO2 && useH2O && useNO && useNH3 && useCH4 && useCN && !progressCalled) 
                    {
                        StartCoroutine(DelayedProgress());
                        progressCalled = true;
                    }
                    break;

                case 4:
                        canProgress = true;
                        PlayerProgress.tutorialFinished = true;
                        objectivesPanel.SetActive(false);
                        DataManager.Instance.SaveGame();
                    break;
                default:
                    
                    break;
            }
        }
        else
        {
            progressCalled = false;
        }
    }

    private IEnumerator DelayedProgress()
    {
        yield return new WaitForSeconds(1f);
        canProgress = true;
    }

    private IEnumerator TypeLine()
    {
        foreach (char c in tutorialInfo[progress].dialogues[index].ToCharArray())
        {
            textCmp.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    public void EnablePlayer()
    {
        PlayerController.Instance.canMove = true;
        PlayerController.Instance.canDash = true;
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerAttack>().enabled = true;
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEquips>().enabled = true;
        NetworkManager.LocalClient.PlayerObject.GetComponent<IsoAim>().enabled = true;
    }

    private void FinishTutorial()
    {
        PlayerProgress.tutorialFinished = true;
        PlayerEquips.Instance.canUseWeapons = true;
        progress = 5;
        canProgress = false;
        EndDialogue();
        EnablePlayer();
        this.enabled = false;
    }
}

[System.Serializable]
public class TutorialInfo
{
    public int progress;
    public string[] dialogues;
}
