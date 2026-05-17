using UnityEngine;

[System.Serializable]
public class MinigameDifficulty
{
    [Range(0.05f, 0.9f)] public float catchBarSize;
    public float targetSpeed;
    public float progressGainRate;
    public float progressLossRate;
    public float gravity;
    public float pushForce;
    public float minIdleTime;
    public float maxIdleTime;
}
