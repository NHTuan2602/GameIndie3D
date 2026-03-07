using UnityEngine;

// Dòng lệnh này ép Unity tự động gắn CharacterController vào nhân vật nếu bạn quên
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 300f;

    [Header("Trạng thái")]
    public bool canMove = false;

    private float xRotation = 0f;
    private float yRotation = 0f;

    // Khai báo bộ điều khiển vật lý mới
    private CharacterController controller;

    void Start()
    {
        // Lấy bộ điều khiển vật lý gắn trên người nhân vật
        controller = GetComponent<CharacterController>();

        Vector3 rot = transform.localRotation.eulerAngles;
        yRotation = rot.y;
        xRotation = rot.x;

        // Mới vào game: Đảm bảo chuột hiện lên để bạn gõ Tên Nhân Vật
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // NẾU CHƯA NHẬP TÊN XONG HOẶC ĐANG XEM ĐIỆN THOẠI -> Đứng im
        if (!canMove) return;

        // KHI ĐANG TRONG CA LÀM VIỆC: Tự động khóa và tàng hình chuột
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- HỆ THỐNG XOAY GÓC NHÌN ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        // Khóa cổ không cho ngửa/cúi quá 90 độ
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // --- HỆ THỐNG ĐI LẠI (ĐÃ CÓ VA CHẠM VẬT LÝ) ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * x + transform.forward * z;
        moveDirection.y = 0f; // Ép độ cao bằng 0 để không cắm đầu xuống đất

        // DÙNG HÀM TÍNH TOÁN VẬT LÝ CỦA UNITY THAY VÌ DỊCH CHUYỂN TỨC THỜI
        controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
    }
}