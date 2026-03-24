using UnityEngine;
using UnityEngine.SceneManagement;

// 1. THÊM ENUM THỜI GIAN
public enum GamePhase
{
    Morning,    // Sáng: Lừa 3 người
    Noon,       // Trưa: Ăn trưa, nói chuyện NPC
    Afternoon,  // Chiều: Lừa nốt 2 người
    Night       // Tối: Đánh bạc, thám thính, ngủ
}

public class GameManager : MonoBehaviour
{
    // --- SINGLETON: Bộ não bất tử ---
    public static GameManager instance;

    [Header("Thông tin Nhân vật")]
    public string playerName = "Tuấn";

    [Header("Chỉ số Sinh tồn")]
    public int hp = 100;
    public int maxHp = 100;
    public int stamina = 100;
    public int maxStamina = 100;
    public bool hasAskedToContinue = false;

    [Header("Chỉ số Tiến trình & Đạo đức")]
    public float money = 1000000f;
    public int karma = 100;
    public float escapeProgress = 0f;
    public bool hasCarInfo = false;
    public int gamblingAddictionLevel = 0;

    [Header("Quản lý Ngày & KPI")]
    public int currentDay = 1;
    public int maxDays = 5;
    public int attemptedScamsToday = 0;
    public int successfulScamsToday = 0;
    public int targetKPI = 3;
    public int maxAttemptsPerDay = 5;

    [Header("--- HỆ THỐNG VÒNG LẶP MỚI ---")]
    public GamePhase currentPhase = GamePhase.Morning;
    public bool hasTalkedToNPC = false;
    public bool unlockedScouting = false;
    public int gambleCountTonight = 0;
    public int collectedQuestItems = 0;
    public int requiredItemsToEscape = 3;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ====================================================================
    // 1. CHỌN 1 NẠN NHÂN ĐỂ BẮT ĐẦU LỪA ĐẢO (Giữ nguyên)
    // ====================================================================
    public bool StartScammingVictim(int staminaCost)
    {
        if (hp <= 0) return false;

        if (stamina >= staminaCost)
        {
            stamina -= staminaCost;
            Debug.Log("Trừ " + staminaCost + " Thể lực. Thể lực còn: " + stamina);
        }
        else
        {
            int deficit = staminaCost - stamina;
            stamina = 0;
            hp -= deficit;
            Debug.Log("Cạn kiệt Thể lực! Rút " + deficit + " Máu để làm việc. Máu còn: " + hp);

            if (hp <= 0)
            {
                CheckDeath();
                return false;
            }
        }
        return true;
    }

    // ====================================================================
    // 2. KẾT QUẢ MINI-GAME LỪA ĐẢO (Giữ nguyên)
    // ====================================================================
    public void OnScamSuccess(float moneyEarned, int karmaLost)
    {
        attemptedScamsToday++;
        successfulScamsToday++;
        money += moneyEarned;
        karma -= karmaLost;
        if (karma < 0) karma = 0;

        Debug.Log("Lừa THÀNH CÔNG! Tiến độ: " + attemptedScamsToday + "/" + maxAttemptsPerDay);
        CheckShiftProgress();
    }

    public void OnScamFail()
    {
        attemptedScamsToday++;
        int penalty = 20;
        if (stamina >= penalty)
        {
            stamina -= penalty;
        }
        else
        {
            int deficit = penalty - stamina;
            stamina = 0;
            hp -= deficit;
            CheckDeath();
        }

        Debug.Log("Lừa THẤT BẠI! Bị chửi. Tiến độ: " + attemptedScamsToday + "/" + maxAttemptsPerDay);
        CheckShiftProgress();
    }

