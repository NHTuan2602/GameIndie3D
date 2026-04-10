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
    [Tooltip("Thời gian lấy đà/hãm phanh để di chuyển mượt hơn")]
    public float movementSmoothTime = 0.1f;

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

    // Biến lưu trữ vận tốc tổng hợp (MỚI)
    private Vector3 currentMovement;
    private float verticalVelocity; // Tách riêng trục Y
    private Vector2 currentDir = Vector2.zero;
    private Vector2 currentDirVelocity = Vector2.zero;

    private bool isCrouching = false;
    private bool isSprinting = false;
    private bool wantsToStand = false;

    // Biến nội suy Ngồi
    private float targetHeight;
    private float targetCenterY;
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

        targetHeight = standingHeight;
        targetCenterY = 0f;

        if (playerCamera != null)
        {
            defaultCameraY = playerCamera.localPosition.y;
            crouchCameraY = defaultCameraY - (standingHeight - crouchHeight);
            targetCameraY = defaultCameraY;
        }
    }

    void Update()
    {
        HandleStamina();
        HandleCrouch();
        SmoothCrouchTransition();

        // Gom tính toán vào 1 hàm duy nhất
        CalculateAndApplyMovement();
    }

    void HandleStamina()
    {
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

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
            Vector3 feetPosition = transform.position - (Vector3.up * (standingHeight / 2f));
            Vector3 rayStart = feetPosition + (Vector3.up * crouchHeight);
            float rayLength = standingHeight - crouchHeight + 0.1f;

            bool hitCeiling = false;
            RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.up, rayLength);
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject != this.gameObject && !hit.collider.isTrigger)
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
            targetCenterY = (crouchHeight - standingHeight) / 2f;
            targetCameraY = crouchCameraY;
        }
        else
        {
            targetHeight = standingHeight;
            targetCenterY = 0f;
            targetCameraY = defaultCameraY;
        }
    }

    void SmoothCrouchTransition()
    {
        if (Mathf.Abs(controller.height - targetHeight) > 0.01f)
        {
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            float newCenterY = Mathf.Lerp(controller.center.y, targetCenterY, Time.deltaTime * crouchTransitionSpeed);
            controller.center = new Vector3(0, newCenterY, 0);
        }

        if (playerCamera != null && Mathf.Abs(playerCamera.localPosition.y - targetCameraY) > 0.01f)
        {
            float newCamY = Mathf.Lerp(playerCamera.localPosition.y, targetCameraY, Time.deltaTime * crouchTransitionSpeed);
            playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, newCamY, playerCamera.localPosition.z);
        }
    }

    // HÀM MỚI: TÍNH TOÁN VÀ DI CHUYỂN TRONG 1 LẦN GỌI (SỬA LỖI KHỰNG)
    void CalculateAndApplyMovement()
    {
        // 1. Lấy input và làm mượt nó (Gia tốc)
        Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetDir.Normalize(); // Sửa lỗi đi chéo bị nhanh hơn

        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, movementSmoothTime);

        // 2. Xác định tốc độ hiện tại
        float currentSpeed = walkSpeed;
        if (isCrouching) currentSpeed = sneakSpeed;
        else if (isSprinting) currentSpeed = sprintSpeed;

        // 3. Tính toán trọng lực & Nhảy
        if (controller.isGrounded)
        {
            // Luôn ép nhẹ xuống sàn để isGrounded không bị lỗi nhấp nháy
            verticalVelocity = -2f;

            if (Input.GetKeyDown(KeyCode.Space) && currentStamina >= jumpStaminaCost && !isCrouching)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                currentStamina -= jumpStaminaCost;
            }
        }
        else
        {
            // Trọng lực rơi tự do
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 4. Kết hợp vector và gọi lệnh Move DUY NHẤT 1 LẦN
        currentMovement = (transform.right * currentDir.x + transform.forward * currentDir.y) * currentSpeed;
        currentMovement.y = verticalVelocity; // Gắn trục Y vào

        controller.Move(currentMovement * Time.deltaTime);
    }
}