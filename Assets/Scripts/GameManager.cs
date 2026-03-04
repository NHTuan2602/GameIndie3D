using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // --- SINGLETON: Bộ não bất tử ---
    public static GameManager instance;

    [Header("Chỉ số Sinh tồn")]
    public int hp = 100;
    public int maxHp = 100;
    public int stamina = 100;
    public int maxStamina = 100;

    [Header("Chỉ số Tiến trình & Đạo đức")]
    public float money = 0f;
    public int karma = 100;
    public float escapeProgress = 0f;
    public bool hasCarInfo = false;
    public int gamblingAddictionLevel = 0; // Biến tha hóa: Số lần đi chơi/đánh bạc

    [Header("Quản lý Ngày & KPI")]
    public int currentDay = 1;
    public int maxDays = 5;
    public int attemptedScamsToday = 0;   // Số người đã tiếp cận (Thành công + Thất bại)
    public int successfulScamsToday = 0;  // Số người lừa THÀNH CÔNG (Dùng để tính KPI)
    public int targetKPI = 5;             // Cần lừa THÀNH CÔNG 5 người để không bị chích điện
    public int maxAttemptsPerDay = 10;    // Tối đa danh sách có 10 người mỗi ngày

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
    // 1. CHỌN 1 NẠN NHÂN ĐỂ BẮT ĐẦU LỪA ĐẢO
    // ====================================================================
    public bool StartScammingVictim(int staminaCost)
    {
        if (hp <= 0) return false;

        // Nếu đủ thể lực -> Trừ thể lực bình thường
        if (stamina >= staminaCost)
        {
            stamina -= staminaCost;
            Debug.Log("Trừ " + staminaCost + " Thể lực. Thể lực còn: " + stamina);
        }
        // Nếu THIẾU thể lực -> Rút sạch thể lực và LẤY MÁU BÙ VÀO
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
    // 2. KẾT QUẢ MINI-GAME LỪA ĐẢO
    // ====================================================================
    public void OnScamSuccess(float moneyEarned, int karmaLost)
    {
        attemptedScamsToday++;
        successfulScamsToday++;

        money += moneyEarned;
        karma -= karmaLost;
        if (karma < 0) karma = 0; // Đảm bảo Karma không bị âm

        Debug.Log("Lừa THÀNH CÔNG! Tiến độ: " + attemptedScamsToday + "/10. KPI đạt: " + successfulScamsToday);
        CheckShiftProgress();
    }

    public void OnScamFail()
    {
        attemptedScamsToday++;
        Debug.Log("Lừa THẤT BẠI! Bị chửi rủa. Tiến độ: " + attemptedScamsToday + "/10. KPI đạt: " + successfulScamsToday);
        CheckShiftProgress();
    }

    // ====================================================================
    // 3. KIỂM TRA MỐC QUAN TRỌNG TRONG CA LÀM
    // ====================================================================
    private void CheckShiftProgress()
    {
        if (attemptedScamsToday >= maxAttemptsPerDay)
        {
            Debug.Log("Đã hết danh sách 10 nạn nhân hôm nay! Tự động kết thúc ca làm.");
            EndDaySummary();
        }
        else if (attemptedScamsToday == 5)
        {
            Debug.Log("--- BẠN ĐĐ TIẾP CẬN 5 NGƯỜI ---");
            Debug.Log("Mở bảng UI: Bạn muốn [DỪNG CA LÀM] hay [LÀM TIẾP]?");
            // Gọi UI hiển thị ở đây
        }
    }

    // ====================================================================
    // 4. TỔNG KẾT NGÀY (CHÍCH ĐIỆN HOẶC THƯỞNG NÓNG)
    // ====================================================================
    public void EndDaySummary()
    {
        Debug.Log("--- TỔNG KẾT NGÀY " + currentDay + " ---");

        if (successfulScamsToday >= targetKPI)
        {
            int extraVictims = successfulScamsToday - targetKPI;
            float bonus = extraVictims * 500f;
            money += bonus;
            Debug.Log("ĐẠT KPI (" + successfulScamsToday + "/" + targetKPI + ")! Thưởng nóng: " + bonus);
        }
        else
        {
            int missingKPI = targetKPI - successfulScamsToday;
            int hpLost = missingKPI * 15;
            hp -= hpLost;
            Debug.Log("TRƯỢT KPI (" + successfulScamsToday + "/" + targetKPI + ")! Bị chích điện mất " + hpLost + " HP!");
            CheckDeath();
        }

        // SceneManager.LoadScene("NightMenuScene");
    }

    // ====================================================================
    // 5. LỰA CHỌN BAN ĐÊM (NGỦ, ĐI CHƠI, ĂN UỐNG) & HÌNH PHẠT
    // ====================================================================
    public void SleepThroughNight()
    {
        stamina = maxStamina;
        Debug.Log("Đã ngủ qua đêm. Thể lực phục hồi 100%.");
        AdvanceToNextDay();
    }

    public void GoOut(float moneyCost)
    {
        if (money >= moneyCost)
        {
            money -= moneyCost;
            stamina -= 20;
            if (stamina < 0) stamina = 0;
            gamblingAddictionLevel++; // Tăng mức độ tha hóa
            Debug.Log("Đi chơi tốn " + moneyCost + "$. Mức độ nghiện: " + gamblingAddictionLevel);
        }
    }

    public void EatFood(bool isCanteen)
    {
        if (isCanteen)
        {
            hp += 10;
        }
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
    }

    // Hình phạt nếu bị bảo vệ gác đêm tóm được
    public void CaughtByNightGuard()
    {
        Debug.Log("Bị bảo vệ phát hiện và chích điện! Ném về phòng.");
        hp = maxHp; // Cho tỉnh lại với mức máu đầy để ngày mai làm việc
        maxStamina -= 20; // Hình phạt: Tụt thể lực tối đa
        if (maxStamina < 20) maxStamina = 20; // Giữ mức đáy
        stamina = maxStamina;

        AdvanceToNextDay();
    }

    // ====================================================================
    // 6. CHUYỂN NGÀY VÀ KIỂM TRA ENDING (ĐÊM 5)
    // ====================================================================
    public void AdvanceToNextDay()
    {
        currentDay++;
        attemptedScamsToday = 0;
        successfulScamsToday = 0;

        if (currentDay > maxDays)
        {
            Debug.Log("ĐÊM 5! Bắt đầu rẽ nhánh Kết cục...");
            // Thực tế bạn sẽ gọi CheckFinalEndings(true/false) từ giao diện Menu chọn của Đêm 5
        }
        else
        {
            Debug.Log("Bắt đầu Ngày thứ " + currentDay);
        }
    }

    // Hàm gọi khi người chơi đưa ra quyết định ở Đêm 5
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

        // Nếu chọn Trốn Thoát:
        if (gamblingAddictionLevel >= 2)
        {
            TriggerEnding("ENDING B (Sa ngã): Muốn trốn nhưng cơn nghiện níu chân. Mãi kẹt lại.");
        }
        else if (escapeProgress >= 100f || hasCarInfo)
        {
            TriggerEnding("ENDING C: TRUE ENDING - Trốn thoát thành công!");
        }
        else
        {
            TriggerEnding("ENDING D: Trốn thất bại do thiếu công cụ. Bị bắt lại.");
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