using UnityEngine;

public class HotbarVisibilityManager : MonoBehaviour
{
    public static HotbarVisibilityManager Instance { get; private set; }

    [Header("References")]
    public GameObject hotbarPanel; // The entire hotbar UI GameObject

    [Header("Settings")]
    public bool hideOnChest = true;
    public bool hideOnDialogue = true;
    public bool hideOnInventory = false; // Optional: hide when inventory is open

    private bool isHidden = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Make sure hotbar is visible at start
        ShowHotbar();
    }

    public void ShowHotbar()
    {
        if (hotbarPanel != null)
        {
            hotbarPanel.SetActive(true);
            isHidden = false;
            Debug.Log("✓ Hotbar shown");
        }
    }

    public void HideHotbar()
    {
        if (hotbarPanel != null)
        {
            hotbarPanel.SetActive(false);
            isHidden = true;
            Debug.Log("✓ Hotbar hidden");
        }
    }

    public bool IsHidden() => isHidden;

    // Called when chest is opened
    public void OnChestOpen()
    {
        if (hideOnChest)
        {
            HideHotbar();
        }
    }

    // Called when chest is closed
    public void OnChestClose()
    {
        if (hideOnChest)
        {
            ShowHotbar();
        }
    }

    // Called when dialogue/NPC interaction starts
    public void OnDialogueStart()
    {
        if (hideOnDialogue)
        {
            HideHotbar();
        }
    }

    // Called when dialogue/NPC interaction ends
    public void OnDialogueEnd()
    {
        if (hideOnDialogue)
        {
            ShowHotbar();
        }
    }

    // Called when inventory is opened
    public void OnInventoryOpen()
    {
        if (hideOnInventory)
        {
            HideHotbar();
        }
    }

    // Called when inventory is closed
    public void OnInventoryClose()
    {
        if (hideOnInventory)
        {
            ShowHotbar();
        }
    }
}