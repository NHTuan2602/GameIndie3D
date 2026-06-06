using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Update()
    {
        // =========================================================
        // HỆ THỐNG PHÍM TẮT BÍ MẬT ĐỂ BẠN TRÌNH BÀY KHI VẤN ĐÁP
        // =========================================================
        // ĐÃ FIX LỖI CHÍNH TẢ: KeyCode.LeftArrow thay vì LeftLeftArrow
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.LeftControl))
        {
            // Bấm Ctrl + F1: Tua nhanh đến màn Đua Xe Máy Vượt Ngục (Có full đồ)
            if (Input.GetKeyDown(KeyCode.F1)) { WarpToEscapeBike(); }

            // Bấm Ctrl + F2: Tua nhanh đến Đêm Thám Thính Lén Lút (Ngày 3)
            if (Input.GetKeyDown(KeyCode.F2)) { WarpToStealthNight(3); }

            // Bấm Ctrl + F3: Tua nhanh đến Giao diện Scam Máy Tính (Ngày 1)
            if (Input.GetKeyDown(KeyCode.F3)) { WarpToScamScreen(1); }
        }
    }

    // =========================================================
    // TRỤC 1: TỰ ĐỘNG LƯU GAME (GỌI KHI CHUYỂN CẢNH AN TOÀN)
    // =========================================================
    public void AutoSaveGameData()
    {
        if (GameManager.instance == null) return;

        PlayerPrefs.SetInt("Saved_CurrentDay", GameManager.instance.currentDay);
        PlayerPrefs.SetInt("Saved_CurrentPhase", (int)GameManager.instance.currentPhase);
        PlayerPrefs.SetInt("Saved_HP", GameManager.instance.hp);
        PlayerPrefs.SetFloat("Saved_Money", GameManager.instance.money);

        // Lưu trạng thái vật phẩm
        PlayerPrefs.SetInt("Saved_HasNotebook", GameManager.instance.hasNotebook ? 1 : 0);
        PlayerPrefs.SetInt("Saved_HasNippers", GameManager.instance.hasNippers ? 1 : 0);
        PlayerPrefs.SetInt("Saved_HasRope", GameManager.instance.hasRope ? 1 : 0);
        PlayerPrefs.SetInt("Saved_HasKey", GameManager.instance.hasKey ? 1 : 0);

        PlayerPrefs.SetInt("Saved_TotalScams", GameManager.instance.totalSuccessfulScamsAllDays);
        PlayerPrefs.SetInt("Saved_HasData", 1); // Đánh dấu là đã có file lưu
        PlayerPrefs.Save();

        Debug.Log("<color=cyan><b>[Auto-Save]</b> Game đã tự động lưu thành công tại điểm an toàn!</color>");
    }

    // Hàm gọi từ nút "TIẾP TỤC" ngoài Main Menu
    public bool LoadGameData()
    {
        if (PlayerPrefs.GetInt("Saved_HasData", 0) == 0 || GameManager.instance == null) return false;

        Time.timeScale = 1f;
        GameManager.instance.currentDay = PlayerPrefs.GetInt("Saved_CurrentDay");
        GameManager.instance.currentPhase = (GamePhase)PlayerPrefs.GetInt("Saved_CurrentPhase");
        GameManager.instance.hp = PlayerPrefs.GetInt("Saved_HP");
        GameManager.instance.money = PlayerPrefs.GetFloat("Saved_Money");

        GameManager.instance.hasNotebook = PlayerPrefs.GetInt("Saved_HasNotebook") == 1;
        GameManager.instance.hasNippers = PlayerPrefs.GetInt("Saved_HasNippers") == 1;
        GameManager.instance.hasRope = PlayerPrefs.GetInt("Saved_HasRope") == 1;
        GameManager.instance.hasKey = PlayerPrefs.GetInt("Saved_HasKey") == 1;

        GameManager.instance.totalSuccessfulScamsAllDays = PlayerPrefs.GetInt("Saved_TotalScams");

        // Đẩy người chơi vào đúng phân đoạn đã lưu
        GameManager.instance.TransitionToPhase(GameManager.instance.currentPhase);
        return true;
    }

    // =========================================================
    // TRỤC 2: CÁC CỬA SAU WARP DỮ LIỆU ĐỂ TRÌNH BÀY VẤN ĐÁP
    // =========================================================

    // 1. Tua nhanh tới màn đua xe máy
    public void WarpToEscapeBike()
    {
        Debug.Log("<color=orange><b>[WARP]</b> Đang nạp dữ liệu khẩn cấp để vào màn Đua xe máy...</color>");
        SetupDebugGameManager(6, GamePhase.Night, 100, 500000);

        // Ép có đủ 4 món đồ để kích hoạt True Ending khi thắng màn đua xe
        GameManager.instance.hasNotebook = true;
        GameManager.instance.hasNippers = true;
        GameManager.instance.hasRope = true;
        GameManager.instance.hasKey = true;

        Time.timeScale = 1f;
        SceneManager.LoadScene("EscapeCyclingScene");
    }

    // 2. Tua nhanh tới đêm thám thính lén lút
    public void WarpToStealthNight(int dayNumber)
    {
        Debug.Log($"<color=orange><b>[WARP]</b> Đang tua tới Đêm stealth lén lút Ngày {dayNumber}...</color>");
        SetupDebugGameManager(dayNumber, GamePhase.Night, 100, 150000);

        Time.timeScale = 1f;
        SceneManager.LoadScene("NightScreen"); // Tên Scene phòng ngủ 3D ban đêm của bạn
    }

    // 3. Tua nhanh tới màn hình làm việc gõ phím lừa đảo
    public void WarpToScamScreen(int dayNumber)
    {
        Debug.Log($"<color=orange><b>[WARP]</b> Đang tua tới Ca làm việc Ngày {dayNumber}...</color>");
        SetupDebugGameManager(dayNumber, GamePhase.Morning, 100, 0);

        Time.timeScale = 1f;
        SceneManager.LoadScene("ScamScreen");
    }

    // Hàm phụ trợ bơm dữ liệu sạch vào GameManager để tránh crash
    private void SetupDebugGameManager(int day, GamePhase phase, int hp, float money)
    {
        if (GameManager.instance == null) return;
        GameManager.instance.currentDay = day;
        GameManager.instance.currentPhase = phase;
        GameManager.instance.hp = hp;
        GameManager.instance.money = money;
        GameManager.instance.caughtCountThisNight = 0;
        GameManager.instance.attemptedScamsToday = 0;
        GameManager.instance.successfulScamsToday = 0;
    }
}