using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class FoodStallWaveManager : MonoBehaviour
{
    public static FoodStallWaveManager Instance;

    [Header("Merchant")]
    public Merchant merchant;

    [Header("Wave Setup")]
    public FoodStallWaveData[] waves;

    [Header("Customer Spawn")]
    public GameObject customerPrefab;
    public Transform customerSpawnPoint;

    [Header("Player")]
    public PlayerInventory playerInventory;
    public PlayerMoney playerMoney;

    [Header("Buttons")]
    public Button openStallButton;
    public Button continueButton;

    [Header("Main UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI moneyEarnedText;
    public TextMeshProUGUI customerCountText;

    public GameObject waitingForCustomerText;

    [Header("Wave Complete UI")]
    public GameObject waveCompletePanel;
    public TextMeshProUGUI waveCompleteText;

    [Header("Summary Panel")]
    public GameObject summaryPanel;

    public TextMeshProUGUI summaryCustomersText;
    public TextMeshProUGUI summaryHappyText;
    public TextMeshProUGUI summaryAngryText;
    public TextMeshProUGUI summaryMoneyText;

    public SummaryPanelAnimator summaryAnimator;

    [Header("Rolling Numbers")]
    public RollingNumberText customersRoll;
    public RollingNumberText happyRoll;
    public RollingNumberText angryRoll;
    public RollingNumberText moneyRoll;

    [Header("All Waves Complete")]
    public GameObject allWavesCompletePanel;
    [Header("Kebab")]
    public KebabFoodItem kebabResult;

    // =========================================
    // RUNTIME
    // =========================================

    private int currentWaveIndex = 0;
    private int currentCustomerIndex = 0;

    private int totalMoneyEarned = 0;

    private int customersServed = 0;
    private int happyCustomers = 0;
    private int angryCustomers = 0;

    private bool isRunning = false;
    private bool waitingForContinue = false;

    public static bool CanSleep = false;
    // Locked after wave session ends; unlocked when player sleeps
    public static bool CanStartWave = true;

    public FoodStallCustomer activeCustomer = null;

    // =========================================
    // START
    // =========================================

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);

        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        if (allWavesCompletePanel != null)
            allWavesCompletePanel.SetActive(false);

        if (waitingForCustomerText != null)
            waitingForCustomerText.SetActive(false);

        // Open Stall Button
        if (openStallButton != null)
            openStallButton.onClick.AddListener(StartWaves);

        // Continue Button
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);
    }

    // =========================================
    // START / STOP WAVES
    // =========================================

    public void StartWaves()
    {
        if (isRunning) return;
        if (!CanStartWave) return;  // locked until player sleeps

        currentWaveIndex = 0;
        currentCustomerIndex = 0;

        totalMoneyEarned = 0;

        customersServed = 0;
        happyCustomers = 0;
        angryCustomers = 0;

        isRunning = true;
        CanStartWave = false;  // locked for this session
        CanSleep = false;

        // Activate buffs the player bought last session
        BuffManager.Instance?.OnWaveStart();

        // Pick a random wave from the pool each day
        int randomIndex = Random.Range(0, waves.Length);
        currentWaveIndex = randomIndex;

        // Hide open button while playing
        if (openStallButton != null)
            openStallButton.gameObject.SetActive(false);

        Debug.Log($"Starting wave system... Picked wave [{randomIndex}]: {waves[randomIndex].waveName}");

        StartCoroutine(RunWave(waves[randomIndex]));
    }

    /// <summary>
    /// Called by TentInteractable after the player wakes up.
    /// Clears wave buffs, re-enables stall, and resets day flags.
    /// </summary>
    public void OnPlayerWokeUp()
    {
        CanStartWave = true;
        CanSleep = false;

        if (openStallButton != null)
            openStallButton.gameObject.SetActive(true);

        Debug.Log("[Stall] Player woke up — stall unlocked for next session.");
    }

    public void StopWaves()
    {
        isRunning = false;

        StopAllCoroutines();

        if (activeCustomer != null)
        {
            Destroy(activeCustomer.gameObject);
            activeCustomer = null;
        }

        Debug.Log("Wave system stopped");
    }

    // =========================================
    // WAVE LOOP
    // =========================================

    IEnumerator RunWave(FoodStallWaveData wave)
    {
        activeCustomer = null;
        currentCustomerIndex = 0;
        UpdateWaveUI();

        Debug.Log($"Starting {wave.waveName}");

        yield return new WaitForSeconds(1f);

        foreach (var customerData in wave.customers)
        {
            if (!isRunning)
                yield break;

            // Wait previous customer leave
            while (activeCustomer != null)
                yield return null;
            Debug.Log($"Customer muncul nih");

            float waitTime = Random.Range(
                wave.minTimeBetweenCustomers,
                wave.maxTimeBetweenCustomers
            );

            if (waitingForCustomerText != null)
                waitingForCustomerText.SetActive(true);

            yield return new WaitForSeconds(waitTime);

            if (waitingForCustomerText != null)
                waitingForCustomerText.SetActive(false);

            SpawnCustomer(customerData);

            currentCustomerIndex++;

            UpdateWaveUI();

            // Wait customer leave
            while (activeCustomer != null)
                yield return null;
        }

        // Wave Complete — one random wave per session, then end the day
        yield return StartCoroutine(ShowWaveComplete(wave));

        StartCoroutine(ShowAllComplete());
    }

    // =========================================
    // CUSTOMER
    // =========================================

    void SpawnCustomer(CustomerData data)
    {
        if (customerPrefab == null || customerSpawnPoint == null)
        {
            Debug.LogError("CustomerPrefab or SpawnPoint missing!");
            return;
        }

        Debug.Log($"check prefab & check spawnpoint passed");

        GameObject obj = Instantiate(
            customerPrefab,
            customerSpawnPoint.position,
            Quaternion.identity,
            customerSpawnPoint
        );

        FoodStallCustomer customer =
            obj.GetComponent<FoodStallCustomer>();

        if (customer == null)
        {
            Debug.LogError("Customer prefab missing FoodStallCustomer!");
            return;
        }

        customer.OnLeft += OnCustomerLeft;

        customer.Spawn(data);

        activeCustomer = customer;
        Debug.Log($"Spawned customer: {data.customerName}");
    }

    void OnCustomerLeft(
        FoodStallCustomer customer,
        bool success,
        int pay
    )
    {
        customersServed++;

        if (success)
        {
            happyCustomers++;

            totalMoneyEarned += pay;

            if (playerMoney != null)
                playerMoney.AddMoney(pay);

            Debug.Log($"Served customer! +${pay}");
        }
        else
        {
            angryCustomers++;

            Debug.Log("Customer left angry!");
        }

        if (moneyEarnedText != null)
            moneyEarnedText.text = $"${totalMoneyEarned}";

        activeCustomer = null;
    }

    // =========================================
    // SERVE SYSTEM
    // =========================================

    public void ServeCurrentCustomer()
    {
        if (activeCustomer == null) return;

        // Cek inventory ada kebab tidak
        int amount = playerInventory.GetAmount(kebabResult);
        if (amount <= 0)
        {
            Debug.Log("Belum ada kebab di inventory!");
            return;
        }

        // Ambil LastResult dari KebabAssembly
        KebabResult result = KebabAssembly.LastResult;
        if (result == null)
        {
            Debug.LogWarning("LastResult null!");
            return;
        }

        // Consume dari inventory
        playerInventory.ConsumeItem(kebabResult, 1);

        // Set harga override
        activeCustomer.ServePriceOverride =
            result.CalculatePrice(activeCustomer.AssignedRecipe);

        // Serve ke customer
        activeCustomer.TryServeKebab(result, activeCustomer.AssignedRecipe);
    }

    // =========================================
    // UI
    // =========================================

    void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text =
                $"Wave {currentWaveIndex + 1} / {waves.Length}";
        }

        if (customerCountText != null &&
            currentWaveIndex < waves.Length)
        {
            int total =
                waves[currentWaveIndex].customers.Length;

            customerCountText.text =
                $"Customer {currentCustomerIndex} / {total}";
        }
    }

    // =========================================
    // WAVE COMPLETE
    // =========================================

    IEnumerator ShowWaveComplete(FoodStallWaveData wave)
    {
        // Bonus reward
        if (wave.waveCompletionBonus > 0)
        {
            totalMoneyEarned += wave.waveCompletionBonus;

            if (playerMoney != null)
            {
                playerMoney.AddMoney(
                    wave.waveCompletionBonus
                );
            }
        }

        // Show wave complete popup
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);

            if (waveCompleteText != null)
            {
                waveCompleteText.text =
                    $"{wave.waveName} Complete!\n" +
                    $"+${wave.waveCompletionBonus} bonus!";
            }
        }

        // Show summary panel
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(true);

            // Slide animation
            if (summaryAnimator != null)
            {
                summaryAnimator.ShowPanel();
            }

            // Slot machine rolling numbers
            if (customersRoll != null)
            {
                yield return StartCoroutine(
                    customersRoll.RollTo(customersServed)
                );
            }

            yield return new WaitForSeconds(0.2f);

            if (happyRoll != null)
            {
                yield return StartCoroutine(
                    happyRoll.RollTo(happyCustomers)
                );
            }

            yield return new WaitForSeconds(0.2f);

            if (angryRoll != null)
            {
                yield return StartCoroutine(
                    angryRoll.RollTo(angryCustomers)
                );
            }

            yield return new WaitForSeconds(0.2f);

            if (moneyRoll != null)
            {
                yield return StartCoroutine(
                    moneyRoll.RollTo(totalMoneyEarned, "$")
                );
            }

        }

        // WAIT FOR PLAYER TO CLICK CONTINUE
        waitingForContinue = true;

        while (waitingForContinue)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        if (merchant != null)
        {
            yield return StartCoroutine(
                merchant.AppearFromDark()
            );
        }
    }

    // =========================================
    // CONTINUE BUTTON
    // =========================================

    public void OnContinuePressed()
    {
        waitingForContinue = false;

        if (merchant != null)
        {
            merchant.Hide();
        }

        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);

        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        Debug.Log("Continue pressed!");
    }

    // =========================================
    // ALL WAVES COMPLETE
    // =========================================

    IEnumerator ShowAllComplete()
    {
        isRunning = false;

        // Clear wave buffs — they expire at session end
        BuffManager.Instance?.OnWaveEnd();

        if (allWavesCompletePanel != null)
            allWavesCompletePanel.SetActive(true);

        // Do NOT re-enable the stall button here.
        // OnPlayerWokeUp() handles that after the player sleeps.

        Debug.Log(
            $"All waves complete! Total earned: ${totalMoneyEarned}"
        );

        yield return null;
    }

    public void OnMerchantShopDone()
    {
        if (merchant != null)
            merchant.Hide();

        waitingForContinue = false;

        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);

        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        // Player finished waves AND bought their buffs — allow sleeping
        CanSleep = true;
        Debug.Log("[Tent] CanSleep = true — player may now rest.");
    }
}
