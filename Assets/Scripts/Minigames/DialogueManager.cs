using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 5)]
    public string sentence;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("Giao diện UI Hội thoại")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Âm thanh Typewriter")]
    public AudioSource audioSource;
    public AudioClip typeSound;
    [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;

    [Header("Cài đặt Hiệu ứng")]
    public float typingSpeed = 0.05f;

    private DialogueLine[] currentLines;
    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private System.Action onDialogueCompleteCallback;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                string pName = GetPlayerName();
                dialogueText.text = currentLines[currentIndex].sentence.Replace("[PLAYER]", pName);
                isTyping = false;
            }
            else
            {
                NextDialogueLine();
            }
        }
    }

    public void StartDialogue(DialogueLine[] lines, System.Action onComplete = null)
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) { player.canWalk = false; player.canLook = false; }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CrosshairController crosshair = FindFirstObjectByType<CrosshairController>();
        if (crosshair != null) crosshair.Hide();

        currentLines = lines;
        onDialogueCompleteCallback = onComplete;
        currentIndex = 0;

        dialoguePanel.SetActive(true);
        ShowLine();
    }

    private string GetPlayerName()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName");
            if (!string.IsNullOrEmpty(savedName)) return savedName;
        }
        if (GameManager.instance != null && !string.IsNullOrEmpty(GameManager.instance.playerName))
            return GameManager.instance.playerName;
        return "Bạn";
    }

    private void ShowLine()
    {
        DialogueLine line = currentLines[currentIndex];
        string pName = GetPlayerName();

        nameText.text = (line.speakerName == "[PLAYER]") ? pName : line.speakerName;
        string finalSentence = line.sentence.Replace("[PLAYER]", pName);

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

            if (letter != ' ' && audioSource != null && typeSound != null)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(typeSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void NextDialogueLine()
    {
        currentIndex++;
        if (currentIndex < currentLines.Length)
        {
            ShowLine();
        }
        else
        {
            // 1. Tắt bảng hội thoại
            dialoguePanel.SetActive(false);

            // 2. PHẦN MỚI: TỰ ĐỘNG DỌN DẸP TRẠNG THÁI TRƯỚC KHI GỌI CALLBACK
            // Dọn dẹp trạng thái điều khiển (Dùng chung cho cả 2 trường hợp)
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null) { player.canWalk = true; player.canLook = true; }

            CrosshairController crosshair = FindFirstObjectByType<CrosshairController>();
            if (crosshair != null) crosshair.Show();

            // 3. XỬ LÝ LOGIC RIÊNG
            if (onDialogueCompleteCallback != null)
            {
                // Gọi sự kiện nối tiếp
                onDialogueCompleteCallback.Invoke();

                // NẾU CALLBACK CỦA BẠN LÀ CHUYỂN CẢNH, THÌ KHÔNG CẦN KHÓA CHUỘT
                // NHƯNG NẾU LÀ BẬT MENU TRONG CẢNH, BẠN CẦN CHUỘT.
                // Ở ĐÂY TÔI KHÔNG KHÓA CHUỘT ĐỂ TRÁNH GÂY LỖI CHO CÁC MENU TƯƠNG TÁC
            }
            else
            {
                // Chỉ khóa chuột khi là hội thoại bình thường không cần tương tác gì thêm
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}