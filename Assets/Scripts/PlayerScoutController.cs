using UnityEngine;
using TMPro;

public class PlayerScoutController : MonoBehaviour
{
    [Header("--- HỆ THỐNG TIA MẮT (RAYCAST) ---")]
    public Camera playerCamera;           // Kéo Main Camera của Player vào đây
    public float interactDistance = 4f;   // Tăng tầm nhìn lên 4 mét cho thoải mái
    public TextMeshProUGUI hintText;      // Chữ "Bấm [E] để ghi nhớ..." ở giữa màn hình
    public LayerMask itemLayer;           // BỔ SUNG: Nên tạo Layer riêng cho Items để Raycast tối ưu hơn

    [Header("--- HỆ THỐNG SỔ TAY (NOTEBOOK) ---")]
    public GameObject notebookPanel;      // Kéo Panel Sổ tay UI vào đây
    public TextMeshProUGUI txtWrench;     // Chữ hiển thị nhiệm vụ Cờ lê
    public TextMeshProUGUI txtMap;        // Chữ hiển thị nhiệm vụ Bản đồ
    public TextMeshProUGUI txtRope;       // Chữ hiển thị nhiệm vụ Dây thừng

    [Header("--- SCRIPT ĐIỀU KHIỂN CHUỘT ---")]
    [Tooltip("Kéo script quay chuột (Ví dụ: MouseLook hoặc PlayerController nếu nó xử lý quay) vào đây để khóa chuột khi mở sổ")]
    public MonoBehaviour mouseLookScript;

    private bool isNotebookOpen = false;
    private GameObject currentTarget = null; // Vật thể đang bị nhìn trúng

    void Start()
    {
        // Ẩn UI khi bắt đầu
        if (notebookPanel != null) notebookPanel.SetActive(false);
        if (hintText != null) hintText.gameObject.SetActive(false);
        UpdateNotebookUI();
    }

    void Update()
    {
        HandleRaycastInteraction();
        HandleNotebookToggle();
    }

    // 1. CHỨC NĂNG BẮN TIA LAZER TỪ MẮT
    private void HandleRaycastInteraction()
    {
        // Nếu đang mở sổ tay thì không cho tương tác đồ vật
        if (isNotebookOpen) return;

        // Tạo tia ray từ tâm camera
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Bắn tia dài 4m. Nhìn trúng vật gì đó (có thể dùng itemLayer để tối ưu)
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            GameObject obj = hit.collider.gameObject;

            // KIỂM TRA XEM ĐÓ CÓ PHẢI ĐỒ VƯỢT NGỤC KHÔNG DỰA VÀO TAG
            // Đồng thời phải kiểm tra GameManager có tồn tại và Item đó CHƯA được lấy
            if (GameManager.instance != null)
            {
                if (obj.CompareTag("Item_Wrench") && !GameManager.instance.hasWrench)
                {
                    ShowHint("Bấm [E] để ghi nhớ vị trí Cờ Lê", obj);
                }
                else if (obj.CompareTag("Item_Map") && !GameManager.instance.hasMap)
                {
                    ShowHint("Bấm [E] để chụp lén Bản Đồ", obj);
                }
                else if (obj.CompareTag("Item_Rope") && !GameManager.instance.hasRope)
                {
                    ShowHint("Bấm [E] để giấu Dây Thừng", obj);
                }
                else
                {
                    HideHint(); // Nhìn trúng vật có Tag Item nhưng GameManager chưa init hoặc đã lấy rồi
                }
            }
            else
            {
                HideHint(); // Nhìn trúng vật gì đó không phải Item
            }
        }
        else
        {
            HideHint(); // Không nhìn trúng gì cả
        }

        // BẤM E ĐỂ LẤY ĐỒ
        // Đã sửa: Phải đang KHÔNG mở sổ tay và có currentTarget mới cho bấm E
        if (!isNotebookOpen && currentTarget != null && Input.GetKeyDown(KeyCode.E))
        {
            CollectItem(currentTarget);
        }
    }

    private void ShowHint(string message, GameObject target)
    {
        currentTarget = target;
        if (hintText != null)
        {
            hintText.text = message;
            hintText.gameObject.SetActive(true);
        }
    }

    private void HideHint()
    {
        currentTarget = null;
        if (hintText != null) hintText.gameObject.SetActive(false);
    }

    private void CollectItem(GameObject item)
    {
        if (GameManager.instance == null) return;

        // Cập nhật vào Não bộ của Game (GameManager)
        if (item.CompareTag("Item_Wrench")) GameManager.instance.hasWrench = true;
        else if (item.CompareTag("Item_Map")) GameManager.instance.hasMap = true;
        else if (item.CompareTag("Item_Rope")) GameManager.instance.hasRope = true;

        // Tắt object đó đi (Coi như đã nhặt)
        // Chú ý: Trong game stealth, đôi khi bạn chỉ muốn "ghi nhớ" chứ không làm nó biến mất
        // Nếu muốn Item vẫn còn đó, hãy tag nó sang Tag "Interacted"
        item.SetActive(false);

        HideHint();
        UpdateNotebookUI();

        // Thêm hiệu ứng âm thanh hoặc Particle ở đây nếu cần
        Debug.Log("<color=green>Đã ghi nhớ vật phẩm thành công!</color>");
    }

    // 2. CHỨC NĂNG BẬT/TẮT SỔ TAY BẰNG PHÍM TAB
    private void HandleNotebookToggle()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isNotebookOpen = !isNotebookOpen;
            if (notebookPanel != null) notebookPanel.SetActive(isNotebookOpen);

            if (isNotebookOpen)
            {
                UpdateNotebookUI();
                // Tạm dừng xoay chuột để xem sổ (quan trọng để mouse không quay Player lung tung)
                if (mouseLookScript != null) mouseLookScript.enabled = false;

                // Mở sổ thì nên hiện chuột lên để người chơi có thể click (nếu sổ có nút bấm)
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Bật lại xoay chuột
                if (mouseLookScript != null) mouseLookScript.enabled = true;

                // Đóng sổ thì ẩn chuột và khóa lại tâm màn hình
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // 3. CẬP NHẬT GIAO DIỆN SỔ TAY (GẠCH BỎ KHI ĐÃ TÌM THẤY)
    private void UpdateNotebookUI()
    {
        if (GameManager.instance == null) return;

        // Cờ lê
        if (GameManager.instance.hasWrench) txtWrench.text = "<s>1. Tìm vật cứng cắt xích (Đã có Cờ lê)</s>";
        else txtWrench.text = "1. Cần tìm vật cứng để cắt xích rào";

        // Bản đồ
        if (GameManager.instance.hasMap) txtMap.text = "<s>2. Bản đồ tuyến đường tuần tra (Đã chụp)</s>";
        else txtMap.text = "2. Phải mò vào văn phòng quản lý tìm Bản đồ";

        // Dây thừng
        if (GameManager.instance.hasRope) txtRope.text = "<s>3. Dây thừng đu tường (Đã giấu)</s>";
        else txtRope.text = "3. Tìm dây thừng ở khu nhà kho";
    }
}