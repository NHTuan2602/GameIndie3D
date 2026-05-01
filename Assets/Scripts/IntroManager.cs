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
    public float fadeDuration = 1.5f;

    [Header("--- KỊCH BẢN TỰ GIỚI THIỆU ---")]
    public DialogueLine[] introLines; // ĐÃ THÊM: Trả kịch bản về đây để tự quản lý

    void Start()
    {
        namePanel.SetActive(true);

        if (namePanelCanvasGroup != null)
        {
            namePanelCanvasGroup.alpha = 1f;
            namePanelCanvasGroup.blocksRaycasts = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.canWalk = false;
            player.canLook = false;
        }

        submitButton.onClick.AddListener(OnSubmitName);
    }

    void OnSubmitName()
    {
        string rawInput = nameInputField.text;
        string playerName = rawInput.Replace("\u200B", "").Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Chưa nhập tên! Vui lòng nhập để tiếp tục.");
            return;
        }

        PlayerPrefs.SetString("SavedPlayerName", playerName);
        PlayerPrefs.Save();

        if (GameManager.instance != null)
        {
            GameManager.instance.playerName = playerName;
        }

        Debug.Log("Đã lưu hồ sơ nhân viên: " + playerName);
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

        yield return new WaitForSeconds(1.0f);

        // ==============================================================
        // ĐÃ FIX: Gọi hàm vạn năng mới của DialogueManager
        // ==============================================================
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(introLines, null);
        }
        else
        {
            Debug.LogError("Chưa gắn kịch bản DialogueManager vô scene!");
        }
    }
}