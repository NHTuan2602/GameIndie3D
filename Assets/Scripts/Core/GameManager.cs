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

    [Header("Chỉ số Tiến trình & Đạo đức")]
    public float money = 0f;
    public int karma = 100;

    [Header("Hệ thống Lương & KPI")]
    public int currentDay = 1;
    public int maxDays = 5;
    public int attemptedScamsToday = 0;
    public int successfulScamsToday = 0;
    public int targetKPI = 3;
    public int maxAttemptsPerDay = 5;
    public float currentCommissionRate = 0.1f;
    public float typingDifficultyMultiplier = 1.0f;
    public int exchangeRateVND = 25000;
    public int consecutiveScamFails = 0;

    [Header("Vật phẩm Vượt ngục (Dữ liệu thật)")]
    public bool hasNotebook = false;
    public bool hasWrench = false;
    public bool hasMap = false;
    public bool hasCalledPolice = false;
    public bool hasRope = false;
    public bool hasKey = false;
    public bool hasMemento = false;
    public int collectedQuestItems = 0;
    public int requiredItemsToEscape = 3;

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
        else { Destroy(gameObject); return; } // Phải có return để ngắt luồng ngay lập tức

        if (enableDevMode)
        {
            Debug.Log("<color=magenta>--- DEV MODE KÍCH HOẠT: BƠM FULL ĐỒ VÀO TÚI ---</color>");
            currentDay = testStartDay;
            currentPhase = testStartPhase;
            hasNotebook = testHasNotebook;
            hasRope = testHasRope;
            hasWrench = testHasNippers;
            hasKey = testHasKey;
            if (SceneManager.GetActiveScene().name.Contains("Night")) currentPhase = GamePhase.Night;
        }

        // ĐỒNG BỘ DỮ LIỆU TÍN DỤNG ĐEN TỪ SÒNG BẠC NGAY KHI MỞ GAME
        SyncCasinoData();
    }

    // Hàm gọi để đồng bộ với TaiXiuManager
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

    public void OnScamSuccess(float rawVNDEarned, int karmaLost)
    {
        attemptedScamsToday++;
        successfulScamsToday++;
        consecutiveScamFails = 0;
        money += (rawVNDEarned * currentCommissionRate);
        karma -= karmaLost;
        if (karma < 0) karma = 0;
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

        // VÁ LỖI TÌM LẠI UI ĐỂ TRÁNH NULL
        GameObject caughtPanel = GameObject.Find("CaughtPanel"); // Phải đảm bảo Canvas có Panel tên y hệt thế này
        if (caughtPanel != null) caughtPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);
        // Sau 2 giây hiện nút ra (Tùy logic UI của bạn)
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

        // ĐỒNG BỘ LẠI TRƯỚC KHI CHUYỂN NGÀY
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

    private void EvaluateEndings()
    {
        int itemCount = (hasWrench ? 1 : 0) + (hasMap ? 1 : 0) + (hasRope ? 1 : 0);

        // Đã bỏ biến totalGambleCount trong này vì quản lý bên kia rồi, chỉ cần check ko bị giang hồ xiết nợ
        bool canEscapeNight5 = (itemCount == 3 && karma > 20 && !isBlackCreditActive);

        if (canEscapeNight5)
        {
            if (hasCalledPolice && hasMemento)
                Debug.Log("<size=120%><color=#00FFFF><b>TRUE ENDING:</b> Đã gọi Cảnh sát và trả Kỷ vật! Bạn được giải cứu an toàn.</color></size>");
            else
                Debug.Log("<size=120%><color=#00FF00><b>NEUTRAL ENDING:</b> Tẩu thoát thành công nhưng lẩn trốn trong rừng.</color></size>");
        }
        else
        {
            currentDay = 6;
            if (karma <= 20) Debug.Log("<size=120%><color=#FF00FF><b>ENDING A (Lưới Trời):</b> 1 năm sau bị Công an VN bắt tại sân bay.</color></size>");
            else if (hp < 30) Debug.Log("<size=120%><color=#888888><b>DEATH ENDING:</b> Tử vong do dẫm đạp trong bạo loạn.</color></size>");
            else if (itemCount >= 2) Debug.Log("<size=120%><color=#FFFF00><b>RIOT SURVIVOR:</b> Lợi dụng bạo loạn, nhảy sông bơi thoát thân.</color></size>");
            else Debug.Log("<size=120%><color=#FF0000><b>TRAPPED ENDING:</b> Bị bắt lại bán xuống hầm mỏ.</color></size>");
        }
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

            // ĐÃ SỬA LẠI THÀNH TÊN ĐÚNG THEO ẢNH BẠN CHỤP LÀ NightGameScreen
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