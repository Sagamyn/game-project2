using UnityEngine;

[System.Serializable]
public class MinigameDifficulty
{
    [Range(0.05f, 0.9f)] public float catchBarSize;     // 0..1, fraksi tinggi VerticalBar
    public float targetSpeed;                            // unit anchored Y per detik
    public float progressGainRate;                       // poin progress per detik (overlap)
    public float progressLossRate;                       // poin progress per detik (lepas)
    public float gravity;                                // unit anchored Y per detik^2 (negatif arah turun)
    public float pushForce;                              // unit anchored Y per detik^2 (positif arah naik)
    public float minIdleTime;                            // detik
    public float maxIdleTime;                            // detik
}
