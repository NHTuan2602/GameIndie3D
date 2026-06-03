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

    [Header("Cài đặt Ngồi xổm")]
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
    public Slider staminaBar;

    [Header("Cài đặt Âm thanh Bước chân (MỚI)")]
    public AudioSource footstepSource;
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.3f;
    public float crouchStepInterval = 0.8f;
    private float stepTimer = 0f;

    private CharacterController controller;
    private Vector3 currentMovement;
    private float verticalVelocity;
    private Vector2 currentDir = Vector2.zero;
    private Vector2 currentDirVelocity = Vector2.zero;

    private bool isCrouching = false;
    private bool isSprinting = false;
    private bool wantsToStand = false;

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

        if (footstepSource == null) footstepSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        HandleStamina();
        HandleCrouch();
        SmoothCrouchTransition();
        CalculateAndApplyMovement();
        HandleFootsteps(); // Gọi hàm phát âm thanh
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

    void CalculateAndApplyMovement()
    {
        Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetDir.Normalize();

        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, movementSmoothTime);

        float currentSpeed = walkSpeed;
        if (isCrouching) currentSpeed = sneakSpeed;
        else if (isSprinting) currentSpeed = sprintSpeed;

        if (controller.isGrounded)
        {
            // Ép vận tốc Y âm một chút để CharacterController luôn dán chặt xuống mặt dốc/cầu thang
            verticalVelocity = -2f;
        }
        else
        {
            // Trọng lực kéo nhân vật xuống khi đi khỏi mép bục
            verticalVelocity += gravity * Time.deltaTime;
        }

        currentMovement = (transform.right * currentDir.x + transform.forward * currentDir.y) * currentSpeed;
        currentMovement.y = verticalVelocity;

        controller.Move(currentMovement * Time.deltaTime);
    }

    // ==========================================
    // XỬ LÝ NHỊP BƯỚC CHÂN
    // ==========================================
    void HandleFootsteps()
    {
        bool isMoving = (currentDir.magnitude > 0.1f) && controller.isGrounded;

        if (isMoving)
        {
            stepTimer += Time.deltaTime;

            // Xác định xem đang đi bộ, chạy hay ngồi xổm để chọn nhịp
            float currentInterval = walkStepInterval;
            if (isSprinting) currentInterval = sprintStepInterval;
            else if (isCrouching) currentInterval = crouchStepInterval;

            if (stepTimer >= currentInterval)
            {
                if (AudioManager.instance != null && footstepSource != null)
                {
                    AudioManager.instance.PlayFootstep(footstepSource);
                }
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f; // Đứng im thì reset nhịp
        }
    }
}