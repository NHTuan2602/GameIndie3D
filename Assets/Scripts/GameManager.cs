using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePhase
{
    Morning,
    Noon,
    Afternoon,
    Night
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // ==========================================
    // BỔ SUNG: CHẾ ĐỘ DÀNH CHO NHÀ PHÁT TRIỂN
    // ==========================================
    [Header("Chế độ Dev (Dành cho Test)")]
    public bool enableDevMode = false;
    public int testStartDay = 1;
    public bool testHasNotebook = false;

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

    // QUAN TRỌNG ĐÃ ĐƯỢC PHỤC HỒI: HỆ THỐNG LƯỜNG & KPI BAN NGÀY
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

    [Header("Vật phẩm Vượt ngục")]
    public bool hasNotebook = false;
    public bool hasWrench = false;
    public bool hasMap = false;
    public bool hasCalledPolice = false;
    public bool hasRope = false;
    public bool hasMemento = false; // Kỷ vật
    public int collectedQuestItems = 0;
    public int requiredItemsToEscape = 3;

    [Header("Hệ thống Mạng (Đêm thám thính)")]
    public int caughtCountThisNight = 0;
    public int maxCaughtBeforeReset = 3;

    // QUAN TRỌNG ĐÃ ĐƯỢC PHỤC HỒI: BIẾN GIAO TIẾP NPC
    [Header("--- HỆ THỐNG VÒNG LẶP ---")]
    public GamePhase currentPhase = GamePhase.Morning;
    public bool hasTalkedToNPC = false;
    public bool unlockedScouting = false;

    [Header("--- HỆ THỐNG SÒNG BẠC & TÍN DỤNG ĐEN ---")]
    public int totalGambleCount = 0;
    public bool isCasinoLocked = false;
    public bool isBlackCreditActive = false;

    // QUAN TRỌNG ĐÃ ĐƯỢC PHỤC HỒI: SỰ KIỆN ĐÊM 5
    [Header("Sự kiện Đêm 5")]
    public bool isEscapeStart = false;
    public int escapeProgress = 0;
    public bool isRedAlert = false;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);

        if (enableDevMode)
        {
            currentDay = testStartDay;
            hasNotebook = testHasNotebook;
            if (SceneManager.GetActiveScene().name.Contains("Night")) currentPhase = GamePhase.Night;
        }
    }

    public void StartEscape()
    {
        isEscapeStart = true;
        isRedAlert = true;
        RenderSettings.ambientLight = Color.red;
        Debug.Log("<color=red>BÁO ĐỘNG ĐỎ! CHẾ ĐỘ VƯỢT NGỤC KÍCH HOẠT!</color>");
    }

    // ==========================================
    // HỆ THỐNG LÀM VIỆC BAN NGÀY (ĐÃ PHỤC HỒI)
    // ==========================================
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

        float myCutVND = rawVNDEarned * currentCommissionRate;
        money += myCutVND;

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
            Debug.Log("<color=red>QUẢN LÝ ĐIÊN LÊN: Lừa hụt 2 đứa liên tiếp! CHÍCH ĐIỆN PHẠT TẠI CHỖ!</color>");
            TakeShockDamage(50);
            consecutiveScamFails = 0;
        }
        else
        {
            Debug.Log("<color=yellow>QUẢN LÝ CHỬI: Mất tập trung à! Trừ 20 Thể Lực.</color>");
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
        Debug.Log("=== TỔNG KẾT CUỐI CA CHIỀU ===");
        if (successfulScamsToday >= targetKPI)
        {
            Debug.Log($"<color=green>ĐẠT KPI ({successfulScamsToday}/{targetKPI}).</color>");
            if (successfulScamsToday > targetKPI || targetKPI == 5)
            {
                if (targetKPI == 3) { targetKPI = 4; currentCommissionRate = 0.2f; }
                else if (targetKPI == 4) { targetKPI = 5; currentCommissionRate = 0.3f; }
                else if (targetKPI == 5 && successfulScamsToday == 5)
                {
                    typingDifficultyMultiplier -= 0.15f;
                    if (typingDifficultyMultiplier < 0.5f) typingDifficultyMultiplier = 0.5f;
                }
            }
        }
        else
        {
            int shortfall = targetKPI - successfulScamsToday;
            int shockDamage = shortfall * 30;
            Debug.Log($"<color=red>TRƯỢT KPI! Thiếu {shortfall} người. BỊ CHÍCH ĐIỆN!</color>");
            TakeShockDamage(shockDamage);
            targetKPI = 3;
            currentCommissionRate = 0.1f;
            typingDifficultyMultiplier = 1.0f;
        }
    }

    // ==========================================
    // HỆ THỐNG ĐÊM THÁM THÍNH & VẬT PHẨM (ĐÃ PHỤC HỒI)
    // ==========================================
    public bool CanCollectItems()
    {
        if (currentDay >= 2 && !hasNotebook) return false;
        return true;
    }

    public void TakeShockDamage(int damageAmount)
    {
        hp -= damageAmount;
        if (hp < 0) hp = 0;
        Debug.Log($"<color=red>BỊ CHÍCH ĐIỆN MẤT {damageAmount} MÁU! HP CÒN: {hp}</color>");
        CheckDeath();
    }

    public bool OnPlayerCaught()
    {
        caughtCountThisNight++;
        if (caughtCountThisNight >= maxCaughtBeforeReset)
        {
            Debug.Log("<color=red>HẾT LƯỢT: Đã bị bắt 3 lần trong đêm nay!</color>");
            CaughtByNightGuard();
            return true;
        }
        else
        {
            int remainingLives = maxCaughtBeforeReset - caughtCountThisNight;
            Debug.Log($"<color=orange>CẢNH BÁO: Còn {remainingLives} lượt dự phòng đêm nay.</color>");
            return false;
        }
    }

    public void CaughtByNightGuard()
    {
        hp = maxHp;
        maxStamina -= 20;
        if (maxStamina < 20) maxStamina = 20;
        stamina = maxStamina;

        Debug.Log("Bị bảo vệ tóm cổ! Đuổi thẳng về phòng ngủ.");
        currentPhase = GamePhase.Night;
        SceneManager.LoadScene("NightScreen");
    }

    public void FinishScoutingNight()
    {
        Debug.Log("Kết thúc thám thính đêm! Chuyển ngày...");
        AdvanceToNextDay();
    }

    public void SleepThroughNight()
    {
        stamina = maxStamina;
        hp += 10;
        if (hp > maxHp) hp = maxHp;
        AdvanceToNextDay();
    }

    // ==========================================
    // CƠ CHẾ SÒNG BẠC DỰA THEO GDD 
    // ==========================================
    public void ProcessGambling(float betAmount, bool isWin)
    {
        if (isCasinoLocked) return;

        totalGambleCount++;

        if (totalGambleCount == 10)
        {
            money = 0;
            Debug.Log("<color=red>SỰ KIỆN LẦN 10: Mất trắng tài sản!</color>");
            return;
        }

        if (isBlackCreditActive)
        {
            if (totalGambleCount == 15) { TriggerSuddenDeathCasino(); return; }
        }

        if (!isWin) money -= betAmount;
        else money += (betAmount * 1.5f);

        stamina -= 20;
        if (stamina < 0) stamina = 0;
    }

    public void AcceptBlackCredit()
    {
        isBlackCreditActive = true;
        money += 500000;
        AdvanceToNextDay();
    }

    public void RefuseBlackCredit()
    {
        isBlackCreditActive = false;
        isCasinoLocked = true;
        hp /= 2;
        maxStamina -= 30;
        stamina = maxStamina;
        AdvanceToNextDay();
    }

    private void TriggerSuddenDeathCasino()
    {
        Debug.Log("<size=150%><color=red><b>SUDDEN DEATH:</b> Bị siết nợ và đem đi bán nội tạng!</color></size>");
    }

    // ==========================================
    // VÒNG LẶP THỜI GIAN
    // ==========================================
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
        hasTalkedToNPC = false; // Reset biến giao tiếp NPC

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

    // ==========================================
    // ĐÁNH GIÁ ENDING 
    // ==========================================
    private void EvaluateEndings()
    {
        int itemCount = (hasWrench ? 1 : 0) + (hasMap ? 1 : 0) + (hasRope ? 1 : 0);
        bool canEscapeNight5 = (itemCount == 3 && karma > 20 && totalGambleCount < 10);

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
            case GamePhase.Night: sceneName = "NightScreen"; break;
        }

        if (currentPhase == GamePhase.Morning && DayTransitionManager.instance != null)
        {
            DayTransitionManager.instance.StartTransition(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}