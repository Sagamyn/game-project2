using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minigame mancing bergaya Stardew Valley untuk fase memasak.
/// Pemain mengontrol Catch Bar dengan menahan tombol mouse kiri agar Target tetap di dalam
/// rentang Catch Bar sehingga Progress bertambah dan mencapai 100.
/// </summary>
public class CookingTemperatureMinigame : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform verticalBar;
    public RectTransform catchBar;
    public RectTransform target;
    public Slider progressBar;
    public UIAnimator uiAnimator;

    [Header("Difficulty (fallback if Recipe has none)")]
    public MinigameDifficulty defaultDifficulty;

    [Header("Optional")]
    public Sprite targetIcon; // Optional. Jika di-set, akan diaplikasikan ke Image target di StartMinigame.

    // Runtime state
    private float currentProgress;
    private float catchBarPosY;
    private float catchBarVelY;
    private float targetPosY;
    private float targetGoalPosY;
    private float targetIdleTimer;
    private bool isPlaying;
    private MinigameDifficulty activeDifficulty;
    private CookingStation currentStation;
    private float barHeight;
    private float catchBarHeight;

    /// <summary>
    /// Memulai minigame menggunakan <see cref="defaultDifficulty"/> sebagai fallback.
    /// Dipertahankan untuk kompatibilitas dengan caller lama.
    /// </summary>
    public void StartMinigame(CookingStation station)
    {
        StartMinigame(station, null);
    }

    /// <summary>
    /// Memulai minigame untuk sebuah resep. Jika <paramref name="recipe"/> punya
    /// <see cref="MinigameDifficulty"/> yang valid, parameter tersebut yang dipakai;
    /// jika tidak, fallback ke <see cref="defaultDifficulty"/>.
    /// </summary>
    public void StartMinigame(CookingStation station, Recipe recipe)
    {
        // Validasi referensi UI
        if (verticalBar == null)
        {
            Debug.LogError("[CookingTemperatureMinigame] 'verticalBar' belum di-assign.");
            currentStation = station;
            EndMinigame(false);
            return;
        }
        if (catchBar == null)
        {
            Debug.LogError("[CookingTemperatureMinigame] 'catchBar' belum di-assign.");
            currentStation = station;
            EndMinigame(false);
            return;
        }
        if (target == null)
        {
            Debug.LogError("[CookingTemperatureMinigame] 'target' belum di-assign.");
            currentStation = station;
            EndMinigame(false);
            return;
        }

        currentStation = station;
        activeDifficulty = ResolveDifficulty(recipe);

        if (activeDifficulty == null
            || activeDifficulty.catchBarSize <= 0f
            || activeDifficulty.targetSpeed <= 0f
            || activeDifficulty.pushForce <= 0f)
        {
            Debug.LogError("[CookingTemperatureMinigame] MinigameDifficulty tidak valid (defaultDifficulty kosong atau Recipe.minigameDifficulty kosong dan tidak ada fallback).");
            EndMinigame(false);
            return;
        }

        isPlaying = true;
        currentProgress = 50f;

        // Pastikan layout sudah dihitung sebelum membaca height
        Canvas.ForceUpdateCanvases();
        barHeight = verticalBar.rect.height;
        if (barHeight <= 0f)
        {
            Debug.LogWarning("[CookingTemperatureMinigame] VerticalBar height is 0 — abort minigame.");
            EndMinigame(false);
            return;
        }

        catchBarHeight = activeDifficulty.catchBarSize * barHeight;

        catchBarPosY = barHeight / 2f;
        catchBarVelY = 0f;
        targetPosY = barHeight / 2f;
        targetGoalPosY = targetPosY;
        targetIdleTimer = Random.Range(activeDifficulty.minIdleTime, activeDifficulty.maxIdleTime);

        ApplyCatchBarHeight();

        // Optional: terapkan sprite Target jika disediakan
        if (targetIcon != null)
        {
            var img = target.GetComponent<Image>();
            if (img != null) img.sprite = targetIcon;
        }

        SyncUI();

        if (uiAnimator != null) uiAnimator.ShowInstant();
        else gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isPlaying) return;

        float dt = Time.deltaTime;
        HandleInput(dt);
        UpdateTarget(dt);
        UpdateProgress(dt);
        SyncUI();
    }

    private void HandleInput(float dt)
    {
        float accel = Input.GetMouseButton(0)
            ? activeDifficulty.pushForce
            : -activeDifficulty.gravity;

        var result = MinigamePure.IntegrateCatchBar(
            catchBarPosY,
            catchBarVelY,
            accel,
            dt,
            catchBarHeight / 2f,
            barHeight - catchBarHeight / 2f);

        catchBarPosY = result.pos;
        catchBarVelY = result.vel;
    }

    private void UpdateTarget(float dt)
    {
        if (targetIdleTimer > 0f)
        {
            targetIdleTimer -= dt;
            if (targetIdleTimer <= 0f)
            {
                targetIdleTimer = 0f;
                targetGoalPosY = PickRandomGoal(targetPosY);
            }
            return;
        }

        float step = activeDifficulty.targetSpeed * dt;
        if (Mathf.Abs(targetGoalPosY - targetPosY) <= step)
        {
            targetPosY = targetGoalPosY;
            targetIdleTimer = Random.Range(activeDifficulty.minIdleTime, activeDifficulty.maxIdleTime);
        }
        else
        {
            targetPosY = MinigamePure.StepTargetTowardsGoal(
                targetPosY,
                targetGoalPosY,
                activeDifficulty.targetSpeed,
                dt);
        }

        targetPosY = Mathf.Clamp(targetPosY, 0f, barHeight);
    }

    private void UpdateProgress(float dt)
    {
        bool inside = MinigamePure.IsTargetInsideCatchBar(targetPosY, catchBarPosY, catchBarHeight);
        currentProgress = MinigamePure.StepProgress(
            currentProgress,
            inside,
            activeDifficulty.progressGainRate,
            activeDifficulty.progressLossRate,
            dt);

        if (currentProgress >= 100f) EndMinigame(true);
        else if (currentProgress <= 0f) EndMinigame(false);
    }

    private void SyncUI()
    {
        if (catchBar != null)
        {
            var p = catchBar.anchoredPosition;
            catchBar.anchoredPosition = new Vector2(p.x, catchBarPosY);
        }
        if (target != null)
        {
            var p = target.anchoredPosition;
            target.anchoredPosition = new Vector2(p.x, targetPosY);
        }
        if (progressBar != null) progressBar.value = currentProgress;
    }

    private MinigameDifficulty ResolveDifficulty(Recipe recipe)
    {
        if (recipe != null
            && recipe.minigameDifficulty != null
            && recipe.minigameDifficulty.catchBarSize > 0f
            && recipe.minigameDifficulty.targetSpeed > 0f
            && recipe.minigameDifficulty.pushForce > 0f)
        {
            return recipe.minigameDifficulty;
        }
        return defaultDifficulty;
    }

    private float PickRandomGoal(float currentY)
    {
        float goal = currentY;
        float minDist = 0.05f * barHeight;
        for (int i = 0; i < 5; i++)
        {
            goal = Random.Range(0f, barHeight);
            if (Mathf.Abs(goal - currentY) > minDist) return goal;
        }
        return goal;
    }

    private void ApplyCatchBarHeight()
    {
        if (catchBar == null) return;
        var sd = catchBar.sizeDelta;
        catchBar.sizeDelta = new Vector2(sd.x, catchBarHeight);
    }

    private void EndMinigame(bool success)
    {
        isPlaying = false;

        if (uiAnimator != null) uiAnimator.HideInstant();
        else gameObject.SetActive(false);

        var finished = currentStation;
        currentStation = null;

        if (finished != null) finished.OnMinigameComplete(success);
        else Debug.LogWarning("[CookingTemperatureMinigame] EndMinigame called without station.");
    }
}

