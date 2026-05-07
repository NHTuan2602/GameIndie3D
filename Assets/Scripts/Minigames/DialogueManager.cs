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
            // KẾT THÚC HỘI THOẠI
            dialoguePanel.SetActive(false);

            // =====================================
            // ĐÃ FIX: CHIA LÀM 2 TRƯỜNG HỢP
            // =====================================
            if (onDialogueCompleteCallback != null)
            {
                // TRƯỜNG HỢP 1: Có sự kiện nối tiếp (Bật Menu Chọn, Chuyển Scene...)
                // -> CHỈ gọi sự kiện, KHÔNG trả lại quyền đi lại và KHÔNG khóa chuột!
                onDialogueCompleteCallback.Invoke();
            }
            else
            {
                // TRƯỜNG HỢP 2: Hội thoại bình thường xong
                // -> Mở khóa chân cẳng, giấu chuột đi để chơi tiếp
                PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
                if (player != null) { player.canWalk = true; player.canLook = true; }

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                CrosshairController crosshair = FindFirstObjectByType<CrosshairController>();
                if (crosshair != null) crosshair.Show();
            }
        }
    }
}