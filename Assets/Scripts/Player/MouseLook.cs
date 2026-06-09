using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 300f; // Chỉnh mặc định về 300f cho đồng bộ với file kia
    public Transform playerBody;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. CHẶN LỖI: Nếu không có PlayerBody thì báo lỗi đỏ để bạn biết mà kéo vào!
        if (playerBody == null)
        {
            Debug.LogError("LỖI: Chưa kéo Player vào ô Player Body trong script MouseLook!");
            return;
        }

        // 2. CHẶN LỖI: Nếu game đang bị dừng (TimeScale = 0), không tính toán chuột nữa
        if (Time.timeScale == 0f) return;

        // ==========================================
        // ĐÃ THÊM: LIÊN KẾT THANH TRƯỢT TỐC ĐỘ CHUỘT TỪ MENU SETTING
        // ==========================================
        float currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivity);

        // Lấy thông tin chuột di chuyển (trên trục X và Y) nhân với tốc độ từ cài đặt
        float mouseX = Input.GetAxis("Mouse X") * currentSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * currentSensitivity * Time.deltaTime;

        // Tính toán góc xoay lên/xuống (xoay quanh trục X của Camera)
        xRotation -= mouseY;

        // Giới hạn góc nhìn: Không cho nhân vật gập cổ quá 90 độ (tránh lộn ngược đầu)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Áp dụng góc xoay lên/xuống cho Camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Áp dụng góc xoay trái/phải cho TOÀN BỘ thân nhân vật (quanh trục Y)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}