/// <summary>
/// Pure helper functions untuk logika minigame. Tidak menyentuh Time, Input, RectTransform,
/// atau UnityEngine.Random. Murni input → output, sehingga dapat diuji secara terisolasi.
/// </summary>
internal static class MinigamePure
{
    /// <summary>
    /// Semi-implicit Euler: vel' = vel + accel*dt; pos' = pos + vel'*dt.
    /// Setelah integrasi, pos' di-clamp ke [lo, hi]. Jika pos' menyentuh batas (akibat
    /// clamp), vel' di-set 0.
    /// </summary>
    public static (float pos, float vel) IntegrateCatchBar(
        float pos, float vel, float accel, float dt, float lo, float hi)
    {
        float newVel = vel + accel * dt;
        float newPos = pos + newVel * dt;

        if (newPos <= lo)
        {
            newPos = lo;
            newVel = 0f;
        }
        else if (newPos >= hi)
        {
            newPos = hi;
            newVel = 0f;
        }

        return (newPos, newVel);
    }

    /// <summary>
    /// Mengembalikan true jika titik tengah Target (targetY) berada di dalam rentang
    /// vertikal Catch Bar [catchBarPosY - catchBarHeight/2, catchBarPosY + catchBarHeight/2].
    /// </summary>
    public static bool IsTargetInsideCatchBar(float targetY, float catchBarPosY, float catchBarHeight)
    {
        float half = catchBarHeight / 2f;
        return targetY >= catchBarPosY - half
            && targetY <= catchBarPosY + half;
    }

    /// <summary>
    /// Menambah progress dengan gainRate*dt jika inside, atau mengurangi dengan lossRate*dt
    /// jika tidak. Hasil di-clamp ke [0, 100].
    /// </summary>
    public static float StepProgress(
        float currentProgress, bool inside, float gainRate, float lossRate, float dt)
    {
        float next = inside
            ? currentProgress + gainRate * dt
            : currentProgress - lossRate * dt;

        if (next < 0f) next = 0f;
        else if (next > 100f) next = 100f;
        return next;
    }

    /// <summary>
    /// Memindahkan targetPosY menuju targetGoalPosY sejauh min(|goal - pos|, targetSpeed*dt).
    /// Jika sudah cukup dekat (langkah cukup untuk mencapai goal), kembalikan goal.
    /// </summary>
    public static float StepTargetTowardsGoal(
        float targetPosY, float targetGoalPosY, float targetSpeed, float dt)
    {
        float diff = targetGoalPosY - targetPosY;
        float absDiff = diff < 0f ? -diff : diff;
        float step = targetSpeed * dt;

        if (absDiff <= step) return targetGoalPosY;

        float dir = diff > 0f ? 1f : -1f;
        return targetPosY + dir * step;
    }
}
