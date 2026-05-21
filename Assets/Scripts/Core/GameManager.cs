using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GamePhase { Morning, Noon, Afternoon, Night }
public enum EndingType { None, TrueEscape, BadCredit, Arrested, RiotSurvivor, Death, Trapped }

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Ban Đêm")]
    public GameObject caughtPanelUI;

    [Header("Chế độ Dev (Dành cho Test Đêm 5)")]
    public bool enableDevMode = false;
    [Range(1, 6)] public int testStartDay = 5;
    public GamePhase testStartPhase = GamePhase.Night;

    [Header("Hack Vật Phẩm (Tích vào để test nhanh)")]
    public bool testHasNotebook = true;
    public bool testHasRope = true;
    public bool testHasNippers = true;
    public bool testHasKey = true;

    [Header("Thông định Nhân vật")]
    public string playerName = "Tuấn";

    [Header("Chỉ số Sinh tồn (ĐÃ BỎ THỂ LỰC)")]
    public int hp = 100;
    public int maxHp = 100;
    public bool hasAskedToContinue = false;

    [Header("Chỉ số Tiến trình & Tiền Bạc")]
    public float money = 0f;

    [Header("Hệ thống Lương & KPI")]
    public int currentDay = 1;
    public int maxDays = 5;
    public int attemptedScamsToday = 0;
    public int successfulScamsToday = 0;

    [Tooltip("Tổng số vụ lừa thành công trong cả game")]
    public int totalSuccessfulScamsAllDays = 0;
    [Tooltip("Lừa thành công bao nhiêu người thì bị Công an tóm?")]
    public int policeArrestThreshold = 15;

    public int targetKPI = 3;
    public int maxAttemptsPerDay = 5;
    public float currentCommissionRate = 0.1f;
    public float typingDifficultyMultiplier = 1.0f;
    public int exchangeRateVND = 25000;
    public int consecutiveScamFails = 0;

    [Header("Vật phẩm Vượt ngục (Chỉ còn 4 món)")]
    public bool hasNotebook = false;
    public bool hasNippers = false;
    public bool hasRope = false;
    public bool hasKey = false;

    [Header("Hệ thống Mạng (Đêm thám thính)")]
    public int caughtCountThisNight = 0;
    public int maxCaughtBeforeReset = 3;

    [Header("--- KẾT NỐI CAMERA & ÂM THANH ---")]
    public Transform playerCamera;
    public AudioSource sfxSource;
    public AudioClip shockSound;

    [Header("--- HỆ THỐNG VÒNG LẶP ---")]
    public GamePhase currentPhase = GamePhase.Morning;
    public bool hasTalkedToNPC = false;
    public bool unlockedScouting = false;

    [Header("--- HỆ THỐNG SÒNG BẠC & TÍN DỤNG ĐEN ---")]
    public bool isCasinoLocked = false;
    public bool isBlackCreditActive = false;

    [Header("Sự kiện Đêm 5")]
    public bool isEscapeStart = false;
    public int escapeProgress = 0;
    public bool isRedAlert = false;

    public EndingType currentEnding = EndingType.None;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (enableDevMode)
        {
            Debug.Log("<color=magenta>--- DEV MODE KÍCH HOẠT: BƠM FULL 4 MÓN ĐỒ VÀO TÚI ---</color>");
            currentDay = testStartDay;
            currentPhase = testStartPhase;
            hasNotebook = testHasNotebook;
            hasRope = testHasRope;
            hasNippers = testHasNippers;
            hasKey = testHasKey;
            if (SceneManager.GetActiveScene().name.Contains("Night")) currentPhase = GamePhase.Night;
        }
        else
        {
            hasNotebook = PlayerPrefs.GetInt("HasNotebook", 0) == 1;
            hasNippers = PlayerPrefs.GetInt("HasNippers", 0) == 1;
            hasRope = PlayerPrefs.GetInt("HasRope", 0) == 1;
            hasKey = PlayerPrefs.GetInt("HasKey", 0) == 1;
        }

        SyncCasinoData();
    }

    public void SyncCasinoData()
    {
        isBlackCreditActive = PlayerPrefs.GetInt("Casino_BlackCredit", 0) == 1;
        isCasinoLocked = PlayerPrefs.GetInt("Casino_Locked", 0) == 1;
    }

    public void StartEscape()
    {
        isEscapeStart = true;
        isRedAlert = true;
        RenderSettings.ambientLight = Color.red;
    }

    public bool StartScammingVictim(int dummyStaminaCost = 0)
    {
        if (hp <= 0) { CheckDeath(); return false; }
        return true;
    }

    public void OnScamSuccess(float rawVNDEarned, int dummyKarmaLost = 0)
    {
        attemptedScamsToday++;
        successfulScamsToday++;
        totalSuccessfulScamsAllDays++;
        consecutiveScamFails = 0;

        money += (rawVNDEarned * currentCommissionRate);
        CheckShiftProgress();
    }

    public void OnScamFail()
    {
        attemptedScamsToday++;
        consecutiveScamFails++;

        if (consecutiveScamFails >= 2)
        {
            TakeShockDamage(50);
            consecutiveScamFails = 0;
        }
        else
        {
            TakeShockDamage(10);
        }
        CheckShiftProgress();
    }

    private void CheckShiftProgress()
    {
        if (currentPhase == GamePhase.Morning && attemptedScamsToday >= 3)
            TransitionToPhase(GamePhase.Noon);
        else if (currentPhase == GamePhase.Afternoon && attemptedScamsToday >= maxAttemptsPerDay)
        {
            ShiftSummaryUI summaryUI = FindFirstObjectByType<ShiftSummaryUI>(FindObjectsInactive.Include);

            if (summaryUI != null)
            {
                summaryUI.ShowForceEndShift();
            }
            else
            {
                EndDaySummary();
                TransitionToPhase(GamePhase.Night);
            }
        }
        else
        {
            
            VictimSelectionManager vsm = FindFirstObjectByType<VictimSelectionManager>(FindObjectsInactive.Include);
            if (vsm != null) vsm.ShowSelectionUI(currentDay);
        }
    }

    public void EndDaySummary()
    {
        if (successfulScamsToday >= targetKPI)
        {
            if (targetKPI == 3) { targetKPI = 4; currentCommissionRate = 0.2f; }
            else if (targetKPI == 4) { targetKPI = 5; currentCommissionRate = 0.3f; }
            else if (targetKPI == 5 && successfulScamsToday == 5)
            {
                typingDifficultyMultiplier = Mathf.Max(0.5f, typingDifficultyMultiplier - 0.15f);
            }
        }
        else
        {
            int shockDamage = (targetKPI - successfulScamsToday) * 30;
            TakeShockDamage(shockDamage);
            targetKPI = 3;
            currentCommissionRate = 0.1f;
            typingDifficultyMultiplier = 1.0f;
        }
    }

    public bool CanCollectItems() { return !(currentDay >= 2 && !hasNotebook); }

    public void TakeShockDamage(int damageAmount)
    {
        hp -= damageAmount;
        if (hp < 0) hp = 0;
        CheckDeath();
    }

    public bool OnPlayerCaught()
    {
        caughtCountThisNight++;
        if (caughtCountThisNight >= maxCaughtBeforeReset)
        {
            CaughtByNightGuard();
            return true;
        }
        return false;
    }

    public void CaughtByNightGuard()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ConfiscateAllItems();

        StartCoroutine(ShockSequenceRoutine());
    }

    IEnumerator ShockSequenceRoutine()
    {
        if (sfxSource != null && shockSound != null) sfxSource.PlayOneShot(shockSound);

        if (playerCamera != null)
        {
            Vector3 originalPos = playerCamera.localPosition;
            Quaternion originalRot = playerCamera.localRotation;
            float elapsed = 0f;
            while (elapsed < 2f)
            {
                playerCamera.localPosition = new Vector3(originalPos.x + Random.Range(-0.2f, 0.2f), originalPos.y + Random.Range(-0.2f, 0.2f), originalPos.z);
                playerCamera.localRotation = originalRot * Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            playerCamera.localPosition = originalPos;
            playerCamera.localRotation = originalRot;
        }
        else yield return new WaitForSeconds(2.0f);

        Time.timeScale = 0f;

        if (caughtPanelUI != null)
        {
            caughtPanelUI.SetActive(true);
        }
    }

    public void ClickSangNgayHomSau()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        hp = maxHp;
        AdvanceToNextDay();
    }

    public void FinishScoutingNight() { AdvanceToNextDay(); }

    public void SleepThroughNight()
    {
        hp = Mathf.Min(maxHp, hp + 10);
        AdvanceToNextDay();
    }

    public void AdvanceToNextDay()
    {
        if (currentDay == 5)
        {
            EvaluateEndings();
            return;
        }

        currentDay++;
        attemptedScamsToday = 0;
        successfulScamsToday = 0;
        consecutiveScamFails = 0;
        caughtCountThisNight = 0;
        hasTalkedToNPC = false;

        SyncCasinoData();

        if (isBlackCreditActive)
        {
            currentPhase = GamePhase.Night;
            TransitionToPhase(GamePhase.Night, true);
        }
        else
        {
            currentPhase = GamePhase.Morning;
            TransitionToPhase(GamePhase.Morning, true);
        }
    }

    private void EvaluateEndings()
    {
        int itemCount = (hasNotebook ? 1 : 0) + (hasNippers ? 1 : 0) + (hasRope ? 1 : 0) + (hasKey ? 1 : 0);
        bool failedStealth = caughtCountThisNight >= maxCaughtBeforeReset;

        if (isBlackCreditActive)
        {
            currentEnding = EndingType.BadCredit;
            SceneManager.LoadScene("EndingScene");
        }
        // Đủ 4 món và KHÔNG BỊ BẮT -> Đua xe tẩu thoát
        else if (itemCount == 4 && !failedStealth)
        {
            Debug.Log("<color=green>TRỐN THOÁT HOÀN HẢO: LUÂN CHUYỂN QUA MINIGAME ĐUA XE ĐẠP!</color>");
            SceneManager.LoadScene("EscapeCyclingScene");
        }
        // FIX Lưới trời: Lừa >= 15 người và KHÔNG CÓ món đồ nào
        else if (totalSuccessfulScamsAllDays >= policeArrestThreshold && itemCount == 0)
        {
            currentEnding = EndingType.Arrested;
            SceneManager.LoadScene("EndingScene");
        }
        // Gặp bạo loạn khi có >= 3 món
        else if (itemCount >= 3)
        {
            currentEnding = EndingType.RiotSurvivor;
            SceneManager.LoadScene("EndingScene");
        }
        else
        {
            currentEnding = EndingType.Death;
            SceneManager.LoadScene("EndingScene");
        }
    }

    private void CheckDeath()
    {
        if (hp <= 0)
        {
            hp = 0;
            currentEnding = EndingType.Death;
            SceneManager.LoadScene("EndingScene");
        }
    }

    public void TransitionToPhase(GamePhase newPhase, bool isNewDay = false)
    {
        currentPhase = newPhase;
        string sceneName = "";

        switch (currentPhase)
        {
            case GamePhase.Morning: sceneName = "ScamScreen"; break;
            case GamePhase.Noon: sceneName = "NoonCanteenScene"; break;
            case GamePhase.Afternoon: sceneName = "ScamScreen"; break;
            case GamePhase.Night:
                if (currentDay == 1) sceneName = "NightGameScreen";
                else sceneName = "NightScreen";
                break;
        }

        if (isNewDay && DayTransitionManager.instance != null)
        {
            DayTransitionManager.instance.StartTransition(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void ConfiscateAllItems()
    {
        if (currentDay >= 5)
        {
            Debug.Log("<color=yellow>ĐÊM 5: Giữ nguyên đồ vật để tính toán Ending phân nhánh!</color>");
            return;
        }

        hasNotebook = false;
        hasNippers = false;
        hasRope = false;
        hasKey = false;

        PlayerPrefs.SetInt("HasNotebook", 0);
        PlayerPrefs.SetInt("HasNippers", 0);
        PlayerPrefs.SetInt("HasRope", 0);
        PlayerPrefs.SetInt("HasKey", 0);
        PlayerPrefs.Save();
    }
}