using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("Giao diện Nhập Tên")]
    public GameObject namePanel;
    public TMP_InputField nameInputField;
    public Button submitButton;
    public CrosshairController crosshairController;

    void Start()
    {
        namePanel.SetActive(true);

        // Mở khóa chuột để người chơi gõ tên
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Trói chân nhân vật
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = false;

        submitButton.onClick.AddListener(OnSubmitName);
    }

    void OnSubmitName()
    {
        string rawInput = nameInputField.text;
        string playerName = rawInput.Replace("\u200B", "").Trim();

        // Chặn không cho qua nếu để trống
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Chưa nhập tên! Vui lòng nhập để tiếp tục.");
            return;
        }

        // LƯU TÊN VÀO BỘ NÃO GAMEMANAGER
        PlayerPrefs.SetString("SavedPlayerName", playerName);
        PlayerPrefs.Save();

        if (GameManager.instance != null)
        {
            GameManager.instance.playerName = playerName;
        }

        Debug.Log("Đã lưu hồ sơ nhân viên: " + playerName);
        namePanel.SetActive(false);

        // Mở khóa di chuyển cho nhân vật
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = true;

        // Tùy cơ chế 3D của bạn: Ở đây tôi giả định bạn cần khóa chuột vào giữa màn hình để chơi góc nhìn thứ nhất
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (crosshairController != null) crosshairController.Show();

        // KÍCH HOẠT MINIGAME XẾP ĐỒ Ở ĐÂY (Nếu cần gọi hàm)
    }
}