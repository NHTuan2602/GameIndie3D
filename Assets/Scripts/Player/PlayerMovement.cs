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

            // ==========================================
            // DÙNG NÚT 'E' ĐỂ UỐNG NƯỚC
            // ==========================================
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
    // 4. HÀM TƯƠNG TÁC (CHỈ BẮT ĐÚNG CHAI NƯỚC, BỎ QUA HỘP HÀNG)
    // ==========================================
    void InteractWithObject()
    {
        if (playerCamera == null) return;

        // BƯỚC 1: KIỂM TRA ĐỒ ĐANG CẦM TRÊN TAY TRƯỚC (Không cần bắn laser)
        // Duyệt qua tất cả các vật thể đang làm "con" của Camera
        foreach (Transform child in playerCamera.transform)
        {
            if (child.CompareTag("WaterBottle") && child.gameObject.activeSelf)
            {
                Debug.Log("Đã ấn E uống chai nước trên tay! Bắt đầu sập nguồn...");
                Destroy(child.gameObject); // Vứt chai nước

                // Dọn sạch bóng ma UI
                if (DialogueManager.instance != null && DialogueManager.instance.dialoguePanel != null)
                {
                    DialogueManager.instance.dialoguePanel.SetActive(false);
                    if (DialogueManager.instance.dialogueText != null) DialogueManager.instance.dialogueText.text = "";
                    if (DialogueManager.instance.nameText != null) DialogueManager.instance.nameText.text = "";
                }

                // Gọi đạo diễn sập màn hình
                BusEventManager busEvent = FindObjectOfType<BusEventManager>();
                if (busEvent != null) busEvent.StartBlackout();

                return; // Đã uống xong thì THOÁT HÀM luôn, không chạy xuống code bắn tia bên dưới nữa!
            }
        }

        // BƯỚC 2: NẾU KHÔNG CẦM GÌ TRÊN TAY, THÌ MỚI BẮN TIA LASER ĐỂ LẤY ĐỒ DƯỚI ĐẤT
        // (Giữ lại đoạn này để sau này sang Campuchia còn lượm nhặt đồ vật khác)
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