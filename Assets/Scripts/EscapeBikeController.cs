using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EscapeBikeController : MonoBehaviour
{
    [Header("Cài đặt Di chuyển Tự do")]
    public float forwardSpeed = 30f;
    public float steerSpeed = 15f;
    public float gravity = -15f;

    // ĐÃ XÓA HOÀN TOÀN: roadBoundary vì đã dùng tường tàng hình vật lý

    [Header("Hệ thống Radar Định vị (Mới)")]
    public int currentDetectedLane = 1; // 1: Làn 1 (Trái), 3: Làn 3 (Giữa), 2: Làn 2 (Phải)

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        ApplyFreeMovement();
        DetectCurrentLane();
    }

    void ApplyFreeMovement()
    {
        // 1. Nhận phím lạng lách (A/D hoặc Mũi tên)
        float horizontalInput = Input.GetAxis("Horizontal");

        // 2. Tính toán vận tốc (Trục X: Lách, Trục Y: Rơi, Trục Z: Tiến)
        float moveX = horizontalInput * steerSpeed;
        float fallSpeed = gravity;

        if (controller.isGrounded)
        {
            fallSpeed = -2f; // Ép nhẹ xuống mặt đường để xe không bị nảy lên
        }

        Vector3 moveVector = new Vector3(moveX, fallSpeed, forwardSpeed);

        // 3. Lệnh di chuyển tự do. Xe sẽ tự trượt khi tông trúng tường tàng hình.
        controller.Move(moveVector * Time.deltaTime);
    }

    void DetectCurrentLane()
    {
        float x = transform.position.x;

        // BỘ NHẬN DIỆN LÀN ĐƯỜNG THEO TỌA ĐỘ BẢN ĐỒ MỚI
        if (x <= 4.894493f)
        {
            currentDetectedLane = 1; // Khu vực từ -30.23792 đổ xuống 4.894493
        }
        else if (x >= 35.10812f)
        {
            currentDetectedLane = 2; // Khu vực từ 35.10812 đổ lên
        }
        else
        {
            currentDetectedLane = 3; // Khu vực "Còn lại" kẹp ở giữa
        }
    }
}