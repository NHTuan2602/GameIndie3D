using UnityEngine;
using UnityEngine.UI; // Bắt buộc thêm dòng này để dùng UI Thanh Stamina

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float walkSpeed = 5f;
    public float sneakSpeed = 2f;
    public float sprintSpeed = 8f; // Tốc độ khi chạy nhanh
    public float gravity = -9.81f;

    [Header("Cài đặt Lén lút")]
    public float standingHeight = 2f;
    public float crouchHeight = 1f;

    [Header("Cài đặt Thể lực (Stamina)")]
    public float maxStamina = 100f; // Thể lực tối đa
    public float currentStamina;    // Thể lực hiện tại
    public float staminaDrainRate = 20f; // Tốc độ tụt thể lực khi chạy
    public float staminaRegenRate = 15f; // Tốc độ hồi phục thể lực khi đi bộ
    public Slider staminaBar; // Kéo thả thanh Slider UI vào đây

    private CharacterController controller;
    private Vector3 velocity;
    private bool isSneaking = false;
    private bool isSprinting = false;

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
        MovePlayer();
        HandleSneak();
        ApplyGravity();
    }

    void HandleStamina()
    {
        // Nhấn giữ Left Shift và đang di chuyển thì mới tính là Chạy nhanh
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && !isSneaking && isMoving)
        {
            isSprinting = true;
            // Trừ thể lực dần dần
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
        if (isSneaking)
        {
            currentSpeed = sneakSpeed;
        }
        else if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleSneak()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isSneaking = true;
            controller.height = crouchHeight;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isSneaking = false;
            controller.height = standingHeight;
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}