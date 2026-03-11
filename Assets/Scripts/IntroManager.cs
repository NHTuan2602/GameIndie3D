using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("Giao diện Nhập Tên")]
    public GameObject namePanel;
    public CanvasGroup namePanelCanvasGroup;
    public TMP_InputField nameInputField;
    public Button submitButton;
    public CrosshairController crosshairController;

    [Header("Cài đặt Hiệu ứng")]
    public float fadeDuration = 1.5f; // Thời gian mờ dần (1.5 giây là đẹp nhất)

    void Start()
    {
        namePanel.SetActive(true);

        // Đảm bảo màn hình rõ nét và cho phép click chuột lúc mới vào
        if (namePanelCanvasGroup != null)
        {
            namePanelCanvasGroup.alpha = 1f;
            namePanelCanvasGroup.blocksRaycasts = true;
        }

        // 1. Mở khóa chuột để người chơi gõ tên
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Trói chân và Khóa cổ nhân vật (MỚI SỬA CẬP NHẬT THEO PLAYERMOVEMENT MỚI)
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.canWalk = false; // Không cho đi lại
            player.canLook = false; // Không cho xoay đầu nhìn lung tung lúc nhập tên
        }

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

        // LƯU TÊN VÀO Ổ CỨNG VÀ GAMEMANAGER
        PlayerPrefs.SetString("SavedPlayerName", playerName);
        PlayerPrefs.Save();

        if (GameManager.instance != null)
        {
            GameManager.instance.playerName = playerName;
        }

        Debug.Log("Đã lưu hồ sơ nhân viên: " + playerName);

        // KHỞI ĐỘNG HIỆU ỨNG MỜ DẦN THAY VÌ TẮT CÁI RỤP
        StartCoroutine(FadeOutAndStartGame());
    }

    IEnumerator FadeOutAndStartGame()
    {
        submitButton.interactable = false;
        nameInputField.interactable = false;
        if (namePanelCanvasGroup != null) namePanelCanvasGroup.blocksRaycasts = false;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            if (namePanelCanvasGroup != null)
            {
                namePanelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            }
            yield return null;
        }

        namePanel.SetActive(false);

        // ==============================================================
        // MỚI SỬA: Thay vì thả cho đi lại, GỌI ĐOẠN TỰ GIỚI THIỆU DẬY
        // ==============================================================
        // Nghỉ 1 giây cho người chơi ngắm cảnh siêu thị
        yield return new WaitForSeconds(1.0f);

        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartIntroDialogue();
        }
        else
        {
            Debug.LogError("Chưa gắn kịch bản DialogueManager vô scene!");
        }
    }
}