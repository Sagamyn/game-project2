using UnityEngine;

/// <summary>
/// Bed that player can interact with to sleep
/// Advances to next day when used
/// </summary>
public class Bed : MonoBehaviour
{
    [Header("Bed Settings")]
    public string bedName = "Bed";
    
    [Header("Sleep Requirements")]
    [Tooltip("Can only sleep after this time")]
    public float earliestSleepTime = 20f; // 8 PM
    
    [Tooltip("Messages")]
    public string tooEarlyMessage = "It's too early to sleep...";
    public string sleepPrompt = "Press E to sleep";
    
    [Header("Fade Settings")]
    public bool useFadeTransition = true;
    public float fadeSpeed = 2f;
    
    private DayNightManager dayNightManager;
    private TeleportFadeManager fadeManager;
    
    void Start()
    {
        dayNightManager = FindObjectOfType<DayNightManager>();
        fadeManager = FindObjectOfType<TeleportFadeManager>();
        
        if (dayNightManager == null)
        {
            Debug.LogError("Bed needs DayNightManager in scene!");
        }
    }

    public string GetInteractionPrompt()
    {
        if (dayNightManager == null) return "Bed (No time manager)";
        
        if (CanSleep())
        {
            return sleepPrompt;
        }
        else
        {
            return tooEarlyMessage;
        }
    }

    public void Interact(GameObject player)
    {
        if (!CanSleep())
        {
            Debug.Log(tooEarlyMessage);
            
            // Optional: Show UI message
            DialogueManager dialogue = FindObjectOfType<DialogueManager>();
            if (dialogue != null)
            {
                dialogue.ShowDialogue(tooEarlyMessage);
            }
            
            return;
        }
        
        // Go to sleep!
        StartCoroutine(SleepSequence(player));
    }

    bool CanSleep()
    {
        if (dayNightManager == null) return false;
        
        return dayNightManager.currentTime >= earliestSleepTime;
    }

    System.Collections.IEnumerator SleepSequence(GameObject player)
    {
        Debug.Log(" Going to sleep...");
        
        // Lock player movement
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
        
        // Fade out
        if (useFadeTransition && fadeManager != null)
        {
            yield return StartCoroutine(fadeManager.FadeOut(fadeSpeed, Color.black));
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        // Sleep (advance day)
        if (dayNightManager != null)
        {
            dayNightManager.PlayerGoesToSleep();
        }
        
        // Wait a moment
        yield return new WaitForSeconds(1f);
        
        // Fade in
        if (useFadeTransition && fadeManager != null)
        {
            yield return StartCoroutine(fadeManager.FadeIn(fadeSpeed));
        }
        
        // Unlock player movement
        if (movement != null)
        {
            movement.enabled = true;
        }
        
        Debug.Log("☀️ Good morning!");
    }

    void OnTriggerStay2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact(other.gameObject);
        }
    }
}
}