using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Bắt buộc phải có để chuyển cảnh

public class IntroManager : MonoBehaviour
{
    [Header("Giao diện Nhập Tên")]
    public GameObject namePanel;
    public CanvasGroup namePanelCanvasGroup;
    public TMP_InputField nameInputField;
    public Button submitButton;
    public CrosshairController crosshairController;

    [Header("Ảnh nền Chuyển cảnh")]
    public GameObject supermarketBg; // Kéo cái Image SupermarketBg vô đây

    [Header("Xử lý Âm thanh & Khung chữ")]
    public AudioSource bgmManager;   // Kéo cái BGM_Manager (loa phát nhạc FNAF 1) vô đây
    public GameObject dialoguePanel; // Kéo cái DialoguePanel (khung đen chứa chữ) vô đây

    [Header("Cài đặt Hiệu ứng")]
    public float fadeDuration = 1.5f;

    [Header("--- KỊCH BẢN TỰ GIỚI THIỆU ---")]
    public DialogueLine[] introLines;

    void Start()
    {
        // Khởi tạo trạng thái ban đầu chuẩn chỉnh
        namePanel.SetActive(true);
        if (supermarketBg != null) supermarketBg.SetActive(false);

        if (namePanelCanvasGroup != null)
        {
            namePanelCanvasGroup.alpha = 1f;
            namePanelCanvasGroup.blocksRaycasts = true;
        }

        // Mở khóa chuột để người chơi nhập tên
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Khóa chân nhân vật không cho đi bậy lúc đang ở Menu
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.canWalk = false;
            player.canLook = false;
        }

        // Đăng ký sự kiện click chuột cho nút Xác nhận
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitName);
        }
    }

    void OnSubmitName()
    {
        string rawInput = nameInputField.text;
        string playerName = rawInput.Replace("\u200B", "").Trim();

        // Kiểm tra nếu người chơi lười không thèm nhập tên
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Chưa nhập tên! Vui lòng nhập để tiếp tục.");
            return;
        }

        // Lưu tên vào bộ nhớ hệ thống
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        if (GameManager.instance != null)
        {
            GameManager.instance.playerName = playerName;
        }

        // THỰC THI YÊU CẦU: Tắt nhạc nền lập tức khi ấn nút Xác nhận
        if (bgmManager != null)
        {
            bgmManager.Stop();
        }

        Debug.Log("Đã lưu hồ sơ nhân viên: " + playerName);

        // Tiến hành hiệu ứng làm mờ và kích hoạt cốt truyện
        StartCoroutine(FadeOutAndStartGame());
    }

    IEnumerator FadeOutAndStartGame()
    {
        // Khóa tương tác UI để tránh người chơi spam click
        submitButton.interactable = false;
        nameInputField.interactable = false;
        if (namePanelCanvasGroup != null) namePanelCanvasGroup.blocksRaycasts = false;

        // Vòng lặp làm mờ bảng nhập tên theo thời gian fadeDuration
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            if (namePanelCanvasGroup != null)
            {
                namePanelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            }
            yield return null;
        }

        // Tắt hẳn bảng nhập tên sau khi đã mờ hẳn
        namePanel.SetActive(false);

        // Bật ảnh nền siêu thị mờ mờ lên tạo không khí bối cảnh mới
        if (supermarketBg != null)
        {
            supermarketBg.SetActive(true);
        }

        // Đợi 1 giây lấy cảm xúc trước khi chữ chạy
        yield return new WaitForSeconds(1.0f);

        // Ép cái hộp chữ DialoguePanel phải hiện hình lên màn hình
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // KÍCH HOẠT HỘI THOẠI VÀ TRUYỀN HÀM CALLBACK CHUYỂN CẢNH VẠN NĂNG
        if (DialogueManager.instance != null)
        {
            // Truyền kịch bản vào và dặn DialogueManager: "Đọc xong thì tự gọi hàm ChuyenCanhSieuThi giùm tôi!"
            DialogueManager.instance.StartDialogue(introLines, ChuyenCanhSieuThi);
        }
        else
        {
            Debug.LogError("Chưa gắn kịch bản DialogueManager vô scene!");
        }
    }

    // Hàm callback sẽ được tự động kích hoạt khi người chơi đọc đến câu cuối cùng
    private void ChuyenCanhSieuThi()
    {
        Debug.Log("Đã đọc xong kịch bản giới thiệu. Tiến hành chuyển sang scene siêu thị!");
        SceneManager.LoadScene("SupermarketScene");
    }
}