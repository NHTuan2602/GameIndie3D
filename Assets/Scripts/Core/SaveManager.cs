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
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.LeftControl))
        {
            // Bấm Ctrl + F1: Tua nhanh đến màn Đua Xe Máy Vượt Ngục (Có full đồ)
            if (Input.GetKeyDown(KeyCode.F1)) { WarpToEscapeBike(); }

            // Bấm Ctrl + F2: Tua nhanh đến Đêm Thám Thính Lén Lút (Ngày 3)
            if (Input.GetKeyDown(KeyCode.F2)) { WarpToStealthNight(3); }

            // Bấm Ctrl + F3: Tua nhanh đến Giao diện Scam Máy Tính (Ngày 1)
            if (Input.GetKeyDown(KeyCode.F3)) { WarpToScamScreen(1); }

            // =========================================================
            // MỚI: Bấm Ctrl + F4: Tua nhanh đến Mini-game Tài Xỉu
            // =========================================================
            if (Input.GetKeyDown(KeyCode.F4)) { WarpToTaiXiu(); }
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

        GameManager.instance.TransitionToPhase(GameManager.instance.currentPhase);
        return true;
    }

    // =========================================================
    // TRỤC 2: CÁC CỬA SAU WARP DỮ LIỆU ĐỂ TRÌNH BÀY VẤN ĐÁP
    // =========================================================

    public void WarpToEscapeBike()
    {
        Debug.Log("<color=orange><b>[WARP]</b> Đang nạp dữ liệu khẩn cấp để vào màn Đua xe máy...</color>");

        // ĐÃ FIX: Chỉ bơm đồ nếu GameManager thực sự tồn tại
        if (GameManager.instance != null)
        {
            SetupDebugGameManager(6, GamePhase.Night, 100, 500000);
            GameManager.instance.hasNotebook = true;
            GameManager.instance.hasNippers = true;
            GameManager.instance.hasRope = true;
            GameManager.instance.hasKey = true;
        }
        else
        {
            Debug.LogWarning("Warp không có GameManager! Bạn sẽ chuyển cảnh chay mà không có tiền/đồ.");
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("EscapeBikeScene"); // Dùng đúng tên màn đua xe
    }

    public void WarpToStealthNight(int dayNumber)
    {
        Debug.Log($"<color=orange><b>[WARP]</b> Đang tua tới Đêm stealth lén lút Ngày {dayNumber}...</color>");
        if (GameManager.instance != null) SetupDebugGameManager(dayNumber, GamePhase.Night, 100, 150000);

        Time.timeScale = 1f;
        SceneManager.LoadScene("NightStealthScene");
    }

    public void WarpToScamScreen(int dayNumber)
    {
        Debug.Log($"<color=orange><b>[WARP]</b> Đang tua tới Ca làm việc Ngày {dayNumber}...</color>");
        if (GameManager.instance != null) SetupDebugGameManager(dayNumber, GamePhase.Morning, 100, 0);

        Time.timeScale = 1f;
        SceneManager.LoadScene("ScamScreen");
    }

    // MỚI: TUA NHANH VÀO SÒNG BẠC TÀI XỈU
    public void WarpToTaiXiu()
    {
        Debug.Log("<color=orange><b>[WARP]</b> Đang tua tới Mini-game Tài Xỉu...</color>");

        // Bơm sẵn 5 triệu VNĐ cho bạn đánh bạc thả ga
        if (GameManager.instance != null) SetupDebugGameManager(2, GamePhase.Night, 100, 5000000);

        Time.timeScale = 1f;
        SceneManager.LoadScene("NightGameScreen");
    }

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