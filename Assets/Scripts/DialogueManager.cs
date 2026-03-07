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
    // Bùa chú Singleton để các Script khác gọi dễ dàng hơn
    public static DialogueManager instance;

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
    public string nextSceneName = "MainGameScene";

    [Header("Cài đặt Hiệu ứng")]
    public float typingSpeed = 0.05f;

    [Header("KỊCH BẢN 1: TỰ GIỚI THIỆU (Mới vào game)")]
    public DialogueLine[] introLines;

    [Header("KỊCH BẢN 2: RỦ RÊ (Sau khi xếp xong đồ)")]
    public DialogueLine[] outroLines;

    private DialogueLine[] currentLinesToPlay; // Dùng để xác định đang đọc kịch bản nào
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isPlayingIntro = false; // Cờ kiểm tra trạng thái
    private Coroutine typingCoroutine;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        choicePanel.SetActive(false);
        dialoguePanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false);

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
                dialogueText.text = currentLinesToPlay[currentLineIndex].sentence.Replace("[PLAYER]", pName);
                isTyping = false;
            }
            else
            {
                NextDialogueLine();
            }
        }
    }

    // ==========================================
    // HÀM MỚI: BẮT ĐẦU ĐOẠN TỰ GIỚI THIỆU
    // ==========================================
    public void StartIntroDialogue()
    {
        // Gán kịch bản 1 vào chạy
        currentLinesToPlay = introLines;
        isPlayingIntro = true;

        dialoguePanel.SetActive(true);
        currentLineIndex = 0;
        StartDialogue();
    }

    // ==========================================
    // HÀM CŨ: BẮT ĐẦU ĐOẠN RỦ RÊ CUỐI CA
    // ==========================================
    public void TriggerStoryEvent()
    {
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Gán kịch bản 2 vào chạy
        currentLinesToPlay = outroLines;
        isPlayingIntro = false;

        dialoguePanel.SetActive(true);
        currentLineIndex = 0;
        StartDialogue();
    }

    public void StartDialogue()
    {
        if (currentLinesToPlay.Length > 0)
        {
            ShowLine(currentLineIndex);
        }
    }

    private string GetPlayerName()
    {
        if (PlayerPrefs.HasKey("SavedPlayerName"))
        {
            string savedName = PlayerPrefs.GetString("SavedPlayerName");
            if (!string.IsNullOrEmpty(savedName)) return savedName;
        }
        if (GameManager.instance != null && !string.IsNullOrEmpty(GameManager.instance.playerName))
            return GameManager.instance.playerName;
        return "Bạn";
    }

    private void ShowLine(int index)
    {
        DialogueLine currentLine = currentLinesToPlay[index];
        string pName = GetPlayerName();

        if (currentLine.speakerName == "[PLAYER]") nameText.text = pName;
        else nameText.text = currentLine.speakerName;

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
        if (currentLineIndex < currentLinesToPlay.Length)
        {
            ShowLine(currentLineIndex);
        }
        else
        {
            // XỬ LÝ RẼ NHÁNH: Đọc xong thì làm gì?
            if (isPlayingIntro)
            {
                // Nếu đọc xong đoạn tự giới thiệu -> Tắt bảng, THẢ CHO NGƯỜI CHƠI ĐI LẠI
                dialoguePanel.SetActive(false);

                PlayerMovement player = FindObjectOfType<PlayerMovement>();
                if (player != null) player.canMove = true;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                CrosshairController crosshair = FindObjectOfType<CrosshairController>();
                if (crosshair != null) crosshair.Show();
            }
            else
            {
                // Nếu đọc xong đoạn rủ rê đi Campuchia -> Bật 2 nút Đồng ý / Từ chối
                dialoguePanel.SetActive(false);
                choicePanel.SetActive(true);
            }
        }
    }

    private void OnAgreeClicked()
    {
        Debug.Log("Đã chọn ĐỒNG Ý. Đang tải màn hình lừa đảo...");
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnRefuseClicked()
    {
        choicePanel.SetActive(false);
        dialoguePanel.SetActive(false);

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
            if (endingText != null)
                endingText.text = "Bạn đã từ chối lời đề nghị. Cuộc sống sinh viên nghèo vẫn tiếp diễn, nhưng ít ra bạn được bình yên.\n\n<color=#FF0000>ENDING 1: BẠN SỢ RỒI!</color>";
        }
        StartCoroutine(EndGameSequence());
    }

    IEnumerator EndGameSequence()
    {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }
}