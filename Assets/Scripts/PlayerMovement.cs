using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 300f;

    [Header("Trạng thái")]
    public bool canMove = false;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        yRotation = rot.y;
        xRotation = rot.x;

        // Mới vào game: Đảm bảo chuột hiện lên để bạn gõ Tên Nhân Vật
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // NẾU CHƯA NHẬP TÊN XONG HOẶC ĐANG XEM ĐIỆN THOẠI -> Đứng im, không chạy code bên dưới
        if (!canMove) return;

        // KHI ĐANG TRONG CA LÀM VIỆC: Tự động khóa và tàng hình chuột
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- HỆ THỐNG XOAY GÓC NHÌN (QUAY TỰ DO KHÔNG CẦN GIỮ NÚT) ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        // Khóa cổ không cho ngửa/cúi quá 90 độ để không bị lộn ngược camera
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // --- HỆ THỐNG ĐI LẠI (WASD) ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * x + transform.forward * z;
        moveDirection.y = 0f; // Ép độ cao bằng 0 để không cắm đầu xuống đất

        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }
}