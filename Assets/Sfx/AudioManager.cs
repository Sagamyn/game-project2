using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("Farming")]
    public AudioClip hoeSound;
    public AudioClip shovelSound;
    public AudioClip plantSound;
    public AudioClip waterSound;
    public AudioClip harvestSound;

    [Header("World")]
    public AudioClip axeHitSound;
    public AudioClip axeBreakSound;
    public AudioClip pickaxeHitSound;
    public AudioClip rockBreakSound;

    public AudioClip rainSound;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(clip, volume);
    }
}