    // ====================================================================
    // 3. KIỂM TRA MỐC QUAN TRỌNG TRONG CA LÀM (NÂNG CẤP VÒNG LẶP SÁNG/CHIỀU)
    // ====================================================================
    private void CheckShiftProgress()
    {
        // GÓC KHUẤT 1: CHIA NHÁNH SÁNG VÀ CHIỀU RÕ RÀNG
        if (currentPhase == GamePhase.Morning && attemptedScamsToday >= 3)
        {
            Debug.Log("==== HẾT CA SÁNG! CHUYỂN SANG NGHỈ TRƯA ====");
            TransitionToPhase(GamePhase.Noon);
        }
        else if (currentPhase == GamePhase.Afternoon && attemptedScamsToday >= maxAttemptsPerDay)
        {
            Debug.Log("==== HẾT CA CHIỀU! KẾT THÚC CÔNG VIỆC TRONG NGÀY ====");
            EndDaySummary(); // Tổng kết KPI và chích điện
            TransitionToPhase(GamePhase.Night);
        }
        else
        {
            // Gọi nạn nhân tiếp theo
            VictimSelectionManager vsm = FindObjectOfType<VictimSelectionManager>();
            if (vsm != null) vsm.GenerateVictimList();
        }
    }

    // ====================================================================
    // 4. HOẠT ĐỘNG BUỔI TRƯA & TỐI (THÊM MỚI)
    // ====================================================================
    public void TalkToNoonNPC()
    {
        if (currentPhase == GamePhase.Noon)
        {
            hasTalkedToNPC = true;
            Debug.Log("Đã hóng hớt thông tin từ NPC. Chiều vào làm tiếp!");
            // Nói xong là tự động bị lùa vào làm ca chiều
            TransitionToPhase(GamePhase.Afternoon);
        }
    }

    public void CollectQuestItem()
    {
        collectedQuestItems++;
        Debug.Log($"Nhặt được vật phẩm cốt truyện: {collectedQuestItems}/{requiredItemsToEscape}");
        // Nhặt xong tự bắt về ngủ
        SleepThroughNight();
    }

    // ====================================================================
    // 5. TỔNG KẾT NGÀY (CHÍCH ĐIỆN HOẶC THƯỞNG NÓNG) (Giữ nguyên logic)
    // ====================================================================
    public void EndDaySummary()
    {
        Debug.Log("--- TỔNG KẾT NGÀY " + currentDay + " ---");

        if (successfulScamsToday >= targetKPI)
        {
            int extraVictims = successfulScamsToday - targetKPI;
            float bonus = extraVictims * 500f;
            money += bonus;
            Debug.Log("ĐẠT KPI! Thưởng nóng: " + bonus);
        }
        else
        {
            int missingKPI = targetKPI - successfulScamsToday;
            int hpLost = missingKPI * 15;
            hp -= hpLost;
            Debug.Log("TRƯỢT KPI! Bị chích điện mất " + hpLost + " HP!");
            CheckDeath();
        }
    }

    // ====================================================================
    // 6. CHUYỂN GIAO THỜI GIAN VÀ SCENE
    // ====================================================================
    public void TransitionToPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;

