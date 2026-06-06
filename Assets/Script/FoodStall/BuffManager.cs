using UnityEngine;
using System.Collections.Generic;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance;


    // Permanent buffs (survive across waves)
    [SerializeField] private List<BuffData> permanentBuffs = new List<BuffData>();

    // One-wave buffs (set at merchant, activate next wave, expire after)
    [SerializeField] private List<BuffData> pendingBuffs = new List<BuffData>();
    [SerializeField] private List<BuffData> activeWaveBuffs = new List<BuffData>();

    // Money
    public float moneyMultiplier           = 1f;
    public int   bonusPerHappyCustomer     = 0;
    public bool  angryCustomerStillPays    = false;

    // Customer behaviour
    public bool  noPatience                = false; // Death 13
    public bool  wrongFoodNoLeave          = false; // Hey Ya!

    // Player
    public bool  isInvincible             = false;  // The Hand

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning($"[BuffManager] Duplicate detected! Destroying {gameObject.name} (ID:{GetInstanceID()}). Real instance is ID:{Instance.GetInstanceID()}");
            Destroy(gameObject);
        }
    }
    public void ApplyBuff(BuffData buff)
    {
        switch (buff.buffType)
        {
            // ── Permanent: takes effect immediately ──
            case BuffType.HealPlayer:
                PlayerHealth.Instance?.Heal((int)buff.value);
                permanentBuffs.Add(buff);
                Debug.Log($"Permanent buff applied: {buff.buffName}");
                break;

            case BuffType.MaxHealthUp:
                PlayerHealth.Instance?.SetMaxHealth(
                    PlayerHealth.Instance.maxHealth + (int)buff.value);
                permanentBuffs.Add(buff);
                break;

            case BuffType.FreeRerollNextMerchant:
                permanentBuffs.Add(buff);
                break;

            // ── One-wave: queued for next wave ──
            default:
                pendingBuffs.Add(buff);
                Debug.Log($"Buff queued for next wave: {buff.buffName}");
                break;
        }
    }

    public void OnWaveStart()
    {
        // Move pending buffs to active
        activeWaveBuffs.Clear();
        activeWaveBuffs.AddRange(pendingBuffs);
        pendingBuffs.Clear();

        // Reset all wave values first
        ResetWaveValues();

        // Apply each active wave buff
        foreach (var buff in activeWaveBuffs)
            ActivateWaveBuff(buff);

        Debug.Log($"[BuffManager ID:{GetInstanceID()}] Wave started with {activeWaveBuffs.Count} active buffs. isInvincible={isInvincible}, moneyMultiplier={moneyMultiplier}");
    }

    void ActivateWaveBuff(BuffData buff)
    {
        switch (buff.buffType)
        {
            case BuffType.DoublePayNextWave:
                moneyMultiplier += buff.value;
                break;

            case BuffType.BonusPerHappyCustomer:
                bonusPerHappyCustomer += (int)buff.value;
                break;

            case BuffType.RefundOnAngry:
                angryCustomerStillPays = true;
                break;

            case BuffType.MorePatience:
                // Applied directly to CustomerData.patience multiplier
                // FoodStallCustomer checks BuffManager.Instance.patienceMultiplier
                patienceMultiplier += buff.value;
                break;
            case BuffType.NoPatienceNextWave:
                noPatience = true;
                Debug.Log("Death 13 — no patience drain this wave!");
                break;

            case BuffType.FewerCustomers:
                fewerCustomers += (int)buff.value;
                break;

            case BuffType.WrongFoodNoLeave:
                wrongFoodNoLeave = true;
                break;

            case BuffType.InvincibleNextWave:
                isInvincible = true;
                break;

            case BuffType.SkipOneAngryCustomer:
                skipAngryCount += (int)buff.value;
                break;
        }

        Debug.Log($"Wave buff activated: {buff.buffName}");
    }


    public void OnWaveEnd()
    {
        activeWaveBuffs.Clear();
        ResetWaveValues();
        Debug.Log("Wave ended — temporary buffs expired");
    }

    void ResetWaveValues()
    {
        moneyMultiplier        = 1f;
        bonusPerHappyCustomer  = 0;
        angryCustomerStillPays = false;
        noPatience             = false;
        wrongFoodNoLeave       = false;
        isInvincible           = false;
        patienceMultiplier     = 1f;
        fewerCustomers         = 0;
        skipAngryCount         = 0;
    }

    // Extra wave values
    public float patienceMultiplier = 1f;
    public int   fewerCustomers     = 0;
    public int   skipAngryCount     = 0;


    public bool HasBuff(BuffType type)
    {
        return permanentBuffs.Exists(b => b.buffType == type)
            || activeWaveBuffs.Exists(b => b.buffType == type)
            || pendingBuffs.Exists(b => b.buffType == type);
    }

    public bool HasPendingBuff(BuffType type)
    {
        return pendingBuffs.Exists(b => b.buffType == type);
    }

    public void ClearAll()
    {
        permanentBuffs.Clear();
        pendingBuffs.Clear();
        activeWaveBuffs.Clear();
        ResetWaveValues();
    }
}