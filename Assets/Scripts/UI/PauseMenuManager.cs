using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("--- Giao diện Menu Tạm Dừng ---")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("--- Các Nút Bấm ---")]
    public Button btnResume;
    public Button btnSettings;
    public Button btnExitToMenu;
    public Button btnExitDesktop;

    [Header("--- Điều khiển Cài Đặt ---")]
    public Slider volumeSlider;
    public Slider mouseSenseSlider;
    public Button btnCloseSettings;

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (btnResume != null) btnResume.onClick.AddListener(ResumeGame);
        if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettings);
        if (btnExitToMenu != null) btnExitToMenu.onClick.AddListener(ExitToMainMenu);
        if (btnExitDesktop != null) btnExitDesktop.onClick.AddListener(QuitDesktop);
        if (btnCloseSettings != null) btnCloseSettings.onClick.AddListener(CloseSettings);

        // Nạp dữ liệu cài đặt (Giống hệt MainMenu)
        LoadSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        PlayerPrefs.Save();
    }

    void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (SaveManager.instance != null) SaveManager.instance.AutoSaveGameData();
        SceneManager.LoadScene("MainMenu");
    }

    void QuitDesktop()
    {
        Application.Quit();
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
            // SỬA SỐ 2f thành 300f CHO ĐỒNG BỘ VỚI PLAYER MOVEMENT
            mouseSenseSlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 300f);
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