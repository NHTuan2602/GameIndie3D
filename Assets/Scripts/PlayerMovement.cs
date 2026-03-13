using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Cài đặt Di chuyển & Góc nhìn")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 300f;

    [Header("Trạng thái Hoạt động")]
    [Tooltip("Bỏ tích ô này để KHÓA CHÂN nhân vật (Dùng cho mini-game)")]
    public bool canWalk = false;
    [Tooltip("Tích ô này để cho phép quay đầu ngắm nhìn xung quanh")]
    public bool canLook = true;

    // ==============================================================
    // KHU VỰC CÔNG TẮC ĐIỆN ẢNH (DÙNG CHO NGỒI TRÊN XE BUS / SIÊU THỊ)
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
        // 1. HỆ THỐNG XOAY GÓC NHÌN
        // ==========================================
        if (canLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            yRotation += mouseX;

            if (limitHorizontalLook)
            {
                yRotation = Mathf.Clamp(yRotation, startYRotation - maxHorizontalAngle, startYRotation + maxHorizontalAngle);
            }

            if (!lockVerticalLook)
            {
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            }

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            // TẠM THỜI TẮT BẮN TIA Ở ĐÂY ĐỂ TRÁNH XUNG ĐỘT VỚI DRAG AND SNAP
            /*
            if (Input.GetMouseButtonDown(0))
            {
                InteractWithObject();
            }
            */
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // ==========================================
        // 2. XỬ LÝ TRỌNG LỰC (LUÔN PHẢI CHẠY DÙ CÓ BỊ KHÓA CHÂN HAY KHÔNG)
        // ==========================================
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        // ==========================================
        // 3. HỆ THỐNG ĐI LẠI TRÊN MẶT PHẲNG (CHỈ CHẠY KHI ĐƯỢC PHÉP)
        // ==========================================
        if (canWalk)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 moveDirection = transform.right * x + transform.forward * z;
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }

        // Áp dụng trọng lực cuối cùng (để nhân vật luôn đứng vững trên sàn)
        controller.Move(velocity * Time.deltaTime);
    }

    // ==========================================
    // 4. CÁC HÀM CÔNG TẮC ĐỂ SCRIPT KHÁC GỌI VÀO
    // ==========================================

    // Hàm này để MiniGameManager gọi khi bắt đầu ca làm
    public void LockMovementForMiniGame()
    {
        canWalk = false;
        canLook = true;
    }

    // Hàm này để MiniGameManager gọi khi kết thúc ca làm (hết giờ)
    public void UnlockMovement()
    {
        canWalk = true;
        canLook = true;
    }

    // Hàm này để vô hiệu hóa hoàn toàn khi đang nói chuyện (Visual Novel)
    public void LockAllForDialogue()
    {
        canWalk = false;
        canLook = false;
    }
}