using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MerchantSpeechBubble : MonoBehaviour
{
    [Header("References")]
    public Animator bubbleAnimator;
    public TextMeshProUGUI dialogueText;

    [Header("Typewriter")]
    public float typingSpeed = 0.04f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] talkSounds;
    public float talkSoundInterval = 0.05f;

    // Animation clip lengths
    private float popUpDuration = 0.25f;  // 6 frames at 24fps
    private float closeDuration = 0.25f;

    private bool isVisible = false;
    private Coroutine typewriterCoroutine;
    private float talkSoundTimer;
    private Button bubbleButton;

    void Awake()
    {
        bubbleButton = GetComponent<Button>();

        // Disable animator so it doesnt play on start
        if (bubbleAnimator != null)
            bubbleAnimator.enabled = false;

        // Hide image
        Image img = GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }

        // Hide text
        if (dialogueText != null)
            dialogueText.gameObject.SetActive(false);
    }

    public IEnumerator ShowAndType(string text)
    {
        // Nonaktifkan tombol agar tidak bisa diklik saat sedang ngetik (tanpa meredupkan warna)
        if (bubbleButton != null) bubbleButton.enabled = false;

        // First time showing — play pop up animation
        if (!isVisible)
        {
            isVisible = true;

            // Show image
            Image img = GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 1f;
                img.color = c;
            }

            // Show text object but keep it empty
            if (dialogueText != null)
            {
                dialogueText.gameObject.SetActive(true);
                dialogueText.text = "";
            }

            // Enable animator — it will auto play PopUp from Entry
            if (bubbleAnimator != null)
                bubbleAnimator.enabled = true;

            // Wait for pop up animation to finish
            yield return new WaitForSeconds(popUpDuration);
        }
        else
        {
            // Already visible — just clear text for next line
            if (dialogueText != null)
                dialogueText.text = "";
        }

        // Stop any existing typewriter
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        // Type out the text
        yield return StartCoroutine(TypeText(text));
    }

    public IEnumerator Hide()
    {
        if (!isVisible) yield break;

        // Trigger close animation
        if (bubbleAnimator != null)
            bubbleAnimator.SetTrigger("Close");

        // Wait for close animation to finish
        yield return new WaitForSeconds(closeDuration);

        isVisible = false;

        // Hide image
        Image img = GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }

        // Hide text
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.gameObject.SetActive(false);
        }

        // Disable animator so it stops on last frame
        if (bubbleAnimator != null)
            bubbleAnimator.enabled = false;
    }

    IEnumerator TypeText(string text)
    {
        if (dialogueText == null) yield break;

        dialogueText.text = "";
        talkSoundTimer = 0f;

        foreach (char c in text)
        {
            dialogueText.text += c;

            // Play sound on non-space characters
            if (c != ' ')
            {
                talkSoundTimer += typingSpeed;

                if (talkSoundTimer >= talkSoundInterval)
                {
                    talkSoundTimer = 0f;
                    PlayTalkSound();
                }
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        typewriterCoroutine = null;

        // Ketikan selesai, aktifkan tombol lagi jika player ingin klik untuk lanjut
        if (bubbleButton != null) bubbleButton.enabled = true;
    }

    void PlayTalkSound()
    {
        if (audioSource == null || 
            talkSounds == null || 
            talkSounds.Length == 0) 
            return;

        AudioClip clip = talkSounds[Random.Range(0, talkSounds.Length)];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clip);
    }
}