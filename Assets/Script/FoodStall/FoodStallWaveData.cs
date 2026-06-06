using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "FoodStall/Wave Data")]
public class FoodStallWaveData : ScriptableObject
{
    [Header("Wave Info")]
    public string waveName = "Wave 1";

    [Header("Customers In This Wave")]
    public CustomerData[] customers; // Each entry = one customer in order

    [Header("Timing")]
    [Tooltip("Minimum seconds before next customer appears after previous leaves")]
    public float minTimeBetweenCustomers = 2f;
    [Tooltip("Maximum seconds before next customer appears after previous leaves")]
    public float maxTimeBetweenCustomers = 5f;

    [Header("Reward")]
    public int waveCompletionBonus = 100;
}