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

        // 1. Mở khóa chuột và hiện chuột để người chơi gõ tên
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Trói chân nhân vật, không cho đi lại lúc đang nhập tên
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = false;

        submitButton.onClick.AddListener(OnSubmitName);
    }

    void OnSubmitName()
    {
        if (crosshairController != null) crosshairController.Show();
        string rawInput = nameInputField.text;
        string playerName = rawInput.Replace("\u200B", "").Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Bạn chưa nhập tên!");
            return;
        }

        PlayerPrefs.SetString("SavedPlayerName", playerName);
        PlayerPrefs.Save();
        Debug.Log("Đã lưu hồ sơ nhân viên: " + playerName);

        namePanel.SetActive(false);

        // 3. Cho phép nhân vật đi lại và TỰ ĐỘNG GIẤU CHUỘT ĐI
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}