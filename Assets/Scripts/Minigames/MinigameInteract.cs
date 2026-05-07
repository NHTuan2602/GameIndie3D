using UnityEngine;

public class MinigameInteract : MonoBehaviour
{
    [Header("UI Của Minigame Này")]
    public GameObject minigameCanvas;
    public GameObject interactPromptUI;

    [Header("Nhân Vật 3D (Khóa khi chơi)")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour cameraLookScript;
    public GameObject playerObject;

    [Header("Hệ Thống Camera (GÓC NHÌN TỪ TRÊN XUỐNG)")]
    public Camera mainCamera;
    public Camera topDownCamera;

    [Header("--- KẾT NỐI VỚI QUẢN LÝ TÀI XỈU ---")]
    [Tooltip("Kéo cục TaiXiuManager vào đây để đánh thức nó dậy")]
    public TaiXiuManager taiXiuManagerScript;

    private bool isPlayerNear = false;
    private bool isPlayingGame = false;

    void Start()
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (topDownCamera != null) topDownCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && !isPlayingGame && Input.GetKeyDown(KeyCode.E))
        {
            EnterMinigame();
        }
    }

    void EnterMinigame()
    {
        isPlayingGame = true;

        // Bật/tắt UI
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (minigameCanvas != null) minigameCanvas.SetActive(true);

        // Khóa nhân vật
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (cameraLookScript != null) cameraLookScript.enabled = false;

        // Chuyển Camera
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (topDownCamera != null) topDownCamera.gameObject.SetActive(true);
        if (playerObject != null) playerObject.SetActive(false);

        // Hiện chuột
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // -----------------------------------------------------
        // KÍCH HOẠT SỚI BẠC (CHẠY ĐỒNG HỒ VÀ NHẠC)
        // -----------------------------------------------------
        if (taiXiuManagerScript != null)
        {
            taiXiuManagerScript.OpenCasino();
            Debug.Log("<color=green>ĐÃ KÍCH HOẠT SỚI BẠC THÀNH CÔNG!</color>");
        }
        else
        {
            Debug.LogError("<color=red>LỖI NGHIÊM TRỌNG: Bạn chưa kéo cục TaiXiuManager vào lỗ Tai Xiu Manager Script trong Goc_Ngoi_Choi!</color>");
        }
    }

    public void ExitMinigame()
    {
        isPlayingGame = false;

        if (minigameCanvas != null) minigameCanvas.SetActive(false);
        if (isPlayerNear && interactPromptUI != null) interactPromptUI.SetActive(true);

        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (cameraLookScript != null) cameraLookScript.enabled = true;

        if (topDownCamera != null) topDownCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
        if (playerObject != null) playerObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (!isPlayingGame && interactPromptUI != null) interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
        }
    }
}