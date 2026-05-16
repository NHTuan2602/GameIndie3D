using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Cài đặt Chuyển Cảnh")]
    public string cinematicSceneName = "";
    public GameObject blackScreenFade;

    [Header("Giao diện Nút bấm (Menu Mới)")]
    public Button playButton;
    public Button exitButton;
    public GameObject titleText; // GÓC KHUẤT: Khai báo thêm chữ Tiêu đề để tắt cho sạch

    [Header("Giao diện Nhập Tên")]
    public GameObject nameInputPanel;
    public TMP_InputField playerNameInput;
    public Button confirmNameButton;

    [Header("Âm thanh (Audio Clips)")]
    public AudioClip clickSound;

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

        // GẮN SỰ KIỆN TỰ ĐỘNG
        if (playButton != null) playButton.onClick.AddListener(OnClickPlayGame);
        if (exitButton != null) exitButton.onClick.AddListener(OnClickQuitGame);
        //if (confirmNameButton != null) confirmNameButton.onClick.AddListener(OnConfirmName);
    }

    // ==========================================
    // SỰ KIỆN 1: BẤM NÚT "BẮT ĐẦU"
    // ==========================================
    public void OnClickPlayGame()
    {
        if (isTransitioning) return;
        PlayClickSound();

        // 1. Bật bảng nhập tên lên
        if (nameInputPanel != null) nameInputPanel.SetActive(true);

        // 2. FIX LỖI Ở ĐÂY: Dùng gameObject.SetActive(false) để làm BIẾN MẤT hoàn toàn
        if (playButton != null) playButton.gameObject.SetActive(false);
        if (exitButton != null) exitButton.gameObject.SetActive(false);

        // 3. Tắt luôn chữ tiêu đề cho gọn
        if (titleText != null) titleText.SetActive(false);
    }

    // ==========================================
    // SỰ KIỆN 2: BẤM NÚT "XÁC NHẬN" TÊN
    // ==========================================
    public void OnConfirmName()
    {
        PlayClickSound();

        string enteredName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(enteredName))
        {
            enteredName = "Bạn";
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