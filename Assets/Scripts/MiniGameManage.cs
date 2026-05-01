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

    void Start()
    {
        DragAndSnap[] allItems = FindObjectsOfType<DragAndSnap>();
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
            // Truyền kịch bản vào, và ra lệnh: "Nói xong thì bật cái bảng ChoicePanel lên nhé!"
            DialogueManager.instance.StartDialogue(outroLines, () =>
            {
                if (choicePanel != null) choicePanel.SetActive(true);
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
                endingText.text = "Bạn đã từ chối lời đề nghị. Cuộc sống sinh viên nghèo vẫn tiếp diễn, nhưng ít ra bạn được bình yên.\n\n<color=#FF0000>ENDING 1: BẠN SỢ RỒI!</color>";
        }
        StartCoroutine(EndGameSequence());
    }

    IEnumerator EndGameSequence()
    {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

    // Hàm dự phòng của bạn
    public void AcceptJobAndGoToCampuchia()
    {
        SceneManager.LoadScene("SampleScene");
    }
}