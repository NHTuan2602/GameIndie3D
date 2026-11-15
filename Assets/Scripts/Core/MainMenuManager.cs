using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("--- Liên kết Cốt truyện Intro ---")]
    public GameObject mainMenuCanvas;
    public GameObject introManagerObj;

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

    [Header("--- Nhãn Hiển Thị Giá Trị (Tùy chọn) ---")]
    public TextMeshProUGUI volumeLabelText;
    public TextMeshProUGUI mouseSenseLabelText;

    private CanvasGroup settingsCanvasGroup;
    private CanvasGroup menuCanvasGroup;

    void Start()
    {
        if (mainMenuCanvas != null)
        {
            menuCanvasGroup = mainMenuCanvas.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null) menuCanvasGroup = mainMenuCanvas.AddComponent<CanvasGroup>();
        }

        if (settingsPanel != null)
        {
            settingsCanvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (settingsCanvasGroup == null) settingsCanvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            settingsPanel.SetActive(false);
        }

        // Kiểm tra dữ liệu lưu để cập nhật trạng thái nút Tiếp tục
        if (btnContinue != null)
        {
            btnContinue.interactable = true;
            bool hasSave = PlayerPrefs.GetInt("Saved_HasData", 0) == 1;
            TextMeshProUGUI continueTmp = btnContinue.GetComponentInChildren<TextMeshProUGUI>();
            if (continueTmp != null && !hasSave)
            {
                continueTmp.text = "<b>CHƠI TIẾP</b> <size=70%><color=#888888>(Chưa có file lưu)</color></size>";
            }
        }

        // GẮN SỰ KIỆN CHO CÁC NÚT
        if (btnNewGame != null) btnNewGame.onClick.AddListener(StartNewGame);
        if (btnContinue != null) btnContinue.onClick.AddListener(ContinueGame);
        if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettings);
        if (btnExit != null) btnExit.onClick.AddListener(ExitGame);
        if (btnCloseSettings != null) btnCloseSettings.onClick.AddListener(CloseSettings);

        // TẢI CÀI ĐẶT
        LoadSettings();

        // Tự động thêm Juice cho toàn bộ nút bấm trong Menu
        UIButtonJuice.EnhanceAllButtonsIn(gameObject);
        if (mainMenuCanvas != null) UIButtonJuice.EnhanceAllButtonsIn(mainMenuCanvas);
        if (settingsPanel != null) UIButtonJuice.EnhanceAllButtonsIn(settingsPanel);

        // Mở chuột cho Menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
        }
    }

    void StartNewGame()
    {
        PlayerPrefs.DeleteKey("Saved_HasData");
        PlayerPrefs.Save();

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

        if (mainMenuCanvas != null)
        {
            StartCoroutine(FadeOutAndActivateIntro());
        }
        else if (introManagerObj != null)
        {
            introManagerObj.SetActive(true);
        }
    }

    IEnumerator FadeOutAndActivateIntro()
    {
        if (menuCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                menuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.2f);
                yield return null;
            }
            menuCanvasGroup.alpha = 0f;
        }

        mainMenuCanvas.SetActive(false);
        if (introManagerObj != null) introManagerObj.SetActive(true);
    }

    void ContinueGame()
    {
        if (PlayerPrefs.GetInt("Saved_HasData", 0) == 1)
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.LoadGameData();
            }
            else
            {
                Debug.LogError("Thiếu SaveManager trong Scene!");
            }
        }
        else
        {
            Debug.Log("<color=yellow>Chưa có file lưu! Khởi động game mới.</color>");
            StartNewGame();
        }
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

    void ExitGame()
    {
        Application.Quit();
    }

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
