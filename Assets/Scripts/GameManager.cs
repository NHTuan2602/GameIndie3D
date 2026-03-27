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
        // KIỂM TRA ĐIỀU KIỆN KẾT THÚC GAME VÀO CUỐI ĐÊM NGÀY 5
        if (currentDay == 5)
        {
            EvaluateEndings();
            return;
        }

        // Nếu chưa tới ngày 5 thì qua ngày bình thường
        currentDay++;
        currentPhase = GamePhase.Morning;
        attemptedScamsToday = 0;
        successfulScamsToday = 0;
        gambleCountTonight = 0;
        consecutiveScamFails = 0; // Chắc chắn đã reset chuỗi thua
        hasAskedToContinue = false;

        // Kích hoạt chuyển phase (Tự động gọi màn hình đen)
        TransitionToPhase(GamePhase.Morning);
    }

    // =======================================================
    // HỆ THỐNG PHÂN NHÁNH CÁI KẾT (MULTI-ENDINGS)
    // =======================================================
    private void EvaluateEndings()
    {
        // 1. Đếm số lượng vật phẩm cốt lõi đã có
        int itemCount = (hasWrench ? 1 : 0) + (hasMap ? 1 : 0) + (hasRope ? 1 : 0);

        // 2. Điều kiện thoát đêm 5: Đủ 3 món + Nghiệp tốt (>20) + Không nghiện nặng (<10)
        bool canEscapeNight5 = (itemCount == 3 && karma > 20 && gamblingAddictionLevel < 10);

        // ==========================================
        // NHÁNH 1: TẨU THOÁT ĐÊM 5
        // ==========================================
        if (canEscapeNight5)
        {
            if (hasCalledPolice)
            {
                Debug.Log("<size=120%><color=#00FFFF><b>TRUE ENDING (Bình Minh Tự Do):</b> Đã gọi Cảnh sát! Dùng cờ lê cắt xích, đu dây qua tường. Vừa lết tới biên giới, Cảnh sát VN đã đợi sẵn. Bạn được giải cứu an toàn.</color></size>");
                // SceneManager.LoadScene("TrueEndingScene");
            }
            else
            {
                Debug.Log("<size=120%><color=#00FF00><b>NEUTRAL ENDING (Kẻ Sống Sót):</b> Không gọi Cảnh sát. Bạn tẩu thoát thành công, nhưng phải lẩn trốn trong rừng thiêng nước độc 3 ngày, đói khát kiệt quệ mới lết về cửa khẩu.</color></size>");
                // SceneManager.LoadScene("NeutralEndingScene");
            }
        }
        // ==========================================
        // NHÁNH 2: ĐỊA NGỤC BẠO LOẠN SÁNG NGÀY 6
        // ==========================================
        else
        {
            currentDay = 6; // Chuyển sang ngày định mệnh

            // Phân loại kết cục theo thứ tự ưu tiên (Tình trạng nào nặng nhất thì bị xét trước)
            if (hp < 30)
            {
                Debug.Log("<size=120%><color=#888888><b>DEATH ENDING (Chết Trong Hỗn Loạn):</b> Cơ thể kiệt quệ (HP < 30). Bạn không còn sức để chạy, bị đám đông dẫm đạp và bảo vệ chích điện tới tắt thở.</color></size>");
                // SceneManager.LoadScene("DeathEndingScene");
            }
            else if (karma <= 0)
            {
                Debug.Log("<size=120%><color=#FF00FF><b>ENDING A (Lưới Trời Lồng Lộng):</b> Nghiệp cực thấp. Bạn đứng nhìn bạo loạn. Được thăng chức Quản lý. 1 năm sau gom tiền về quê ăn Tết thì bị Cảnh sát hình sự phục kích tại sân bay.</color></size>");
                // SceneManager.LoadScene("EndingAScene");
            }
            else if (gamblingAddictionLevel >= 10)
            {
                Debug.Log("<size=120%><color=#FFA500><b>ENDING B (Con Bạc Khát Nước):</b> Nghiện cờ bạc chạm đỉnh. Giữa lúc cháy nổ, bạn lao thẳng vào Sới Bạc. Gánh nợ khổng lồ, vĩnh viễn làm cỗ máy lừa đảo vô hồn để trả nợ.</color></size>");
                // SceneManager.LoadScene("EndingBScene");
            }
            else if (itemCount >= 2)
            {
                Debug.Log("<size=120%><color=#FFFF00><b>RIOT SURVIVOR (Thoát Xác Trong Máu):</b> Nhặt được " + itemCount + " món đồ. Lợi dụng bạo loạn, bạn đập kính cửa sổ, lao xuống sông và bơi thoát thân trong tàn tạ.</color></size>");
                // SceneManager.LoadScene("RiotSurvivorScene");
            }
            else
            {
                Debug.Log("<size=120%><color=#FF0000><b>TRAPPED ENDING (Tận Cùng Bi Kịch):</b> Không có đồ, núp gầm bàn. Khi bạo loạn bị dập, bạn bị lôi ra, trùm bao bố và quăng lên xe chuyển đến hầm mỏ khai thác nội tạng.</color></size>");
                // SceneManager.LoadScene("TrappedEndingScene");
            }
        }
    }

    private void CheckDeath()
    {
        if (hp <= 0)
        {
            hp = 0;
            Debug.Log("<size=150%><color=grey><b>GAME OVER:</b> BẠN ĐÃ KIỆT SỨC VÀ GỤC NGÃ NGAY TẠI BÀN LÀM VIỆC!</color></size>");
            // SceneManager.LoadScene("GameOverScene");
        }
    }

    public void TransitionToPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;
        string sceneName = "";

        // BƯỚC 1: KIỂM TRA LẠI TÊN SCENE CHUẨN XÁC
        // (Bạn hãy nhìn vào bảng Build Settings, chữ viết hoa/thường phải y hệt thế này)
        switch (currentPhase)
        {
            case GamePhase.Morning: sceneName = "ScamScreen"; break;
            case GamePhase.Noon: sceneName = "NoonCanteenScene"; break;
            case GamePhase.Afternoon: sceneName = "ScamScreen"; break;
            case GamePhase.Night: sceneName = "NightScreen"; break;
        }

        // BƯỚC 2: BẮT MẠCH MÀN HÌNH ĐEN
        if (currentPhase == GamePhase.Morning)
        {
            if (DayTransitionManager.instance != null)
            {
                Debug.Log("<color=cyan>ĐÃ TÌM THẤY MÀN ĐEN! Đang kéo rèm chuyển ngày sang: " + sceneName + "</color>");
                DayTransitionManager.instance.StartTransition(sceneName);
            }
            else
            {
                Debug.LogError("<color=red>LỖI CHÍ MẠNG: Không tìm thấy DayTransitionManager! Đã có lỗi xảy ra với FadePanel. Game sẽ bỏ qua màn đen và nhảy thẳng!</color>");
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }



}