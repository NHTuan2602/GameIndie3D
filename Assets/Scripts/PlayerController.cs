using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float walkSpeed = 5f;
    public float sneakSpeed = 2.5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;

    [Header("Cài đặt Nhảy & Ngồi")]
    public float jumpHeight = 1.5f;
    public float standingHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchTransitionSpeed = 10f;

    [Header("Tham chiếu Camera")]
    public Transform playerCamera;
    private float defaultCameraY;
    private float crouchCameraY;

    [Header("Cài đặt Thể lực")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 15f;
    public float jumpStaminaCost = 15f;
    public Slider staminaBar;

    private CharacterController controller;
    private Vector3 velocity;

    private bool isCrouching = false;
    private bool isSprinting = false;
    private bool wantsToStand = false;

    // Biến nội suy
    private float targetHeight;
    private float targetCenterY; // BỔ SUNG: Tính toán tâm va chạm
    private float targetCameraY;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }

        // Khởi tạo thông số Đứng
        targetHeight = standingHeight;
        targetCenterY = 0f; // Tâm mặc định của Unity Capsule là 0 (ở giữa)

        if (playerCamera != null)
        {
            defaultCameraY = playerCamera.localPosition.y;
            // Công thức: Mắt hạ xuống đúng bằng khoảng cách chiều cao bị lùn đi
            crouchCameraY = defaultCameraY - (standingHeight - crouchHeight);
            targetCameraY = defaultCameraY;
        }
    }

    void Update()
    {
        HandleStamina();
        HandleCrouch();
        SmoothCrouchTransition();
        MovePlayer();
        ApplyGravityAndJump();
    }

    void HandleStamina()
    {
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && !isCrouching && isMoving)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            isSprinting = false;
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        if (staminaBar != null) staminaBar.value = currentStamina;
    }

    void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = walkSpeed;
        if (isCrouching) currentSpeed = sneakSpeed;
        else if (isSprinting) currentSpeed = sprintSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            wantsToStand = false;
            SetCrouch(true);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            wantsToStand = true;
        }

        if (wantsToStand && isCrouching)
        {
            // Bắn tia từ vị trí đỉnh đầu lúc đang ngồi lên trên
            Vector3 feetPosition = transform.position - (Vector3.up * (standingHeight / 2f));
            Vector3 rayStart = feetPosition + (Vector3.up * crouchHeight);
            float rayLength = standingHeight - crouchHeight + 0.1f;

            bool hitCeiling = false;
            RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.up, rayLength);
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject != this.gameObject)
                {
                    hitCeiling = true;
                    break;
                }
            }

            if (!hitCeiling)
            {
                SetCrouch(false);
                wantsToStand = false;
            }
        }
    }

    private void SetCrouch(bool crouch)
    {
        isCrouching = crouch;
        if (crouch)
        {
            targetHeight = crouchHeight;
            // Chuyển tâm xuống dưới để giữ bàn chân không bị tụt
            targetCenterY = (crouchHeight - standingHeight) / 2f;
            targetCameraY = crouchCameraY;
        }
        else
        {
            targetHeight = standingHeight;
            targetCenterY = 0f; // Trả tâm về giữa
            targetCameraY = defaultCameraY;
        }
    }

    void SmoothCrouchTransition()
    {
        if (Mathf.Abs(controller.height - targetHeight) > 0.01f)
        {
            // Nội suy chiều cao
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

            // Nội suy Tâm (Center) để giữ vững bàn chân trên mặt đất
            float newCenterY = Mathf.Lerp(controller.center.y, targetCenterY, Time.deltaTime * crouchTransitionSpeed);
            controller.center = new Vector3(0, newCenterY, 0);
        }

        if (playerCamera != null && Mathf.Abs(playerCamera.localPosition.y - targetCameraY) > 0.01f)
        {
            float newCamY = Mathf.Lerp(playerCamera.localPosition.y, targetCameraY, Time.deltaTime * crouchTransitionSpeed);
            playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, newCamY, playerCamera.localPosition.z);
        }
    }

    void ApplyGravityAndJump()
    {
        if (controller.isGrounded)
        {
            if (velocity.y < 0) velocity.y = -2f;

            if (Input.GetKeyDown(KeyCode.Space) && currentStamina >= jumpStaminaCost && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                currentStamina -= jumpStaminaCost;
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}