        if (currentPhase == GamePhase.Noon)
        {
            // SceneManager.LoadScene("NoonCanteenScene");
        }
        else if (currentPhase == GamePhase.Afternoon)
        {
            // SceneManager.LoadScene("OfficeScene"); 
            // Nếu dùng chung 1 Scene, chỉ cần gọi UI chọn nạn nhân ra lại
        }
        else if (currentPhase == GamePhase.Night)
        {
            // SceneManager.LoadScene("NightRoomScene");
        }
    }

    public void SleepThroughNight()
    {
        stamina = maxStamina;
        Debug.Log("Đã ngủ qua đêm. Thể lực phục hồi 100%.");
        AdvanceToNextDay();
    }

    // ====================================================================
    // 7. LỰA CHỌN BAN ĐÊM & HÌNH PHẠT (ĐÃ FIX GÓC KHUẤT 3)
    // ====================================================================
    public void GoOut(float moneyCost)
    {
        // Kiểm tra số lần đi đánh bạc trong đêm
        if (gambleCountTonight >= 5)
        {
            Debug.Log("Đã chơi 5 ván, sòng bạc đóng cửa! Về ngủ đi.");
            return;
        }

        if (money >= moneyCost)
        {
            money -= moneyCost;
            stamina -= 20;
            if (stamina < 0) stamina = 0;
            gamblingAddictionLevel++;
            gambleCountTonight++; // Tăng biến đếm
            Debug.Log($"Đi chơi tốn {moneyCost}$. Lần chơi: {gambleCountTonight}/5. Mức nghiện: {gamblingAddictionLevel}");
        }
    }

    public void EatFood(bool isCanteen)
    {
        if (isCanteen) hp += 10;
        else
        {
            float foodPrice = 200f;
            if (money >= foodPrice)
            {
                money -= foodPrice;
                hp += 30;
            }
        }
        if (hp > maxHp) hp = maxHp;

        // Nếu ăn ở Canteen (buổi trưa), ăn xong tự động vào ca chiều
        if (isCanteen && currentPhase == GamePhase.Noon)
        {
            TransitionToPhase(GamePhase.Afternoon);
        }
    }

    public void CaughtByNightGuard()
    {
        Debug.Log("Bị bảo vệ phát hiện và chích điện! Ném về phòng.");
        hp = maxHp;
        maxStamina -= 20;
        if (maxStamina < 20) maxStamina = 20;
        stamina = maxStamina;

        AdvanceToNextDay();
    }

    // ====================================================================
    // 8. CHUYỂN NGÀY VÀ KIỂM TRA ENDING (ĐÊM 5)
    // ====================================================================
    public void AdvanceToNextDay()
    {
        Debug.Log($"==== KẾT THÚC NGÀY {currentDay} ====");

        // KIỂM TRA NGÀY 5
        if (currentDay == maxDays)
        {
            // Dựa vào số item lượm được lúc thám thính để quyết định
            if (collectedQuestItems >= requiredItemsToEscape)
                escapeProgress = 100f; // Đánh dấu đủ điều kiện

            Debug.Log("ĐÊM 5! Bắt đầu rẽ nhánh Kết cục...");
            // Gọi UI Chọn Quyết Định Đêm 5 ra (Trốn hay Ở lại)
            return;
        }

        // RESET CHO NGÀY MỚI (FIX GÓC KHUẤT 3)
        currentDay++;
        currentPhase = GamePhase.Morning;
        attemptedScamsToday = 0;
        successfulScamsToday = 0;
        gambleCountTonight = 0;
        hasAskedToContinue = false;

        // Mở khóa chức năng thám thính cho Đêm 2 trở đi
        if (hasTalkedToNPC && currentDay >= 2)
        {
            unlockedScouting = true;
            Debug.Log("Bạn đã mở khóa chức năng Thám Thính vào ban đêm!");
        }

        Debug.Log("Bắt đầu Ngày thứ " + currentDay + " - Ca Sáng.");
        // SceneManager.LoadScene("MorningOfficeScene");
    }

    public void CheckFinalEndings(bool choseToEscape)
    {
        if (karma <= 0)
        {
            TriggerEnding("ENDING A: Bị công an đột kích bắt giữ (Nghiệp quật).");
            return;
        }

        if (!choseToEscape)
        {
            TriggerEnding("ENDING B: Chấp nhận số phận. Đi ngủ và làm việc ở đây mãi mãi.");
            return;
        }

        if (gamblingAddictionLevel >= 2)
        {
            TriggerEnding("ENDING B (Sa ngã): Muốn trốn nhưng cơn nghiện níu chân. Mãi kẹt lại.");
        }
        else if (escapeProgress >= 100f || hasCarInfo || collectedQuestItems >= requiredItemsToEscape)
        {
            TriggerEnding("ENDING C: TRUE ENDING - Trốn thoát thành công!");
        }
        else
        {
            TriggerEnding("ENDING D: Trốn thất bại do thiếu công cụ. Bước sang NGÀY 6 ĐỊA NGỤC...");
            currentDay++;
            // SceneManager.LoadScene("Day6_HellRoute");
        }
    }

    private void CheckDeath()
    {
        if (hp <= 0)
        {
            hp = 0;
            TriggerEnding("GAME OVER: Bạn đã chết ngay trên bàn làm việc do kiệt sức/chích điện!");
        }
    }

    private void TriggerEnding(string msg)
    {
        Debug.Log(">>> " + msg + " <<<");
        // SceneManager.LoadScene("EndingScene");
    }
}