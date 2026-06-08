using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MiniGameManager : MonoBehaviour
{
    [Header("Cài đặt Mini-game")]
    public int totalItemsToSort = 0;
    private int currentItemsPlaced = 0;
    public CrosshairController crosshairController;

    [Header("Giao diện Điện thoại")]
    public GameObject phoneMessagePanel;

    [Header("--- KỊCH BẢN RỦ RÊ (Hết ca làm) ---")]
    public DialogueLine[] outroLines;

    [Header("--- GIAO DIỆN LỰA CHỌN ---")]
    public GameObject choicePanel;
    public Button agreeButton;
    public Button refuseButton;

    [Header("--- GIAO DIỆN ENDING (Màn hình đen) ---")]
    public GameObject endingPanel;
    public TextMeshProUGUI endingText;

    // ĐÃ XÓA BIẾN COROUTINE VÌ KHÔNG CÒN DÙNG ĐẾN ĐẾM NGƯỢC NỮA

    void Start()
    {
        // Khắc phục lỗi báo Warning Obsolete ở Unity 6
        DragAndSnap[] allItems = FindObjectsByType<DragAndSnap>(FindObjectsSortMode.None);
        totalItemsToSort = allItems.Length;
        Debug.Log("Tổng số lượng hàng: " + totalItemsToSort);

        // Tắt các bảng lúc mới vào game
        if (choicePanel != null) choicePanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false);

        // Gắn sự kiện cho 2 nút bấm
        if (agreeButton != null) agreeButton.onClick.AddListener(OnAgreeClicked);
        if (refuseButton != null) refuseButton.onClick.AddListener(OnRefuseClicked);
    }

    void OnEnable()
    {
        DragAndSnap.OnItemPlaced += CountItem;
    }

    void OnDisable()
    {
        DragAndSnap.OnItemPlaced -= CountItem;
    }

    void CountItem()
    {
        currentItemsPlaced++;
        Debug.Log("Tiến độ: " + currentItemsPlaced + "/" + totalItemsToSort);

        if (currentItemsPlaced >= totalItemsToSort && totalItemsToSort > 0)
        {
            CompleteShift();
        }
    }

    void CompleteShift()
    {
        if (crosshairController != null) crosshairController.Hide();
        Debug.Log("HẾT CA LÀM! Kích hoạt cốt truyện Visual Novel...");

        if (phoneMessagePanel != null)
        {
            phoneMessagePanel.SetActive(false);
        }

        // ========================================================
        // GỌI HỆ THỐNG DIALOGUE MỚI BẰNG HÀM VẠN NĂNG
        // ========================================================
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(outroLines, () =>
            {
                if (choicePanel != null) choicePanel.SetActive(true);

                // ========================================================
                // ĐÃ FIX: TƯỚC QUYỀN ĐIỀU KHIỂN CỦA PLAYER TRƯỚC KHI BẬT CHUỘT
                // ========================================================
                PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
                if (player != null)
                {
                    player.canWalk = false;
                    player.canLook = false; // Khóa dòng này thì Player mới nhả chuột ra!
                }

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            });
        }
        else
        {
            Debug.LogError("Không tìm thấy DialogueManager trong Scene!");
        }
    }

    // ========================================================
    // XỬ LÝ NÚT BẤM KHI HỘI THOẠI KẾT THÚC
    // ========================================================
    private void OnAgreeClicked()
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        SceneManager.LoadScene("BusScene");
    }

    private void OnRefuseClicked()
    {
        if (choicePanel != null) choicePanel.SetActive(false);

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
            if (endingText != null)
                endingText.text = "Bạn đã từ chối lời đề nghị. Cuộc sống sinh viên nghèo vẫn tiếp diễn, nhưng ít ra bạn được bình yên.\n\n<color=#FF0000>ENDING 1: Lựa chọn đúng đắn!</color>";
        }

        // ĐÃ XÓA QUẢ BOM ĐẾM NGƯỢC 5 GIÂY TẮT GAME Ở ĐÂY
    }

    // ĐÃ XÓA HÀM IEnumerator EndGameSequence() VÌ KHÔNG CÒN DÙNG NỮA

    // ==========================================
    // NÚT 1: CHƠI LẠI (QUAY LẠI ĐOẠN LỰA CHỌN)
    // ==========================================
    public void QuayLaiLuaChon()
    {
        // 1. Tắt màn hình Ending đi
        if (endingPanel != null) endingPanel.SetActive(false);

        // 2. Bật lại bảng lựa chọn (Đồng ý / Từ chối)
        if (choicePanel != null) choicePanel.SetActive(true);

        // 3. Đảm bảo khóa Player khi chọn lại
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.canWalk = false;
            player.canLook = false;
        }

        // 4. Đảm bảo hiện chuột để chọn lại
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Người chơi muốn chọn lại... Cơ hội thứ 2!");
    }

    // ==========================================
    // NÚT 2: VỀ MENU CHÍNH
    // ==========================================
    public void VeMenuChinh()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Hàm dự phòng
    public void AcceptJobAndGoToCampuchia()
    {
        SceneManager.LoadScene("SampleScene");
    }
}