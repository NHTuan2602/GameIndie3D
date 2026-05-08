using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GamePhase { Morning, Noon, Afternoon, Night }

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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
    public int stamina = 100;
    public int maxStamina = 100;
    public bool hasAskedToContinue = false;

    [Header("Chỉ số Tiến trình & Tiền Bạc")]
    public float money = 0f;
    // ĐÃ XÓA KARMA!

    [Header("Hệ thống Lương & KPI")]
    public int currentDay = 1;
    public int maxDays = 5;
    public int attemptedScamsToday = 0;
    public int successfulScamsToday = 0;

    // ĐÃ THÊM: Biến đếm tổng số vụ lừa thành công để kích hoạt Ending Công An
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
    public bool hasNippers = false; // Đã đổi tên từ Wrench thành Nippers (Kềm)
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
        Debug.Log("<color=red>BÁO ĐỘNG ĐỎ! CHẾ ĐỘ VƯỢT NGỤC KÍCH HOẠT!</color>");
    }

    public bool StartScammingVictim(int staminaCost)
    {
        if (hp <= 0) return false;
        if (stamina >= staminaCost) stamina -= staminaCost;
        else
        {
            int deficit = staminaCost - stamina;
            stamina = 0;
            hp -= deficit;
            if (hp <= 0) { CheckDeath(); return false; }
        }
        return true;
    }

    // ĐÃ SỬA: Giữ biến dummyKarmaLost để không làm mẻ code ở các file khác đang gọi hàm này
    public void OnScamSuccess(float rawVNDEarned, int dummyKarmaLost = 0)
    {
        attemptedScamsToday++;
        successfulScamsToday++;
        totalSuccessfulScamsAllDays++; // Cộng dồn số vụ lừa hoàn hảo!
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
            int penalty = 20;
            if (stamina >= penalty) stamina -= penalty;
            else
            {
                int deficit = penalty - stamina;
                stamina = 0;
                hp -= deficit;
                CheckDeath();
            }
        }
        CheckShiftProgress();
    }

    private void CheckShiftProgress()
    {
        if (currentPhase == GamePhase.Morning && attemptedScamsToday >= 3)
            TransitionToPhase(GamePhase.Noon);
        else if (currentPhase == GamePhase.Afternoon && attemptedScamsToday >= maxAttemptsPerDay)
        {
            EndDaySummary();
            TransitionToPhase(GamePhase.Night);
        }
        else
        {
            VictimSelectionManager vsm = FindObjectOfType<VictimSelectionManager>();
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
        StartCoroutine(ShockSequenceRoutine());
    }

    IEnumerator ShockSequenceRoutine()
    {
        Debug.Log("<color=yellow>BẮT ĐẦU CHÍCH ĐIỆN...</color>");
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

        GameObject caughtPanel = GameObject.Find("CaughtPanel");
        if (caughtPanel != null) caughtPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);
    }

    public void ClickSangNgayHomSau()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        hp = maxHp;
        maxStamina = Mathf.Max(20, maxStamina - 20);
        stamina = maxStamina;
        AdvanceToNextDay();
    }

    public void FinishScoutingNight() { AdvanceToNextDay(); }

    public void SleepThroughNight()
    {
        stamina = maxStamina;
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
            TransitionToPhase(GamePhase.Night);
        }
        else
        {
            currentPhase = GamePhase.Morning;
            TransitionToPhase(GamePhase.Morning);
        }
    }

    // =================================================================
    // ĐÃ NÂNG CẤP LẠI TẤT CẢ ENDING THEO LOGIC MỚI TỐI GIẢN
    // =================================================================
    private void EvaluateEndings()
    {
        int itemCount = (hasNotebook ? 1 : 0) + (hasNippers ? 1 : 0) + (hasRope ? 1 : 0) + (hasKey ? 1 : 0);

        if (totalSuccessfulScamsAllDays >= policeArrestThreshold)
        {
            Debug.Log($"<size=120%><color=#FF00FF><b>ENDING A (LƯỚI TRỜI):</b> Lừa đảo hoàn hảo {totalSuccessfulScamsAllDays} người! Quy mô quá lớn, Cục An Ninh Mạng đã tóm cổ bạn tại biên giới!</color></size>");
        }
        else if (itemCount == 4 && !isBlackCreditActive)
        {
            Debug.Log("<size=120%><color=#00FFFF><b>TRUE ENDING:</b> Đủ 4 món Sổ, Kềm, Dây, Chìa. Mở khóa, cắt rào, đu dây tẩu thoát thành công trong đêm!</color></size>");
        }
        else if (isBlackCreditActive)
        {
            Debug.Log("<size=120%><color=red><b>BAD ENDING:</b> Bị kẹt lại do dính khoản nợ Tín dụng đen. Bọn chúng đã lôi bạn đi bán nội tạng.</color></size>");
        }
        else if (hp <= 30)
        {
            Debug.Log("<size=120%><color=#888888><b>DEATH ENDING:</b> Quá kiệt sức. Gục ngã và chết trong khu trại.</color></size>");
        }
        else
        {
            Debug.Log($"<size=120%><color=#FF0000><b>TRAPPED ENDING:</b> Chỉ có {itemCount}/4 món đồ nghề. Kẹt lại làm scammer cho chúng cả đời.</color></size>");
        }

        // Đoạn này bạn có thể gọi hàm bật các UI Panel Ending tương ứng giống như sòng bạc
    }

    private void CheckDeath()
    {
        if (hp <= 0)
        {
            hp = 0;
            Debug.Log("<size=150%><color=grey><b>GAME OVER:</b> BẠN ĐÃ KIỆT SỨC VÀ GỤC NGÃ!</color></size>");
        }
    }

    public void TransitionToPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;
        string sceneName = "";

        switch (currentPhase)
        {
            case GamePhase.Morning: sceneName = "ScamScreen"; break;
            case GamePhase.Noon: sceneName = "NoonCanteenScene"; break;
            case GamePhase.Afternoon: sceneName = "ScamScreen"; break;
            case GamePhase.Night: sceneName = "NightGameScreen"; break;
        }

        if (DayTransitionManager.instance != null)
        {
            DayTransitionManager.instance.StartTransition(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}