using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class NPCDialogue : NetworkBehaviour
{
    [SerializeField] private NPCInfo[] npcInfo; 

    [SerializeField] private TextMeshProUGUI textCmp;
    [SerializeField] private float textSpeed;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject yesNoPanel;
    [SerializeField] private GameObject tapAnywherePanel;
    [SerializeField] private string printWhatToOpen;

    private GameObject player;
    private int index;
    private bool isDialogActive;
    private NPCInteract npcInteract;
    private GameObject gameMan;

    //private GameManager gameManager;

    private int progress = 0;
    private bool canProgress = true;
    private bool inSelection = false;

    private PlayerInput playerInput;
    private InputAction leftClickAction;

    public bool imShop;
    public bool imSetting;
    public bool imCoop;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        npcInteract = GetComponent<NPCInteract>();
        gameMan = GameObject.Find("Game Manager");
        //gameManager = gameMan.GetComponent<GameManager>();

        playerInput = GetComponent<PlayerInput>();
        playerInput.actions.FindActionMap("Inputs").Enable();
        leftClickAction = playerInput.actions["Attack"];
    }

    private void Start()
    {
        textCmp.text = string.Empty;
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (isDialogActive && leftClickAction.triggered)// && !gameManager.isPaused)
        {
            if (textCmp.text == npcInfo[progress].dialogues[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textCmp.text = npcInfo[progress].dialogues[index];
            }
        }

        if(index >= npcInfo[progress].dialogues.Length - 1 && 
            textCmp.text == npcInfo[progress].dialogues[index])
        {
            inSelection = true;
        }

        if(inSelection)
        {
            yesNoPanel.SetActive(true);
            tapAnywherePanel.SetActive(false);
        }
        else
        {
            yesNoPanel.SetActive(false);
            tapAnywherePanel.SetActive(true);
        }
    }

    public void StartDialogue()
    {
        textCmp.text = string.Empty;
        index = 0;
        isDialogActive = true;
        StartCoroutine(TypeLine());
        dialoguePanel.SetActive(true);
    }

    private void NextLine()
    {
        if (index < npcInfo[progress].dialogues.Length - 1)
        {
            index++;
            textCmp.text = string.Empty;

            StartCoroutine(TypeLine());
        }
        else
        {
            //EndDialogue();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogActive = false;
        npcInteract.isInteracting = false;
        GameManager.Instance.isInteracted = false;
        
        dialoguePanel.SetActive(false);

        textCmp.text = string.Empty;
        index = 0;
        inSelection = false;

        if(progress < npcInfo.Length - 1 && canProgress)
        {
            progress++;
        }
    }

    private IEnumerator TypeLine()
    {
        foreach (char c in npcInfo[progress].dialogues[index].ToCharArray())
        {
            textCmp.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    public void YesBtn()
    {
        if(imShop)
        {
            dialoguePanel.SetActive(false);

            ShopManager shopManager = GetComponent<ShopManager>();
            shopManager.OpenShop();
        }
        else if(imSetting)
        {
            dialoguePanel.SetActive(false);

            SettingsManager settingsManager = GetComponent<SettingsManager>();
            settingsManager.OpenSettings();
        }
        else if(imCoop)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void NoBtn()
    {
        EndDialogue();
        EnablePlayer();
    }

    public void EnablePlayer()
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().canMove = true;
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().canDash = true;
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerHealth>().healthCanvas.SetActive(true);
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerAttack>().enabled = true;
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerEquips>().enabled = true;
        //NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>().enabled = true;
        NetworkManager.LocalClient.PlayerObject.GetComponent<IsoAim>().enabled = true;
    }
}

[System.Serializable]
public class NPCInfo
{
    public int progress;
    public string[] dialogues;
}