using UnityEngine;
using UnityEngine.UI; // Bắt buộc thêm dòng này để dùng UI Thanh Stamina
using System.Collections;
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float walkSpeed = 5f;
    public float sneakSpeed = 2f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;

    [Header("Cài đặt Nhảy & Ngồi")]
    public float jumpHeight = 1.5f;     // BỔ SUNG: Độ cao khi nhảy
    public float standingHeight = 2f;
    public float crouchHeight = 1f;

    [Header("Cài đặt Thể lực (Stamina)")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 15f;
    public float jumpStaminaCost = 15f; // BỔ SUNG: Thể lực tốn khi nhảy 1 lần
    public Slider staminaBar;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isSneaking = false;
    private bool isSprinting = false;

    // Biến tạm để xử lý mượt mà khi đứng lên (tránh bị kẹt trần)
    private bool isCrouching = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Khởi tạo thể lực đầy 100% khi bắt đầu game
        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    void Update()
    {
        HandleStamina();
        HandleCrouch(); // BỔ SUNG: Đổi tên và Input
        MovePlayer();
        ApplyGravityAndJump(); // ĐÃ SỬA: Gộp Gravity và Nhảy vào 1 chỗ để tránh xung đột
    }

    void HandleStamina()
    {
        // Nhấn giữ Left Shift và đang di chuyển thì mới tính là Chạy nhanh
        // B BỔ SUNG: Không cho chạy nhanh khi đang ngồi
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && !isCrouching && isMoving)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            isSprinting = false;
            // Hồi phục thể lực nếu đang dưới mức tối đa
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }

        // Đảm bảo thể lực không bị âm hoặc vượt mức 100
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // Cập nhật thanh UI trên màn hình
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }
    }

    void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        // Quyết định tốc độ hiện tại
        float currentSpeed = walkSpeed;
        if (isCrouching) // ĐÃ SỬA: Dùng biến Crouch
        {
            currentSpeed = sneakSpeed;
        }
        else if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    // ĐÃ SỬA: Đổi Input từ Ctrl thành Left Control để phù hợp với phím tắt standard
    void HandleCrouch()
    {
        // Khi nhấn Left Control
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            SetCrouch(true);
        }

        // Khi thả Left Control
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            // BỔ SUNG: Cần kiểm tra xem phía trên có trần nhà không trước khi đứng lên
            if (!Physics.Raycast(transform.position, Vector3.up, standingHeight))
            {
                SetCrouch(false);
            }
            else
            {
                // Nếu bị kẹt, vẫn giữ trạng thái ngồi, cần phải đi ra chỗ trống mới đứng lên được
                Debug.Log("Không thể đứng lên, bị vướng trần nhà!");
                StartCoroutine(TryStandUpLater());
            }
        }
    }

    private void SetCrouch(bool crouch)
    {
        isCrouching = crouch;
        if (crouch)
        {
            controller.height = crouchHeight;
            // Chỉnh trung tâm của Collider xuống thấp để nhân vật không bị bay lơ lửng
            controller.center = new Vector3(0, crouchHeight / 2f, 0);
        }
        else
        {
            controller.height = standingHeight;
            controller.center = new Vector3(0, standingHeight / 2f, 0);
        }
    }

    // Coroutine để thử đứng lên liên tục khi người chơi thả nút nhưng bị kẹt trần
    IEnumerator TryStandUpLater()
    {
        while (Input.GetKeyUp(KeyCode.LeftControl) && isCrouching)
        {
            if (!Physics.Raycast(transform.position, Vector3.up, standingHeight))
            {
                SetCrouch(false);
                yield break;
            }
            yield return null; // Đợi frame tiếp theo
        }
    }

    // ĐÃ SỬA: Gộp Jump và Gravity vào đây
    void ApplyGravityAndJump()
    {
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f; // Giữ nhân vật bám đất
            }

            // B BỔ SUNG: Xử lý Nhảy khi nhấn Space
            // Chỉ nhảy khi: Chạm đất, Nhấn Space, Đủ thể lực, và KHÔNG đang ngồi
            if (Input.GetKeyDown(KeyCode.Space) && currentStamina >= jumpStaminaCost && !isCrouching)
            {
                // Công thức vật lý tính vận tốc nhảy: v = sqrt(height * -2 * g)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // Trừ thể lực ngay lập tức
                currentStamina -= jumpStaminaCost;
            }
        }

        // Áp dụng trọng lực liên tục
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}