using UnityEngine;
using System.Collections;

public enum CookingState
{
    Idle,
    Cooking,
    Plating
}

public class CookingStation : Interactable
{
    [Header("Cooking Station")]
    public Recipe[] availableRecipes;
    public CookingUI cookingUI;
    public CookingTemperatureMinigame temperatureMinigame;

    [Header("Visual Feedback")]
    public ParticleSystem cookingParticles;
    public AudioSource cookingSound;

    private bool isCooking = false;
    private bool isOpen = false;
    private CookingState currentState = CookingState.Idle;

    // Menyimpan recipe & inventory saat minigame berjalan
    private Recipe pendingRecipe;
    private PlayerInventory pendingInventory;

    public void ChangeState(CookingState newState)
    {
        currentState = newState;
        Debug.Log($"CookingStation state changed to: {newState}");

        switch (newState)
        {
            case CookingState.Idle:
                isCooking = false;
                break;
            case CookingState.Cooking:
                isCooking = true;
                break;
            case CookingState.Plating:
                isCooking = false;
                // TODO: Mulai plating/QTE phase
                break;
        }
    }

    public override void Interact()
    {
        if (isCooking)
        {
            Debug.Log("Already cooking!");
            return;
        }

        if (isOpen)
        {
            CloseCookingUI();
        }
        else
        {
            OpenCookingUI();
        }
    }

    void OpenCookingUI()
    {
        if (cookingUI == null)
        {
            Debug.LogError("CookingUI not assigned!");
            return;
        }

        isOpen = true;
        cookingUI.Open(this, availableRecipes);

        PlayerMovement.Instance?.LockMovement(true);

        // Hide hotbar
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.HideHotbar();
        }

        Debug.Log("Cooking Station opened");
    }

    // NEW: Public method that CookingUI can call when it closes
    public void OnUIClosed()
    {
        isOpen = false;
        
        PlayerMovement.Instance?.LockMovement(false);

        // Show hotbar
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.ShowHotbar();
        }

        Debug.Log("Cooking Station closed (via UI)");
    }

    void CloseCookingUI()
    {
        if (cookingUI == null) return;

        isOpen = false;
        cookingUI.Close();

        PlayerMovement.Instance?.LockMovement(false);

        // Show hotbar
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.ShowHotbar();
        }

        Debug.Log("Cooking Station closed");
    }

    public void StartCooking(Recipe recipe, PlayerInventory inventory)
    {
        if (isCooking)
        {
            Debug.LogWarning("Already cooking!");
            return;
        }

        if (!recipe.CanCraft(inventory))
        {
            Debug.LogWarning("Not enough ingredients!");
            return;
        }

        // Jika ada Temperature Minigame, gunakan minigame
        if (temperatureMinigame != null)
        {
            // Simpan recipe & inventory untuk diproses setelah minigame selesai
            pendingRecipe = recipe;
            pendingInventory = inventory;

            isCooking = true;
            ChangeState(CookingState.Cooking);

            // Tutup CookingUI supaya layar bersih untuk minigame
            if (cookingUI != null)
            {
                cookingUI.Close();
            }

            // Visual feedback
            if (cookingParticles != null) cookingParticles.Play();
            if (cookingSound != null) cookingSound.Play();

            // Mulai minigame!
            Debug.Log($"Memulai Temperature Minigame untuk {recipe.recipeName}...");
            temperatureMinigame.StartMinigame(this);
            return;
        }

        // Fallback: tanpa minigame, langsung pakai coroutine timer
        StartCoroutine(CookingProcess(recipe, inventory));
    }

    /// <summary>
    /// Dipanggil oleh CookingTemperatureMinigame saat minigame selesai.
    /// </summary>
    public void OnMinigameComplete(bool success)
    {
        // Stop effects
        if (cookingParticles != null) cookingParticles.Stop();
        if (cookingSound != null) cookingSound.Stop();

        if (success && pendingRecipe != null && pendingInventory != null)
        {
            // Konsumsi bahan
            foreach (var ing in pendingRecipe.ingredients)
            {
                pendingInventory.ConsumeItem(ing.ingredient, ing.amount);
            }

            // Berikan hasil masakan
            bool added = pendingInventory.AddItem(pendingRecipe.resultItem, pendingRecipe.resultAmount);
            if (added)
            {
                Debug.Log($"✓ Berhasil memasak {pendingRecipe.resultAmount}x {pendingRecipe.resultItem.itemName}!");
            }
            else
            {
                Debug.LogWarning("Inventory penuh! Hasil masakan hilang!");
            }
        }
        else if (!success)
        {
            // Gagal (gosong) - bahan tetap dikonsumsi sebagai penalti
            if (pendingRecipe != null && pendingInventory != null)
            {
                foreach (var ing in pendingRecipe.ingredients)
                {
                    pendingInventory.ConsumeItem(ing.ingredient, ing.amount);
                }
            }
            Debug.LogWarning("Masakan GOSONG! Bahan terbuang!");
        }

        // Reset state
        isCooking = false;
        ChangeState(CookingState.Idle);
        pendingRecipe = null;
        pendingInventory = null;

        // Unlock player movement
        PlayerMovement.Instance?.LockMovement(false);

        // Show hotbar kembali
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.ShowHotbar();
        }
    }

    IEnumerator CookingProcess(Recipe recipe, PlayerInventory inventory)
    {
        isCooking = true;

        // Consume ingredients
        foreach (var ing in recipe.ingredients)
        {
            inventory.ConsumeItem(ing.ingredient, ing.amount);
        }

        Debug.Log($"Started cooking {recipe.recipeName}...");

        // Visual feedback
        if (cookingParticles != null)
            cookingParticles.Play();

        if (cookingSound != null)
            cookingSound.Play();

        // Update UI
        if (cookingUI != null)
        {
            cookingUI.ShowCookingProgress(recipe, recipe.cookingTime);
        }

        // Wait for cooking time
        yield return new WaitForSeconds(recipe.cookingTime);

        // Add result to inventory
        bool success = inventory.AddItem(recipe.resultItem, recipe.resultAmount);

        if (success)
        {
            Debug.Log($"✓ Cooked {recipe.resultAmount}x {recipe.resultItem.itemName}!");
        }
        else
        {
            Debug.LogWarning("Inventory full! Cooked item lost!");
        }

        // Stop effects
        if (cookingParticles != null)
            cookingParticles.Stop();

        if (cookingSound != null)
            cookingSound.Stop();

        isCooking = false;

        // Update UI
        if (cookingUI != null)
        {
            cookingUI.OnCookingComplete(success);
        }
    }

    public bool IsCooking() => isCooking;

    void OnDestroy()
    {
        if (isOpen)
        {
            CloseCookingUI();
        }
    }
}