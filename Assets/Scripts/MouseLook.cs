using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f; // Độ nhạy của chuột
    public Transform playerBody;          // Thân nhân vật để xoay trái/phải

    private float xRotation = 0f;

    void Start()
    {
        // Khóa con trỏ chuột ở giữa màn hình và làm ẩn nó đi khi vào game
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Lấy thông tin chuột di chuyển (trên trục X và Y)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Tính toán góc xoay lên/xuống (xoay quanh trục X của Camera)
        xRotation -= mouseY;

        // Giới hạn góc nhìn: Không cho nhân vật gập cổ quá 90 độ (tránh lộn ngược đầu)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Áp dụng góc xoay lêm/xuống cho Camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Áp dụng góc xoay trái/phải cho TOÀN BỘ thân nhân vật (quanh trục Y)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}