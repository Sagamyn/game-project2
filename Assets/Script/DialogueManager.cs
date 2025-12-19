using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialogueUI;
    public TextMeshProUGUI dialogueText;
    public GameObject continueIcon;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    bool isOpen;
    bool isTyping;

    List<string> pages;
    int currentPage;

    Coroutine typingCoroutine;
    PlayerInteraction playerInteraction;

    public bool IsOpen => isOpen;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        dialogueUI.SetActive(false);
        continueIcon.SetActive(false);
        playerInteraction = FindObjectOfType<PlayerInteraction>();
    }

    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.maxVisibleCharacters = dialogueText.text.Length;
                isTyping = false;
                continueIcon.SetActive(true);
            }
            else
            {
                NextPage();
            }
        }
    }

    public void ShowDialogue(List<string> dialoguePages)
    {
        if (isOpen) return;

        pages = dialoguePages;
        currentPage = 0;

        isOpen = true;
        dialogueUI.SetActive(true);
        continueIcon.SetActive(false);

        PlayerMovement.Instance?.LockMovement(true);
        if (playerInteraction != null)
            playerInteraction.enabled = false;

        ShowPage();
    }

    void ShowPage()
    {
        dialogueText.text = pages[currentPage];
        dialogueText.maxVisibleCharacters = 0;
        continueIcon.SetActive(false);

        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;

        while (dialogueText.maxVisibleCharacters < dialogueText.text.Length)
        {
            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        continueIcon.SetActive(true);
    }

    void NextPage()
    {
        currentPage++;

        if (currentPage >= pages.Count)
        {
            CloseDialogue();
            return;
        }

        ShowPage();
    }

    void CloseDialogue()
    {
        isOpen = false;
        dialogueUI.SetActive(false);
        continueIcon.SetActive(false);

        PlayerMovement.Instance?.LockMovement(false);
        if (playerInteraction != null)
            playerInteraction.enabled = true;
    }
}
