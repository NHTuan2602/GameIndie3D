using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Cài đặt Di chuyển & Góc nhìn")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 300f; // Vẫn giữ biến này để làm thông số dự phòng

    [Header("Trạng thái Hoạt động")]
    public bool canWalk = false;
    public bool canLook = true;

    [Header("Giới hạn Cổ (Chống xoay 360 độ)")]
    public bool lockVerticalLook = false;
    public bool limitHorizontalLook = false;
    public float maxHorizontalAngle = 80f;

    [Header("Trọng lực & Vật lý")]
    public float gravity = -9.81f;
    private Vector3 velocity;

    [Header("Tương tác (Lấy đồ)")]
    public float interactRange = 3f;
    public Camera playerCamera;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float startYRotation = 0f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Vector3 rot = transform.localRotation.eulerAngles;
        yRotation = rot.y;
        xRotation = rot.x;
        startYRotation = yRotation;
    }

    void Update()
    {
        // Tạm dừng mọi thứ nếu mở Pause Menu
        if (Time.timeScale == 0f) return;

        if (canLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // ==========================================
            // ĐÃ THÊM: LIÊN KẾT THANH TRƯỢT TỐC ĐỘ CHUỘT
            // ==========================================
            float currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivity);

            float mouseX = Input.GetAxis("Mouse X") * currentSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * currentSensitivity * Time.deltaTime;

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

            // DÙNG NÚT 'E' ĐỂ UỐNG NƯỚC
            if (Input.GetKeyDown(KeyCode.E))
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
        // GIỮ NGUYÊN HOÀN TOÀN CODE DI CHUYỂN (WASD) CỦA BẠN
        // ==========================================
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        if (canWalk)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 moveDirection = transform.right * x + transform.forward * z;
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // ==========================================
    // HÀM TƯƠNG TÁC
    // ==========================================
    void InteractWithObject()
    {
        if (playerCamera == null) return;

        foreach (Transform child in playerCamera.transform)
        {
            if (child.CompareTag("WaterBottle") && child.gameObject.activeSelf)
            {
                Debug.Log("Đã ấn E uống chai nước trên tay! Bắt đầu sập nguồn...");
                Destroy(child.gameObject);

                if (DialogueManager.instance != null && DialogueManager.instance.dialoguePanel != null)
                {
                    DialogueManager.instance.dialoguePanel.SetActive(false);
                    if (DialogueManager.instance.dialogueText != null) DialogueManager.instance.dialogueText.text = "";
                    if (DialogueManager.instance.nameText != null) DialogueManager.instance.nameText.text = "";
                }

                BusEventManager busEvent = FindObjectOfType<BusEventManager>();
                if (busEvent != null) busEvent.StartBlackout();

                return;
            }
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.CompareTag("WaterBottle"))
            {
                // (Code nhặt đồ dưới đất... dự phòng)
            }
        }
    }

    public void LockMovementForMiniGame() { canWalk = false; canLook = true; }
    public void UnlockMovement() { canWalk = true; canLook = true; }
    public void LockAllForDialogue() { canWalk = false; canLook = false; }
}