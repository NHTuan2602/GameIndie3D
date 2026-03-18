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

    [Header("Giao diện Nút bấm (Menu Mới)")]
    public Button playButton;
    public Button exitButton;

    [Header("Giao diện Nhập Tên")]
    public GameObject nameInputPanel;
    public TMP_InputField playerNameInput;
    public Button confirmNameButton;

    [Header("Âm thanh (Audio Clips)")]
    public AudioClip clickSound; // Thay âm thanh giấy/điện thoại bằng tiếng click chuột

    private AudioSource audioSource;
    private bool isTransitioning = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (blackScreenFade != null) blackScreenFade.SetActive(false);
        if (nameInputPanel != null) nameInputPanel.SetActive(false);

        // GẮN SỰ KIỆN TỰ ĐỘNG (Không cần kéo thả OnClick ngoài Unity nữa)
        if (playButton != null) playButton.onClick.AddListener(OnClickPlayGame);
        if (exitButton != null) exitButton.onClick.AddListener(OnClickQuitGame);
        if (confirmNameButton != null) confirmNameButton.onClick.AddListener(OnConfirmName);
    }

    // ==========================================
    // SỰ KIỆN 1: BẤM NÚT "BẮT ĐẦU"
    // ==========================================
    public void OnClickPlayGame()
    {
        if (isTransitioning) return;
        PlayClickSound();

        // Bật bảng nhập tên lên
        if (nameInputPanel != null) nameInputPanel.SetActive(true);

        // Khóa 2 nút nền để người chơi không bấm lung tung
        if (playButton != null) playButton.interactable = false;
        if (exitButton != null) exitButton.interactable = false;
    }

    // ==========================================
    // SỰ KIỆN 2: BẤM NÚT "XÁC NHẬN" TÊN
    // ==========================================
    public void OnConfirmName()
    {
        PlayClickSound();

        string enteredName = playerNameInput.text.Trim();

        // Phòng thủ: Nếu bỏ trống, tự đặt tên là "Kẻ Khách"
        if (string.IsNullOrEmpty(enteredName))
        {
            enteredName = "Kẻ Khách";
        }

        // Lưu tên vào bộ nhớ
        PlayerPrefs.SetString("PlayerName", enteredName);
        PlayerPrefs.Save();

        // Tắt bảng và chuyển cảnh
        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        StartCoroutine(TransitionToCinematic());
    }

    // ==========================================
    // SỰ KIỆN 3: BẤM NÚT "THOÁT"
    // ==========================================
    public void OnClickQuitGame()
    {
        if (isTransitioning) return;
        PlayClickSound();
        Debug.Log("Đang thoát game...");
        Application.Quit();
    }

    IEnumerator TransitionToCinematic()
    {
        isTransitioning = true;
        if (blackScreenFade != null) blackScreenFade.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(cinematicSceneName);
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}