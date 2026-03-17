using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Cài đặt Chuyển Cảnh")]
    public string cinematicSceneName = "CinematicIntro";
    public GameObject blackScreenFade;

    [Header("Bảng tin nhắn (Điện thoại)")]
    public TextMeshProUGUI phoneMessageText;
    public Button quitGameButton;

    [Header("Giấy nhận việc")]
    public TextMeshProUGUI paperTitleText;
    public TextMeshProUGUI paperInstructionText;
    public Button startGameButton;

    [Header("Giao diện Nhập Tên")]
    public GameObject nameInputPanel; // Kéo Panel chứa bảng nhập tên vào đây
    public TMP_InputField playerNameInput; // Kéo ô nhập chữ (InputField) vào đây
    public Button confirmNameButton; // Kéo nút Xác nhận vào đây

    [Header("Âm thanh (Audio Clips)")]
    public AudioClip paperCrumpleSound;
    public AudioClip phoneVibrateSound;
    public AudioClip phoneRingSound;

    private AudioSource audioSource;
    private bool isTransitioning = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (blackScreenFade != null) blackScreenFade.SetActive(false);

        // Tắt bảng nhập tên lúc mới vào game
        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        phoneMessageText.gameObject.SetActive(false);

        StartCoroutine(StartMenuSequence());
    }

    IEnumerator StartMenuSequence()
    {
        yield return new WaitForSeconds(2f);
        if (phoneRingSound != null) audioSource.PlayOneShot(phoneRingSound);
        if (phoneVibrateSound != null) audioSource.PlayOneShot(phoneVibrateSound);
        phoneMessageText.gameObject.SetActive(true);
        phoneMessageText.color = Color.green;
    }

    // ==========================================
    // SỰ KIỆN 1: BẤM VÀO TỜ GIẤY CHỐT ĐƠN
    // ==========================================
    public void OnClickStartGame()
    {
        if (isTransitioning) return;

        // Bật bảng nhập tên lên
        nameInputPanel.SetActive(true);

        // Vô hiệu hóa tờ giấy và điện thoại để người chơi không bấm linh tinh nữa
        startGameButton.interactable = false;
        quitGameButton.interactable = false;
    }

    // ==========================================
    // SỰ KIỆN 2: BẤM NÚT XÁC NHẬN TÊN
    // ==========================================
    public void OnConfirmName()
    {
        string enteredName = playerNameInput.text.Trim();

        // Phòng thủ: Nếu người chơi bỏ trống, tự đặt tên là "Kẻ Khách"
        if (string.IsNullOrEmpty(enteredName))
        {
            enteredName = "Kẻ Khách";
        }

        // LƯU TÊN VÀO BỘ NHỚ XUYÊN KHÔNG GIAN (PlayerPrefs)
        PlayerPrefs.SetString("PlayerName", enteredName);
        PlayerPrefs.Save(); // Chốt sổ lưu lại

        // Tắt bảng nhập tên và bắt đầu chuyển cảnh
        nameInputPanel.SetActive(false);
        StartCoroutine(TransitionToCinematic());
    }

    public void OnClickQuitGame()
    {
        Application.Quit();
    }

    IEnumerator TransitionToCinematic()
    {
        isTransitioning = true;
        if (blackScreenFade != null) blackScreenFade.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(cinematicSceneName);
    }

    // Các hàm Hover chuột giữ nguyên
    public void OnPointerEnterPaper()
    {
        if (isTransitioning) return;
        paperInstructionText.text = "> CLICK ĐỂ NHẬN VIỆC <";
        paperInstructionText.color = Color.yellow;
        if (paperCrumpleSound != null) audioSource.PlayOneShot(paperCrumpleSound, 0.5f);
    }
    public void OnPointerExitPaper()
    {
        if (isTransitioning) return;
        paperInstructionText.text = "[CLICK ĐỂ NHẬN VIỆC]";
        paperInstructionText.color = Color.white;
    }
    public void OnPointerEnterPhone()
    {
        if (isTransitioning) return;
        quitGameButton.image.color = new Color(0, 1, 0, 0.5f);
        if (phoneVibrateSound != null) audioSource.PlayOneShot(phoneVibrateSound, 0.3f);
    }
    public void OnPointerExitPhone()
    {
        if (isTransitioning) return;
        quitGameButton.image.color = new Color(0, 0, 0, 0f);
    }
}