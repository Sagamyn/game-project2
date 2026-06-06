using UnityEngine;
using TMPro;
using System.Collections;

public class RollingNumberText : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textUI;

    [Header("Rolling")]
    public float duration = 2f;

    [Header("Fake Slot Numbers")]
    public int fakeMin = 0;
    public int fakeMax = 999;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip tickSound;

    public IEnumerator RollTo(int finalValue, string prefix = "")
    {
        yield return StartCoroutine(
            RollNumbers(finalValue, prefix)
        );
    }

    IEnumerator RollNumbers(int target, string prefix)
    {
        float timer = 0;

        float tickTimer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // Speed slows near end
            float progress = timer / duration;

            // Random fake number
            int fakeNumber =
                Random.Range(fakeMin, fakeMax);

            textUI.text = prefix + fakeNumber;

            // Tick tick tick sound
            tickTimer += Time.deltaTime;

            // Faster at start, slower near end
            float currentTickRate =
                Mathf.Lerp(0.03f, 0.2f, progress);

            if (tickTimer >= currentTickRate)
            {
                tickTimer = 0;

                if (audioSource != null &&
                    tickSound != null)
                {
                    audioSource.pitch =
                        Random.Range(0.95f, 1.05f);

                    audioSource.PlayOneShot(tickSound);
                }
            }

            yield return null;
        }

        // Final value
        textUI.text = prefix + target;

        // Final stop sound
        if (audioSource != null &&
            tickSound != null)
        {
            audioSource.pitch = 0.8f;
            audioSource.PlayOneShot(tickSound);
        }
    }
}