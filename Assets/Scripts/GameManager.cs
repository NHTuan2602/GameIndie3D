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

    [Header("Thông tin Nhân vật")]
    public string playerName = "Tuấn";

    [Header("Chỉ số Sinh tồn")]
    public int hp = 100;
    public int maxHp = 100;
    public int stamina = 100;
    public int maxStamina = 100;
    public bool hasAskedToContinue = false;

    // Đã xóa các biến UI để PlayerHUD lo

    [Header("Chỉ số Tiến trình & Đạo đức")]
    public float money = 0f; // Tiền này giờ là VNĐ nhé!
    public int karma = 100;
    public int gamblingAddictionLevel = 0;

    [Header("Hệ thống Lương & KPI (ĐỘNG)")]
    public int currentDay = 1;
    public int maxDays = 5;
    public int attemptedScamsToday = 0;
    public int successfulScamsToday = 0;

    // CÁC BIẾN MỚI CHO CHỨC NĂNG THĂNG CHỨC
    public int targetKPI = 3;
    public int maxAttemptsPerDay = 5;
    public float currentCommissionRate = 0.1f; // Khởi đầu hoa hồng 10%
    public float typingDifficultyMultiplier = 1.0f; // 1.0 là tốc độ gốc. Càng nhỏ đồng hồ chạy càng nhanh!
    public int exchangeRateVND = 25000; // 1 USD = 25.000 VNĐ
    public int consecutiveScamFails = 0;

    [Header("Vật phẩm Vượt ngục (Góp nhặt ban đêm)")]
    public bool hasWrench = false;
    public bool hasMap = false;
    public bool hasCalledPolice = false;
    public bool hasRope = false;
    public int collectedQuestItems = 0;
    public int requiredItemsToEscape = 3;

    [Header("--- HỆ THỐNG VÒNG LẶP ---")]
    public GamePhase currentPhase = GamePhase.Morning;
    public bool hasTalkedToNPC = false;
    public bool unlockedScouting = false;
    public int gambleCountTonight = 0;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void TakeShockDamage(int damageAmount)
    {
        hp -= damageAmount;
        if (hp < 0) hp = 0;
        Debug.Log($"<color=red>BỊ CHÍCH ĐIỆN MẤT {damageAmount} MÁU! HP CÒN: {hp}</color>");
        CheckDeath();
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
        consecutiveScamFails = 0; // Đứt chuỗi thua

        // Công thức mới: Chỉ lấy Tiền VNĐ lừa được nhân với % Hoa hồng
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
            if (vsm != null) vsm.GenerateVictimList();
        }
    }

    // ĐÃ FIX: HỆ THỐNG THĂNG CHỨC & GIÁNG CHỨC ĐỘNG
    public void EndDaySummary()
    {
        Debug.Log("=== TỔNG KẾT CUỐI CA CHIỀU ===");

        if (successfulScamsToday >= targetKPI)
        {
            Debug.Log($"<color=green>ĐẠT KPI ({successfulScamsToday}/{targetKPI}).</color>");

            // XÉT THĂNG CHỨC NẾU VƯỢT KPI (hoặc đang ở mức max)
            if (successfulScamsToday > targetKPI || targetKPI == 5)
            {
                if (targetKPI == 3)
                {
                    targetKPI = 4;
                    currentCommissionRate = 0.2f; // Tăng lên 20%
                    Debug.Log("THĂNG CHỨC! Ngày mai KPI: 4, Hoa hồng: 20%");
                }
                else if (targetKPI == 4)
                {
                    targetKPI = 5;
                    currentCommissionRate = 0.3f; // Tăng lên 30%
                    Debug.Log("THĂNG CHỨC! Ngày mai KPI: 5, Hoa hồng: 30%");
                }
                else if (targetKPI == 5 && successfulScamsToday == 5)
                {
                    // Chạm đỉnh -> Tăng độ khó gõ phím
                    typingDifficultyMultiplier -= 0.15f;
                    if (typingDifficultyMultiplier < 0.5f) typingDifficultyMultiplier = 0.5f; // Khó gấp đôi là max
                    Debug.Log("ĐẠT ĐỈNH! Tổ chức ép bạn gõ phím nhanh hơn vào ngày mai!");
                }
            }
            else
            {
                Debug.Log("Vừa đủ KPI, giữ nguyên chức vụ!");
            }
        }
        else
        {
            int shortfall = targetKPI - successfulScamsToday;
            int shockDamage = shortfall * 30; // Phạt chích điện cuối ngày

            Debug.Log($"<color=red>TRƯỢT KPI! Thiếu {shortfall} người. BỊ CHÍCH ĐIỆN VÀ GIÁNG CHỨC!</color>");
            TakeShockDamage(shockDamage);

            // GIÁNG CHỨC VỀ BAN ĐẦU
            targetKPI = 3;
            currentCommissionRate = 0.1f;
            typingDifficultyMultiplier = 1.0f;
        }
    }

    public void GoOut(float moneyCost)
    {
        if (gambleCountTonight >= 5) return;

        if (money >= moneyCost)
        {
            money -= moneyCost;
            stamina -= 20;
            if (stamina < 0) stamina = 0;

            gamblingAddictionLevel++;
            gambleCountTonight++;

            // MỚI THÊM: Nếu chơi đủ 5 lần, tự động sập nguồn chuyển ngày
            if (gambleCountTonight >= 5)
            {
                Debug.Log("Chơi đủ 5 lần! Bảo vệ sới bạc đuổi về. Chuyển sang ngày hôm sau!");
                AdvanceToNextDay();
            }
        }
    }

    public void CaughtByNightGuard()
    {
        hp = maxHp;
        maxStamina -= 20;
        if (maxStamina < 20) maxStamina = 20;
        stamina = maxStamina;
        AdvanceToNextDay();
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

    public void AdvanceToNextDay()
    {
        if (currentDay == 5)
        {
            // Kiểm tra Ending (giữ nguyên code cũ của bạn)
            return;
        }

        currentDay++;
        currentPhase = GamePhase.Morning;
        attemptedScamsToday = 0;
        successfulScamsToday = 0;
        gambleCountTonight = 0;
        hasAskedToContinue = false;

        // Kích hoạt chuyển phase
        TransitionToPhase(GamePhase.Morning);
    }

    public void TransitionToPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;
        string sceneName = "";

        // Định hướng Scene dựa trên buổi
        switch (currentPhase)
        {
            case GamePhase.Morning: sceneName = "ScamScreen"; break;
            case GamePhase.Noon: sceneName = "NoonCanteenScene"; break;
            case GamePhase.Afternoon: sceneName = "ScamScreen"; break;
            case GamePhase.Night: sceneName = "NightScreen"; break;
        }

        // LOGIC QUAN TRỌNG: Nếu vào buổi Sáng -> Gọi màn đen hiện chữ Ngày X
        if (currentPhase == GamePhase.Morning && DayTransitionManager.instance != null)
        {
            DayTransitionManager.instance.StartTransition(sceneName);
        }
        else
        {
            // Các buổi khác (Trưa, Chiều, Tối) thì đổi Scene bình thường
            SceneManager.LoadScene(sceneName);
        }
    }
    public void CheckFinalEndings() { /* Code Ending của bạn */ }
    public void TriggerDay6Riot() { /* Code Ending của bạn */ }
    private void CheckDeath() { if (hp <= 0) hp = 0; }
}