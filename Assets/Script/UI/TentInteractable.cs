using UnityEngine;
using System.Collections;


public class TentInteractable : MonoBehaviour
{
    [Header("References")]
    public EKeyPrompt eKeyPrompt;
    public SleepConfirmUI sleepConfirmUI;
    public DayNightManager dayNightManager;
    public TeleportFadeManager fadeManager;

    [Header("Sleep Settings")]
    public float fadeSpeed    = 2f;
    public float sleepPause   = 1.5f;   // how long screen stays black

    [Header("Earliest Sleep Time")]
    public float earliestSleepHour = 18f;   // 6 PM
    public string tooEarlyMessage  = "It's too early to sleep...";
    public string notReadyMessage  = "Finish today's work first before resting!";

    [Header("Trigger")]
    public float interactRadius = 1.5f;

    private bool playerNear    = false;
    private bool isSleeping    = false;
    private bool isConfirmOpen = false;

    void Update()
    {
        if (!playerNear || isSleeping) return;

        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();
    }

    void TryInteract()
    {
        // Gate: wave must be finished and merchant shop closed
        if (!FoodStallWaveManager.CanSleep)
        {
            DialogueManager.Instance?.ShowDialogue(notReadyMessage);
            return;
        }

        // Check if too early
        if (dayNightManager != null &&
            dayNightManager.currentTime < earliestSleepHour)
        {
            DialogueManager.Instance?.ShowDialogue(tooEarlyMessage);
            return;
        }

        // Hide the E key prompt while confirm dialog is open
        eKeyPrompt?.Hide();
        isConfirmOpen = true;

        int currentDay = dayNightManager != null ? dayNightManager.currentDay : 1;

        // Show Yes/No panel biasa
        sleepConfirmUI?.Show(
            yesCallback: OnConfirmSleep,
            noCallback:  OnCancelSleep
        );
    }

    void OnConfirmSleep()
    {
        isConfirmOpen = false;
        StartCoroutine(SleepSequence());
    }

    void OnCancelSleep()
    {
        isConfirmOpen = false;

        // Restore E key prompt if player is still nearby
        if (playerNear)
            eKeyPrompt?.Show();

        Debug.Log("Player cancelled sleep.");
    }

    IEnumerator SleepSequence()
    {
        isSleeping = true;

        // Hide E key prompt
        eKeyPrompt?.Hide();

        // Lock player
        PlayerMovement.Instance?.LockMovement(true);

        // Fade to black
        if (fadeManager != null)
            yield return StartCoroutine(
                fadeManager.FadeOut(fadeSpeed, Color.black));
        else
            yield return new WaitForSeconds(0.5f);

        // Advance day
        if (dayNightManager != null)
            dayNightManager.PlayerGoesToSleep();

        // Notify the wave manager: unlocks stall + resets day flags
        FoodStallWaveManager.Instance?.OnPlayerWokeUp();

        // Hold black screen for a moment
        yield return new WaitForSeconds(sleepPause);

        // Fade back in
        if (fadeManager != null)
            yield return StartCoroutine(fadeManager.FadeIn(fadeSpeed));

        // Unlock player
        PlayerMovement.Instance?.LockMovement(false);

        isSleeping = false;

        Debug.Log("Player woke up — new day started!");
    }

    // =========================================
    // TRIGGER DETECTION
    // =========================================

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = true;

        // Only show E prompt once the wave + merchant shop are done
        if (FoodStallWaveManager.CanSleep)
            eKeyPrompt?.Show();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        eKeyPrompt?.Hide();

        // Only hide confirm panel if it's not currently open
        // (locking movement can push player out of trigger while panel is showing)
        if (!isConfirmOpen)
            sleepConfirmUI?.Hide();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}