using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameManager : MonoBehaviour
{
    [Header("Cài đặt Mini-game")]
    public int totalItemsToSort = 0;
    private int currentItemsPlaced = 0;
    public CrosshairController crosshairController;
    [Header("Giao diện Điện thoại")]
    public GameObject phoneMessagePanel;

    void Start()
    {
        DragAndSnap[] allItems = FindObjectsOfType<DragAndSnap>();
        totalItemsToSort = allItems.Length;
        Debug.Log("Tổng số lượng hàng: " + totalItemsToSort);
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

        // Tước quyền đi lại của nhân vật khi đang xem hội thoại
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = false;

        // Đảm bảo chuột đã được hiện lên để bấm nút Đồng ý / Từ chối
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tắt cái giao diện điện thoại cũ đi để không bị đè hình
        if (phoneMessagePanel != null)
        {
            phoneMessagePanel.SetActive(false);
        }

        // ========================================================
        // ĐÂY LÀ DÒNG LỆNH ĐÁNH THỨC VISUAL NOVEL CỦA CHÚNG TA
        // ========================================================
        FindObjectOfType<DialogueManager>().TriggerStoryEvent();
    }

    public void AcceptJobAndGoToCampuchia()
    {
        SceneManager.LoadScene("SampleScene");
    }
}