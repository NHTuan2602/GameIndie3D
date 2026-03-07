using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 5)]
    public string sentence;
}

public class DialogueManager : MonoBehaviour
{
    [Header("Giao diện UI Hội thoại")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Giao diện Lựa chọn")]
    public GameObject choicePanel;
    public Button agreeButton;
    public Button refuseButton;

    [Header("Giao diện Ending (Màn hình đen)")]
    public GameObject endingPanel;
    public TextMeshProUGUI endingText;

    [Header("Cài đặt Chuyển Màn")]
    [Tooltip("Gõ chính xác tên Scene Minigame lừa đảo của bạn vào đây")]
    public string nextSceneName = "MainGameScene";

    [Header("Cài đặt Hiệu ứng")]
    public float typingSpeed = 0.05f;

    [Header("Kịch bản Hội thoại (Dùng [PLAYER] để thay tên)")]
    public DialogueLine[] dialogueLines;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        // Ẩn tất cả khi mới vào game
        choicePanel.SetActive(false);
        dialoguePanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false); // Ẩn màn hình ending

        agreeButton.onClick.AddListener(OnAgreeClicked);
        refuseButton.onClick.AddListener(OnRefuseClicked);
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);

                string pName = GetPlayerName();
                dialogueText.text = dialogueLines[currentLineIndex].sentence.Replace("[PLAYER]", pName);

                isTyping = false;
            }
            else
            {
                NextDialogueLine();
            }
        }
    }

    public void TriggerStoryEvent()
    {
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        dialoguePanel.SetActive(true);
        currentLineIndex = 0;
        StartDialogue();
    }

    public void StartDialogue()
    {
        if (dialogueLines.Length > 0)
        {
            ShowLine(currentLineIndex);
        }
    }

    private string GetPlayerName()
    {
        // Ưu tiên 1: Đọc tên trực tiếp từ bộ nhớ mà IntroManager vừa lưu
        if (PlayerPrefs.HasKey("SavedPlayerName"))
        {
            string savedName = PlayerPrefs.GetString("SavedPlayerName");
            if (!string.IsNullOrEmpty(savedName))
            {
                return savedName; // Trả về tên thật (Ví dụ: "hưng")
            }
        }

        // Ưu tiên 2: Nếu lỡ quên lưu ổ cứng thì hỏi GameManager (Kế hoạch dự phòng)
        if (GameManager.instance != null && !string.IsNullOrEmpty(GameManager.instance.playerName))
        {
            return GameManager.instance.playerName;
        }

        // Đường cùng: Máy tính hỏng, data mất hết thì mới dùng chữ "Bạn"
        return "Bạn";
    }

    private void ShowLine(int index)
    {
        DialogueLine currentLine = dialogueLines[index];
        string pName = GetPlayerName();

        if (currentLine.speakerName == "[PLAYER]")
            nameText.text = pName;
        else
            nameText.text = currentLine.speakerName;

        string finalSentence = currentLine.sentence.Replace("[PLAYER]", pName);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(finalSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void NextDialogueLine()
    {
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length) ShowLine(currentLineIndex);
        else ShowChoices();
    }

    private void ShowChoices()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(true);
    }

    // =======================================================
    // NÚT ĐỒNG Ý: CHUYỂN QUA MÀN GÕ PHÍM
    // =======================================================
    private void OnAgreeClicked()
    {
        Debug.Log("Đã chọn ĐỒNG Ý. Đang tải màn hình lừa đảo...");

        // Load qua Scene gõ phím
        SceneManager.LoadScene(nextSceneName);
    }

    // =======================================================
    // NÚT TỪ CHỐI: MÀN HÌNH ĐEN ENDING
    // =======================================================
    private void OnRefuseClicked()
    {
        Debug.Log("Đã chọn TỪ CHỐI. Kích hoạt Ending 1.");

        // Tắt hết các UI hội thoại
        choicePanel.SetActive(false);
        dialoguePanel.SetActive(false);

        // Bật màn hình đen Ending lên
        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
            if (endingText != null)
            {
                // Set nội dung chữ cho ngầu
                endingText.text = "Bạn đã từ chối lời đề nghị. Cuộc sống sinh viên nghèo vẫn tiếp diễn, nhưng ít ra bạn được bình yên.\n\n<color=#FF0000>ENDING 1: BẠN SỢ RỒI!</color>";
            }
        }

        // GỌI HÀM ĐẾM NGƯỢC ĐỂ KẾT THÚC GAME
        StartCoroutine(EndGameSequence());
    }

    // Luồng đếm ngược tự động thoát
    IEnumerator EndGameSequence()
    {
        // Đợi 5 giây cho người chơi đủ thời gian đọc dòng chữ Ending
        yield return new WaitForSeconds(5f);

        Debug.Log("Đang thoát game...");

        // Nếu chạy trong Unity Editor thì lệnh này vô tác dụng, nhưng khi Build ra file .exe thì nó sẽ tự văng game
        Application.Quit();

        // Nếu bạn có Scene Main Menu, có thể thay dòng Quit() bằng:
        // SceneManager.LoadScene("MainMenuScene");
    }
}