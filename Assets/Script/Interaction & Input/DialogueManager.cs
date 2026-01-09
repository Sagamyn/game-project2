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
    bool inputLocked; // 🔒 prevents same-frame re-trigger

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
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        dialogueUI.SetActive(false);
        continueIcon.SetActive(false);
        playerInteraction = FindObjectOfType<PlayerInteraction>();
    }

    void Update()
    {
        if (!isOpen || inputLocked) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                NextPage();
            }
        }
    }

    // ===============================
    // PUBLIC API
    // ===============================

    public void ShowDialogue(List<string> dialoguePages)
    {
        if (isOpen) return;

        pages = dialoguePages;
        currentPage = 0;

        isOpen = true;
        inputLocked = true;

        dialogueUI.SetActive(true);
        continueIcon.SetActive(false);

        PlayerMovement.Instance?.LockMovement(true);

        if (playerInteraction != null)
            playerInteraction.enabled = false;

        StartCoroutine(UnlockInputNextFrame());
        ShowPage();
    }

    public void ShowDialogue(string text)
{
    ShowDialogue(new List<string> { text });
}

    // ===============================
    // INTERNAL LOGIC
    // ===============================

    void ShowPage()
    {
        dialogueText.text = pages[currentPage];
        dialogueText.maxVisibleCharacters = 0;
        continueIcon.SetActive(false);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

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

    void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
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
        inputLocked = true;

        dialogueUI.SetActive(false);
        continueIcon.SetActive(false);
        
        if (playerInteraction != null)
            playerInteraction.ResetInteraction();

        PlayerMovement.Instance?.LockMovement(false);

        StartCoroutine(ReEnableInteractionNextFrame());
    }

    // ===============================
    // INPUT SAFETY
    // ===============================

    IEnumerator UnlockInputNextFrame()
    {
        yield return null; // wait 1 frame
        inputLocked = false;
    }

    IEnumerator ReEnableInteractionNextFrame()
    {
        yield return null; // wait 1 frame

        if (playerInteraction != null)
            playerInteraction.enabled = true;

        inputLocked = false;
    }
}
