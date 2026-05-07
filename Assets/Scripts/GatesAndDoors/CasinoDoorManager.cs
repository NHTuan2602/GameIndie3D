using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CasinoDoorManager : MonoBehaviour
{
    [Header("--- GIAO DIỆN XÁC NHẬN ---")]
    public GameObject leaveConfirmationPanel;
    public Button btnConfirmLeave;
    public Button btnCancelLeave;

    [Header("--- HƯỚNG DẪN BẤM PHÍM ---")]
    public TextMeshProUGUI hintText;

    [Header("--- KHÓA NGƯỜI CHƠI (CHỐNG LỖI CHUỘT) ---")]
    [Tooltip("Kéo các script quay chuột (MouseLook) hoặc di chuyển (PlayerMovement) của Player vào đây để tự động tắt nó khi mở Menu")]
    public MonoBehaviour[] scriptsToDisable;

    private bool isNearDoor = false;
    private bool isMenuOpen = false;

    void Start()
    {
        if (leaveConfirmationPanel != null) leaveConfirmationPanel.SetActive(false);
        if (hintText != null) hintText.gameObject.SetActive(false);

        if (btnConfirmLeave != null) btnConfirmLeave.onClick.AddListener(ConfirmLeave);
        if (btnCancelLeave != null) btnCancelLeave.onClick.AddListener(CancelLeave);
    }

    void Update()
    {
        // Nhấn E để MỞ menu khi đang đứng gần
        if (isNearDoor && Input.GetKeyDown(KeyCode.E) && !isMenuOpen)
        {
            OpenMenu();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearDoor = true;
            if (!isMenuOpen && hintText != null) hintText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearDoor = false;
            if (hintText != null) hintText.gameObject.SetActive(false);

            // Nếu người chơi lùi ra xa thì tự động đóng menu (cho an toàn)
            if (isMenuOpen) CloseMenu();
        }
    }

    // HÀM MỞ MENU VÀ MỞ KHÓA CHUỘT
    void OpenMenu()
    {
        isMenuOpen = true;
        if (leaveConfirmationPanel != null) leaveConfirmationPanel.SetActive(true);
        if (hintText != null) hintText.gameObject.SetActive(false); // Ẩn chữ E đi

        // 1. Tạm tắt não người chơi (Tắt script Camera/Di chuyển)
        foreach (var script in scriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }

        // 2. Ép mở khóa con chuột một cách dứt khoát
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // HÀM ĐÓNG MENU VÀ TRẢ LẠI CHUỘT CHO GAME
    void CloseMenu()
    {
        isMenuOpen = false;
        if (leaveConfirmationPanel != null) leaveConfirmationPanel.SetActive(false);
        if (isNearDoor && hintText != null) hintText.gameObject.SetActive(true); // Hiện lại chữ E

        // 1. Bật lại não cho người chơi
        foreach (var script in scriptsToDisable)
        {
            if (script != null) script.enabled = true;
        }

        // 2. Khóa lại con chuột vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ConfirmLeave()
    {
        CloseMenu(); // Đóng menu và bật lại player trước khi chuyển cảnh cho an toàn

        if (GameManager.instance != null)
        {
            GameManager.instance.SleepThroughNight();
        }
    }

    void CancelLeave()
    {
        CloseMenu();
    }
}