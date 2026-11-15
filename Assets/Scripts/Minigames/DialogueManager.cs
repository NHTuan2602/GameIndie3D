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
    [Tooltip("Gợi ý tiếp tục (Nhấn SPACE/Click)")]
    public TextMeshProUGUI continuePromptText;

    [Header("Âm thanh Typewriter")]
    public AudioSource audioSource;
    public AudioClip typeSound;
    [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;

    [Header("Cài đặt Hiệu ứng")]
    public float typingSpeed = 0.04f;

    private DialogueLine[] currentLines;
    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Coroutine promptPulseCoroutine;

    private System.Action onDialogueCompleteCallback;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (continuePromptText != null) continuePromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                string pName = GetPlayerName();
                dialogueText.text = currentLines[currentIndex].sentence.Replace("[PLAYER]", pName);
                isTyping = false;
                ShowContinuePrompt();
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

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
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
        if (continuePromptText != null) continuePromptText.gameObject.SetActive(false);

        DialogueLine line = currentLines[currentIndex];
        string pName = GetPlayerName();

        string resolvedSpeaker = (line.speakerName == "[PLAYER]") ? pName : line.speakerName;
        if (nameText != null)
        {
            // Thiết kế huy hiệu tên người nói sắc nét
            nameText.text = $"<b><color=#FFD166>◆ {resolvedSpeaker} ◆</color></b>";
        }

        string finalSentence = line.sentence.Replace("[PLAYER]", pName);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(finalSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        for (int i = 0; i < sentence.Length; i++)
        {
            char letter = sentence[i];
            dialogueText.text += letter;

            if (letter != ' ' && audioSource != null && typeSound != null)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(typeSound);
            }

            // Nghỉ nhịp tự nhiên tại dấu câu để tăng cảm xúc kịch tính
            if (letter == '.' || letter == '!' || letter == '?')
            {
                yield return new WaitForSeconds(typingSpeed * 4.5f);
            }
            else if (letter == ',' || letter == ';' || letter == ':')
            {
                yield return new WaitForSeconds(typingSpeed * 2.2f);
            }
            else
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        isTyping = false;
        ShowContinuePrompt();
    }

    private void ShowContinuePrompt()
    {
        if (continuePromptText != null)
        {
            continuePromptText.gameObject.SetActive(true);
            continuePromptText.text = "<size=80%><color=#A8DADC>▼ Nhấn [SPACE] hoặc [CLICK] để tiếp tục</color></size>";
            if (promptPulseCoroutine != null) StopCoroutine(promptPulseCoroutine);
            promptPulseCoroutine = StartCoroutine(PromptPulseRoutine());
        }
    }

    IEnumerator PromptPulseRoutine()
    {
        while (continuePromptText != null && continuePromptText.gameObject.activeSelf)
        {
            float alpha = Mathf.PingPong(Time.time * 2.5f, 0.6f) + 0.4f;
            Color c = continuePromptText.color;
            continuePromptText.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
    }

    private void NextDialogueLine()
    {
        if (continuePromptText != null) continuePromptText.gameObject.SetActive(false);

        currentIndex++;
        if (currentIndex < currentLines.Length)
        {
            ShowLine();
        }
        else
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null) { player.canWalk = true; player.canLook = true; }

            CrosshairController crosshair = FindFirstObjectByType<CrosshairController>();
            if (crosshair != null) crosshair.Show();

            if (onDialogueCompleteCallback != null)
            {
                onDialogueCompleteCallback.Invoke();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
