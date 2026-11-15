using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("--- Nhãn Hiển Thị Giá Trị (Tùy chọn) ---")]
    public TextMeshProUGUI volumeLabelText;
    public TextMeshProUGUI mouseSenseLabelText;

    private bool isPaused = false;
    private CanvasGroup pauseCanvasGroup;
    private CanvasGroup settingsCanvasGroup;

    void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseCanvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (pauseCanvasGroup == null) pauseCanvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
            pauseMenuPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsCanvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (settingsCanvasGroup == null) settingsCanvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            settingsPanel.SetActive(false);
        }

        if (btnResume != null) btnResume.onClick.AddListener(ResumeGame);
        if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettings);
        if (btnExitToMenu != null) btnExitToMenu.onClick.AddListener(ExitToMainMenu);
        if (btnExitDesktop != null) btnExitDesktop.onClick.AddListener(QuitDesktop);
        if (btnCloseSettings != null) btnCloseSettings.onClick.AddListener(CloseSettings);

        // Nạp dữ liệu cài đặt
        LoadSettings();

        // Gắn hiệu ứng nảy (Juice) và phản hồi xúc giác cho toàn bộ nút menu
        UIButtonJuice.EnhanceAllButtonsIn(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(pauseCanvasGroup, 0f, 1f, 0.15f));
        }
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (pauseMenuPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(pauseCanvasGroup, 1f, 0f, 0.1f, () =>
            {
                pauseMenuPanel.SetActive(false);
            }));
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            StartCoroutine(FadeCanvasGroup(settingsCanvasGroup, 0f, 1f, 0.15f));
        }
    }

    void CloseSettings()
    {
        if (settingsPanel != null)
        {
            StartCoroutine(FadeCanvasGroup(settingsCanvasGroup, 1f, 0f, 0.1f, () =>
            {
                settingsPanel.SetActive(false);
            }));
        }
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
            mouseSenseSlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 300f);
            mouseSenseSlider.onValueChanged.AddListener(SetMouseSensitivity);
            SetMouseSensitivity(mouseSenseSlider.value);
        }
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        if (volumeLabelText != null)
        {
            volumeLabelText.text = $"Âm Lượng: <color=#00F5D4>{Mathf.RoundToInt(value * 100f)}%</color>";
        }
    }

    public void SetMouseSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        if (mouseSenseLabelText != null)
        {
            mouseSenseLabelText.text = $"Độ Nhạy Chuột: <color=#FFD166>{Mathf.RoundToInt(value)}</color>";
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, System.Action onComplete = null)
    {
        if (cg == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float timer = 0f;
        cg.alpha = from;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }

        cg.alpha = to;
        onComplete?.Invoke();
    }
}
