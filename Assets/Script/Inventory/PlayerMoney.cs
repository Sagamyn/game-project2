using UnityEngine;
using System;

public class PlayerMoney : MonoBehaviour
{
    [Header("Money")]
    [SerializeField] private int currentMoney = 100; // Starting money

    public int CurrentMoney => currentMoney;

    public event Action<int> OnMoneyChanged; // Notify UI when money changes

    public bool HasMoney(int amount)
    {
        return currentMoney >= amount;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($" +${amount} (Total: ${currentMoney})");
    }

    public bool RemoveMoney(int amount)
    {
        if (amount <= 0) return false;

        if (!HasMoney(amount))
        {
            Debug.LogWarning($"Not enough money! Need ${amount}, have ${currentMoney}");
            return false;
        }

        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($" -${amount} (Total: ${currentMoney})");
        return true;
    }

    public void SetMoney(int amount)
    {
        currentMoney = Mathf.Max(0, amount);
        OnMoneyChanged?.Invoke(currentMoney);
    }
}