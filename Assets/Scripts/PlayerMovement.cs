using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Cài đặt Di chuyển & Góc nhìn")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 300f;

    [Header("Trạng thái Hoạt động")]
    public bool canWalk = false;
    public bool canLook = true;

    // ==============================================================
    // KHU VỰC CÔNG TẮC ĐIỆN ẢNH (DÙNG CHO NGỒI TRÊN XE BUS)
    // ==============================================================
    [Header("Giới hạn Cổ (Chống xoay 360 độ)")]
    [Tooltip("Tích vào đây để KHÓA CỔ không cho nhìn lên/xuống")]
    public bool lockVerticalLook = false;

    [Tooltip("Tích vào đây để ép chỉ được quay đầu sang 2 bên như người thật")]
    public bool limitHorizontalLook = false;
    [Tooltip("Độ ngoái cổ tối đa (Người bình thường là 80-90 độ)")]
    public float maxHorizontalAngle = 80f;

    [Header("Trọng lực & Vật lý")]
    public float gravity = -9.81f;
    private Vector3 velocity;

    [Header("Tương tác (Lấy đồ)")]
    public float interactRange = 3f;
    public Camera playerCamera;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float startYRotation = 0f; // Biến bí mật để chống gãy cổ

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Giữ nguyên góc nhìn ban đầu khi mới load Scene
        Vector3 rot = transform.localRotation.eulerAngles;
        yRotation = rot.y;
        xRotation = rot.x;

        // BÍ QUYẾT: Ghi nhớ lại tọa độ góc cổ lúc mới vào game làm cột mốc
        startYRotation = yRotation;
    }

    void Update()
    {
        // ==========================================
        // 1. HỆ THỐNG XOAY GÓC NHÌN & TƯƠNG TÁC
        // ==========================================
        if (canLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Xoay cổ ngang (Trục Y)
            yRotation += mouseX;

            // NẾU BẬT GIỚI HẠN NGANG: Ép cổ không được xoay quá góc maxHorizontalAngle
            if (limitHorizontalLook)
            {
                yRotation = Mathf.Clamp(yRotation, startYRotation - maxHorizontalAngle, startYRotation + maxHorizontalAngle);
            }

            // NẾU KHÔNG BỊ KHÓA DỌC: Mới cho phép nhìn lên/xuống
            if (!lockVerticalLook)
            {
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            }

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            // Bắn tia click chuột để lấy chai nước
            if (Input.GetMouseButtonDown(0))
            {
                InteractWithObject();
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // ==========================================
        // 2. HỆ THỐNG ĐI LẠI TRÊN MẶT PHẲNG
        // ==========================================
        if (canWalk)
        {
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 moveDirection = transform.right * x + transform.forward * z;
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }

    // ==========================================
    // 3. HÀM PHÁT TIA LASER LẤY CHAI NƯỚC
    // ==========================================
    void InteractWithObject()
    {
        if (playerCamera == null)
        {
            Debug.LogError("GÓC KHUẤT: Bạn chưa kéo Main Camera vào script PlayerMovement!");
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red, 2f);

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.CompareTag("WaterBottle"))
            {
                Debug.Log("CẢNH BÁO: Đã lấy chai nước tẩm thuốc mê!");
                Destroy(hit.collider.gameObject);

                // TODO: Gọi hiệu ứng nhịp tim đập và màn hình đen mờ dần tại đây!
            }
        }
    }
}