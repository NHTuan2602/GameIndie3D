using UnityEngine;
using TMPro; // Khai báo dùng UI Text hiện đại

public class MinigameInteract : MonoBehaviour
{
    [Header("UI Của Minigame Này")]
    public GameObject minigameCanvas; // Kéo Canvas Tài Xỉu (đã tắt sẵn) vào đây

    [Header("Gợi ý Nút Bấm (Press E)")]
    public GameObject interactPromptUI; // Kéo chữ "Nhấn E để cược" ngoài màn hình vào đây

    [Header("Nhân Vật 3D (Khóa khi chơi)")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour cameraLookScript;

    private bool isPlayerNear = false;
    private bool isPlayingGame = false;

    void Start()
    {
        // Vừa vào game thì giấu chữ E đi
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    void Update()
    {
        // Đứng gần + Chưa mở sòng + Bấm phím E
        if (isPlayerNear && !isPlayingGame && Input.GetKeyDown(KeyCode.E))
        {
            EnterMinigame();
        }
    }

    void EnterMinigame()
    {
        isPlayingGame = true;

        // Ẩn chữ E và Hiện sòng bạc
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (minigameCanvas != null) minigameCanvas.SetActive(true);

        // Khóa chân và mắt nhân vật
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (cameraLookScript != null) cameraLookScript.enabled = false;

        // Thả chuột ra cho người chơi bấm nút
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Hàm này nối với nút [Hủy/Thoát] trong Tài Xỉu
    public void ExitMinigame()
    {
        isPlayingGame = false;

        // Tắt sòng bạc
        if (minigameCanvas != null) minigameCanvas.SetActive(false);

        // Nếu thoát ra mà vẫn đứng cạnh bàn thì hiện lại chữ E
        if (isPlayerNear && interactPromptUI != null) interactPromptUI.SetActive(true);

        // Trả lại quyền chạy nhảy
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (cameraLookScript != null) cameraLookScript.enabled = true;

        // Khóa chuột lại vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // NHỚ: Nhân vật phải được gắn Tag là "Player"
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