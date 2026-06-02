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

    [Header("Chỉ số Sinh tồn")]
    public int hp = 100;
    public int maxHp = 100;
    public bool hasAskedToContinue = false;

    [Header("Chỉ số Tiến trình & Tiền Bạc")]
    public float money = 0f;

    [Header("Hệ thống Lương & KPI")]
    public int currentDay = 1;
    public int maxDays = 6;
    public int attemptedScamsToday = 0;
    public int successfulScamsToday = 0;

    public int totalSuccessfulScamsAllDays = 0;
    public int policeArrestThreshold = 15;

    // ĐÃ FIX: Giảm KPI khởi điểm xuống 2
    public int targetKPI = 2;
    public int maxAttemptsPerDay = 5;
    public float currentCommissionRate = 0.1f;
    public float typingDifficultyMultiplier = 1.0f;
    public int exchangeRateVND = 25000;
    public int consecutiveScamFails = 0;

    [Header("Vật phẩm Vượt ngục")]
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

    [Header("Sự kiện Đêm 5/6")]
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
        if (hp <= 0 && currentDay < 6) { CheckDeath(); return false; }
        return true;
    }

    public void OnScamSuccess(float rawVNDEarned, int dummyKarmaLost = 0)
    {
        if (hp <= 0 && currentDay < 6) return;

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

        // ĐÃ FIX: XÓA CHÍCH ĐIỆN 50 HP TỨC TƯỞI GIỮA NGÀY.
        // Chỉ phạt cảnh cáo 5 HP (Bị mắng chửi) để người chơi vẫn sống và làm cho hết 5 ca.
        TakeShockDamage(5);

        if (hp > 0 || currentDay >= 6) CheckShiftProgress();
    }

    private void CheckShiftProgress()
    {
        if (hp <= 0 && currentDay < 6) return;

        if (currentPhase == GamePhase.Morning && attemptedScamsToday >= 3)
            TransitionToPhase(GamePhase.Noon);
        else if (currentPhase == GamePhase.Afternoon && attemptedScamsToday >= maxAttemptsPerDay)
        {
            ShiftSummaryUI summaryUI = FindFirstObjectByType<ShiftSummaryUI>(FindObjectsInactive.Include);

            if (summaryUI != null) summaryUI.ShowForceEndShift();
            else
            {
                EndDaySummary();
                if (hp > 0 || currentDay >= 6) TransitionToPhase(GamePhase.Night);
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
            // ĐẠT KPI: Dụ dỗ người chơi lừa thêm bằng cách tăng lương và tăng chỉ tiêu
            targetKPI++;
            if (targetKPI > 5) targetKPI = 5; // Tối đa yêu cầu 5/5

            currentCommissionRate += 0.05f; // Tăng % hoa hồng nhận được
            typingDifficultyMultiplier = Mathf.Max(0.5f, typingDifficultyMultiplier - 0.1f);
        }
        else
        {
            // KHÔNG ĐẠT KPI: Đây mới là lúc Ban quản lý lôi ra chích điện phạt nặng!
            int shockDamage = (targetKPI - successfulScamsToday) * 30;
            TakeShockDamage(shockDamage);

            // Đưa KPI về lại 2 để ngày hôm sau dễ sống hơn
            targetKPI = 2;
            currentCommissionRate = 0.1f;
            typingDifficultyMultiplier = 1.0f;
        }
    }

    // ========================================================
    // ĐÃ FIX: HÀM DỌN DẸP DỮ LIỆU ĐỂ BẤM NÚT CHƠI LẠI TRƠN TRU
    // ========================================================
    public void ResetDayForRetry()
    {
        hp = maxHp;
        currentEnding = EndingType.None;
        attemptedScamsToday = 0;
        successfulScamsToday = 0;
        consecutiveScamFails = 0;
        caughtCountThisNight = 0;
        // KHÔNG RESET currentDay để người chơi làm lại đúng ngày đó
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

        if (caughtPanelUI != null) caughtPanelUI.SetActive(true);
    }

    public void ClickSangNgayHomSau()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentDay < maxDays - 1)
        {
            hp = maxHp;
        }

        AdvanceToNextDay();
    }

    public void FinishScoutingNight() { AdvanceToNextDay(); }

    public void SleepThroughNight()
    {
        if (currentDay < maxDays - 1)
        {
            hp = Mathf.Min(maxHp, hp + 10);
        }
        AdvanceToNextDay();
    }

    public void AdvanceToNextDay()
    {
        if (currentDay >= maxDays - 1)
        {
            currentDay = 6;
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

    private void TriggerEndingTransition(EndingType ending)
    {
        currentEnding = ending;
        if (DayTransitionManager.instance != null)
        {
            DayTransitionManager.instance.StartTransition("EndingScene");
        }
        else
        {
            SceneManager.LoadScene("EndingScene");
        }
    }

    private void EvaluateEndings()
    {
        int itemCount = (hasNotebook ? 1 : 0) + (hasNippers ? 1 : 0) + (hasRope ? 1 : 0) + (hasKey ? 1 : 0);
        bool failedStealth = caughtCountThisNight >= maxCaughtBeforeReset;

        if (hp < 20)
        {
            TriggerEndingTransition(EndingType.Death);
            return;
        }

        if (isBlackCreditActive)
        {
            TriggerEndingTransition(EndingType.BadCredit);
        }
        else if (itemCount == 4 && !failedStealth)
        {
            currentEnding = EndingType.TrueEscape;
            SceneManager.LoadScene("EscapeCyclingScene");
        }
        else if (itemCount >= 3)
        {
            TriggerEndingTransition(EndingType.RiotSurvivor);
        }
        else if (totalSuccessfulScamsAllDays >= policeArrestThreshold && itemCount == 0)
        {
            TriggerEndingTransition(EndingType.Arrested);
        }
        else
        {
            TriggerEndingTransition(EndingType.Death);
        }
    }

    private void CheckDeath()
    {
        if (hp <= 0 && currentDay < 6)
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
                // ========================================================
                // ĐÃ FIX: TRẢ NGƯỜI CHƠI VỀ PHÒNG NGỦ 3D VÀO ĐÊM 1
                // ========================================================
                if (isBlackCreditActive)
                    sceneName = "NightGameScreen"; // Bị siết nợ thì ném thẳng vào sới bạc
                else
                    sceneName = "NightScreen"; // Bình thường thì luôn về phòng ngủ 3D trước
                break;
        }

        if (isNewDay && DayTransitionManager.instance != null) DayTransitionManager.instance.StartTransition(sceneName);
        else SceneManager.LoadScene(sceneName);
    }

    public void ConfiscateAllItems()
    {
        if (currentDay >= 5) return;

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