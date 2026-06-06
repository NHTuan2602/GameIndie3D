using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("--- Liên kết Cốt truyện Intro ---")]
    public GameObject mainMenuCanvas;   // Kéo cái MainMenuCanvas chứa các nút vào đây
    public GameObject introManagerObj;  // Kéo cái GameObject IntroManager vào đây

    [Header("--- Nút Menu Chính ---")]
    public Button btnNewGame;
    public Button btnContinue;
    public Button btnSettings;
    public Button btnExit;

    [Header("--- Panel Cài Đặt ---")]
    public GameObject settingsPanel;
    public Slider volumeSlider;
    public Slider mouseSenseSlider;
    public Button btnCloseSettings;

    void Start()
    {
        // ÉP NÚT "CHƠI TIẾP" LUÔN SÁNG ĐỂ BẤM ĐƯỢC TRONG MỌI TRƯỜNG HỢP
        if (btnContinue != null)
        {
            btnContinue.interactable = true;
        }

        // GẮN SỰ KIỆN CHO CÁC NÚT VÀO HỆ THỐNG LIÊN KẾT
        if (btnNewGame != null) btnNewGame.onClick.AddListener(StartNewGame);
        if (btnContinue != null) btnContinue.onClick.AddListener(ContinueGame);
        if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettings);
        if (btnExit != null) btnExit.onClick.AddListener(ExitGame);
        if (btnCloseSettings != null) btnCloseSettings.onClick.AddListener(CloseSettings);

        // TẢI CÀI ĐẶT TỪ LẦN CHƠI TRƯỚC
        LoadSettings();
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Hiện chuột ra sảnh chờ
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void StartNewGame()
    {
        // 1. XÓA SẠCH DỮ LIỆU LƯU CŨ ĐỂ KHÔNG BỊ TRÙNG LẶP
        PlayerPrefs.DeleteKey("Saved_HasData");
        PlayerPrefs.Save();

        // 2. RESET TRẠNG THÁI GAME MANAGER VỀ CON SỐ 0
        if (GameManager.instance != null)
        {
            GameManager.instance.currentDay = 1;
            GameManager.instance.currentPhase = GamePhase.Morning;
            GameManager.instance.hp = GameManager.instance.maxHp;
            GameManager.instance.money = 0f;

            GameManager.instance.hasNotebook = false;
            GameManager.instance.hasNippers = false;
            GameManager.instance.hasRope = false;
            GameManager.instance.hasKey = false;

            GameManager.instance.totalSuccessfulScamsAllDays = 0;
        }

        // 3. KHÔNG DÙNG LOAD SCENE NỮA! BẬT KỊCH BẢN INTRO LÊN
        // Giấu các nút bấm Bắt đầu, Chơi tiếp, Cài đặt... đi
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);

        // Đánh thức script IntroManager dậy để nó bắt đầu kêu người chơi Nhập tên
        if (introManagerObj != null) introManagerObj.SetActive(true);
    }

    void ContinueGame()
    {
        // RẼ NHÁNH XỬ LÝ THÔNG MINH KHI BẤM NÚT CHƠI TIẾP
        if (PlayerPrefs.GetInt("Saved_HasData", 0) == 1)
        {
            // TRƯỜNG HỢP A: CÓ DỮ LIỆU -> Tải lại phân đoạn cũ đang chơi dở
            if (SaveManager.instance != null)
            {
                SaveManager.instance.LoadGameData();
            }
            else
            {
                Debug.LogError("Bị thiếu cấu trúc SaveManager trong Scene này rồi!");
            }
        }
        else
        {
            // TRƯỜNG HỢP B: KHÔNG CÓ DỮ LIỆU -> Chạy như nút Bắt Đầu Mới
            Debug.Log("<color=yellow><b>[Menu]</b> Chưa có file lưu! Tự động kích hoạt luồng Chơi Mới.</color>");
            StartNewGame();
        }
    }

    void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        PlayerPrefs.Save(); // Lưu cứng thiết lập vào ổ cứng khi đóng bảng
    }

    void ExitGame()
    {
        Application.Quit();
        Debug.Log("Đã thoát game!");
    }

    // ==========================================
    // HỆ THỐNG CÀI ĐẶT (ÂM THANH & CHUỘT)
    // ==========================================
    void LoadSettings()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.onValueChanged.AddListener(SetVolume);
            SetVolume(volumeSlider.value);
        }

        if (mouseSenseSlider != null)
        {
            mouseSenseSlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
            mouseSenseSlider.onValueChanged.AddListener(SetMouseSensitivity);
        }
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMouseSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }
}