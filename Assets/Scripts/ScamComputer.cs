using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class ScamComputer : MonoBehaviour
{
    [Header("Cài đặt Tương tác")]
    public float holdTimeRequired = 1.0f; // Đã giảm xuống 1 giây cho mượt hơn
    private float currentHoldTime = 0f;

    [Header("Giao diện & Kết nối")]
    public GameObject scamUIPanel;
    public GameObject rootUI; // MỚI THÊM: Dùng để kích hoạt thằng Cha
    public PlayerMovement player;

    [Header("Giao diện 3D (Hiện khi nhìn vào)")]
    public GameObject promptCanvas;
    public Image loadingCircle;

    private bool isWorking = false;

    void Start()
    {
        if (scamUIPanel != null) scamUIPanel.SetActive(false);
        if (promptCanvas != null) promptCanvas.SetActive(false);
        if (loadingCircle != null) loadingCircle.fillAmount = 0f;
    }

    void Update()
    {
        if (player == null || player.playerCamera == null) return;

        // Nếu đang ngồi hack máy tính thì chỉ chờ bấm Escape để thoát
        if (isWorking)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopWorking();
            }
            return;
        }

        Ray ray = player.playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        bool isLookingAtScreen = false;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            ScamComputer hitComputer = hit.collider.GetComponentInParent<ScamComputer>();

            // Nếu tia nhìn đập trúng cái màn hình
            if (hitComputer != null && hitComputer == this)
            {
                isLookingAtScreen = true;

                // Bật chữ "Giữ E..." lên
                if (promptCanvas != null && !promptCanvas.activeSelf)
                {
                    promptCanvas.SetActive(true);
                }

                // Nếu người chơi đè chặt phím E
                if (Input.GetKey(KeyCode.E))
                {
                    currentHoldTime += Time.deltaTime;

                    if (loadingCircle != null)
                    {
                        loadingCircle.fillAmount = currentHoldTime / holdTimeRequired;
                    }

                    // KHI ĐÃ ĐẦY 100% -> VÀO VIỆC!
                    if (currentHoldTime >= holdTimeRequired)
                    {
                        StartWorking();
                    }
                }
                else
                {
                    // Thả tay ra thì tụt vòng tròn về 0
                    currentHoldTime = 0f;
                    if (loadingCircle != null) loadingCircle.fillAmount = 0f;
                }
            }
        }

        // Quay mặt đi chỗ khác -> Tắt biển báo 3D
        if (!isLookingAtScreen)
        {
            currentHoldTime = 0f;
            if (promptCanvas != null) promptCanvas.SetActive(false);
            if (loadingCircle != null) loadingCircle.fillAmount = 0f;
        }
    }

    void StartWorking()
    {
        isWorking = true;
        currentHoldTime = 0f;

        // 1. TẮT NGAY DÒNG CHỮ VÀ VÒNG TRÒN 3D
        if (promptCanvas != null) promptCanvas.SetActive(false);
        if (loadingCircle != null) loadingCircle.fillAmount = 0f;

        // 2. BẬT BẢNG LỪA ĐẢO LÊN (Bật Cả Cha Lẫn Con)
        if (rootUI != null) rootUI.SetActive(true);
        if (scamUIPanel != null) scamUIPanel.SetActive(true);

        // 3. Khóa camera, xả chuột
        player.canLook = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void StopWorking()
    {
        isWorking = false;

        // Tắt bảng lừa đảo
        if (scamUIPanel != null) scamUIPanel.SetActive(false);
        // Tắt luôn thằng cha cho sạch màn hình
        if (rootUI != null) rootUI.SetActive(false);

        // Nhốt chuột, mở khóa camera
        player.canLook = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}