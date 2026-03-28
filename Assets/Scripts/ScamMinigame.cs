using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ScamRound
{
    public string victimMessage;
    [TextArea(2, 3)]
    public string scriptToType;
    public float timeLimit;

    [Header("Cản trở (Pop-up)")]
    public bool hasDistraction;
    public string distractionMessage;
}

public class ScamMinigame : MonoBehaviour
{
    [Header("Giao diện Điện thoại (CƠ BẢN)")]
    public GameObject phonePanel;
    public RectTransform phoneScreenRect;
    public TextMeshProUGUI chatHistoryText;

    // ĐÃ THÊM: Lỗ cắm để chứa toàn bộ cái thanh màu xanh
    [Tooltip("Kéo cục ChatBubble (cái nền màu xanh) vào đây để ẩn đi lúc thắng")]
    public GameObject playerChatBubble;
    public TextMeshProUGUI typingAreaText;
    public Slider timerSlider;

    [Header("Giao diện Điện thoại (MESSENGER MỚI)")]
    public Image headerAvatarImage;
    public Image chatAvatarImage;
    public TextMeshProUGUI victimNameText;
    public Button btnBlockVictim;

    [Header("Giao diện Pop-up Cản trở")]
    public GameObject distractionPanel;
    public TextMeshProUGUI distractionText;
    public Button closeDistractionButton;

    [Header("Âm thanh (Audio)")]
    public AudioSource audioSource;
    public AudioClip typingSound;
    public AudioClip waitingSound;
    public AudioClip messageSound;
    public AudioClip shockSound;

    [Header("Phần thưởng / Hình phạt")]
    public float maxMoneyReward = 10000f;
    public float bossBonus = 5000f;
    public int karmaPenalty = 20;

    [Header("--- CÔNG CỤ DEV ---")]
    public Button btnHiddenSkip;
    public Button btnHiddenFail;

    private ScamRound[] currentRounds;
    private int currentRoundIndex = 0;
    private int successfulRounds = 0;
    private int consecutiveFails = 0;

    private int currentTypedIndex = 0;
    private string wrongCharsTyped = "";
    private float timeRemaining;

    private bool isTypingPhase = false;
    private bool isResting = false;
    private bool isDistracted = false;

    private Coroutine distractionCoroutine;
    private string currentVictimName = "";

    void Start()
    {
        if (phonePanel != null) phonePanel.SetActive(false);
        if (distractionPanel != null) distractionPanel.SetActive(false);
        if (btnBlockVictim != null) btnBlockVictim.gameObject.SetActive(false);

        if (closeDistractionButton != null) closeDistractionButton.onClick.AddListener(CloseDistraction);
        if (btnHiddenSkip != null) btnHiddenSkip.onClick.AddListener(CheatSkipMinigame);
        if (btnHiddenFail != null) btnHiddenFail.onClick.AddListener(CheatFailMinigame);
    }

    void Update()
    {
        if (phonePanel != null && phonePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.F9)) { CheatSkipMinigame(); return; }
            if (Input.GetKeyDown(KeyCode.F10)) { CheatFailMinigame(); return; }
        }

        if (isDistracted && Input.GetKeyDown(KeyCode.Escape)) CloseDistraction();

        if (!isTypingPhase || isResting) return;

        timeRemaining -= Time.deltaTime;
        if (timerSlider != null) timerSlider.value = timeRemaining / currentRounds[currentRoundIndex].timeLimit;

        if (timeRemaining <= 0)
        {
            StartCoroutine(ProcessRoundEnd(false));
            return;
        }

        if (isDistracted) return;

        foreach (char c in Input.inputString)
        {
            if (c == '\n' || c == '\r' || c == (char)27) continue;

            if (audioSource != null && typingSound != null) audioSource.PlayOneShot(typingSound, 0.7f);

            string targetScript = currentRounds[currentRoundIndex].scriptToType;

            if (c == '\b')
            {
                if (wrongCharsTyped.Length > 0)
                {
                    wrongCharsTyped = wrongCharsTyped.Substring(0, wrongCharsTyped.Length - 1);
                    UpdateTypingUI();
                }
                continue;
            }

            if (wrongCharsTyped.Length < 15)
            {
                if (wrongCharsTyped.Length == 0 && char.ToLower(c) == char.ToLower(targetScript[currentTypedIndex]))
                {
                    currentTypedIndex++;
                    UpdateTypingUI();

                    if (currentTypedIndex >= targetScript.Length)
                    {
                        StartCoroutine(ProcessRoundEnd(true));
                        return;
                    }
                }
                else
                {
                    wrongCharsTyped += c;
                    UpdateTypingUI();
                }
            }
        }
    }

    public void CheatSkipMinigame() { StopAllCoroutines(); isDistracted = false; if (distractionPanel != null) distractionPanel.SetActive(false); EndGame(true, maxMoneyReward + bossBonus); }
    public void CheatFailMinigame() { if (!isTypingPhase) return; isTypingPhase = false; StartCoroutine(ProcessRoundEnd(false)); }

    public void StartMiniGame(ScamRound[] roundsData, string vName, Sprite vAvatar = null)
    {
        currentRounds = roundsData;
        currentVictimName = vName;
        currentRoundIndex = 0; successfulRounds = 0; consecutiveFails = 0;

        if (phonePanel != null) phonePanel.SetActive(true);
        if (distractionPanel != null) distractionPanel.SetActive(false);
        if (btnBlockVictim != null) btnBlockVictim.gameObject.SetActive(false);

        // ĐẢM BẢO BẬT THANH CHAT XANH LÊN KHI BẮT ĐẦU
        if (playerChatBubble != null) playerChatBubble.SetActive(true);

        if (victimNameText != null) victimNameText.text = currentVictimName;

        if (vAvatar != null)
        {
            if (headerAvatarImage != null) { headerAvatarImage.sprite = vAvatar; headerAvatarImage.gameObject.SetActive(true); }
            if (chatAvatarImage != null) { chatAvatarImage.sprite = vAvatar; chatAvatarImage.gameObject.SetActive(true); }
        }
        else
        {
            if (headerAvatarImage != null) headerAvatarImage.gameObject.SetActive(false);
            if (chatAvatarImage != null) chatAvatarImage.gameObject.SetActive(false);
        }

        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
        isTypingPhase = false; isResting = true; typingAreaText.text = "";
        if (timerSlider != null) timerSlider.value = 1f;

        chatHistoryText.text = $"<color=#FFFF00>Đang kết nối với {currentVictimName}...</color>";
        yield return new WaitForSeconds(2f);
        isResting = false; LoadRound(currentRoundIndex);
    }

    void LoadRound(int index)
    {
        ScamRound round = currentRounds[index];
        float difficulty = GameManager.instance != null ? GameManager.instance.typingDifficultyMultiplier : 1.0f;
        timeRemaining = round.timeLimit * difficulty;
        currentTypedIndex = 0; wrongCharsTyped = "";
        string msg = round.victimMessage;

        if (consecutiveFails == 1) msg = "<color=#FF9900>[Nhắn nhầm à? Gì vậy?]</color>\n" + msg;

        if (chatHistoryText != null) chatHistoryText.text = $"<color=#FFFFFF>{msg}</color>";
        if (audioSource != null && messageSound != null) audioSource.PlayOneShot(messageSound);
        isDistracted = false; if (distractionPanel != null) distractionPanel.SetActive(false);
        StartCoroutine(ThinkingPhase(round));
    }

    IEnumerator ThinkingPhase(ScamRound round)
    {
        isTypingPhase = false; isResting = true;
        for (int i = 3; i > 0; i--)
        {
            typingAreaText.text = $"<i><color=#888888>Đang suy nghĩ kịch bản lừa đảo... ({i}s)</color></i>";
            if (timerSlider != null) timerSlider.value = 1f;
            yield return new WaitForSeconds(1f);
        }
        isResting = false; isTypingPhase = true; UpdateTypingUI();
        if (round.hasDistraction) distractionCoroutine = StartCoroutine(TriggerDistraction(round.distractionMessage, round.timeLimit));
    }

    IEnumerator TriggerDistraction(string message, float totalTime)
    {
        // 1. Đợi một khoảng thời gian random rồi mới bất ngờ hiện Pop-up (từ 1 giây đến nửa thời gian)
        float randomDelay = Random.Range(1f, totalTime / 2f);
        yield return new WaitForSeconds(randomDelay);

        // 2. Nếu người chơi gõ quá nhanh, xong trước khi sếp chửi thì bỏ qua luôn
        if (!isTypingPhase) yield break;

        // 3. Khóa phím người chơi và nạp câu chửi
        isDistracted = true;
        if (distractionText != null) distractionText.text = message;

        // 4. --- BỘ TOÁN HỌC RANDOM VỊ TRÍ ---
        if (distractionPanel != null && phoneScreenRect != null)
        {
            RectTransform distRect = distractionPanel.GetComponent<RectTransform>();

            // Tính toán "Khung an toàn" để Pop-up không bay lọt mép ra ngoài màn hình
            float maxX = (phoneScreenRect.rect.width - distRect.rect.width) / 2f;
            float maxY = (phoneScreenRect.rect.height - distRect.rect.height) / 2f;

            // Random tọa độ X, Y trong vùng an toàn đó
            float randomX = Random.Range(-maxX, maxX);
            float randomY = Random.Range(-maxY, maxY);

            // Đặt vị trí mới cho bảng chửi
            distRect.anchoredPosition = new Vector2(randomX, randomY);
        }

        // 5. Hiện Pop-up lên màn hình
        if (distractionPanel != null) distractionPanel.SetActive(true);
    }
    public void CloseDistraction() { isDistracted = false; if (distractionPanel != null) distractionPanel.SetActive(false); }

    IEnumerator ProcessRoundEnd(bool isSuccess)
    {
        isTypingPhase = false;
        if (distractionCoroutine != null) StopCoroutine(distractionCoroutine);
        if (distractionPanel != null) distractionPanel.SetActive(false);

        if (isSuccess)
        {
            successfulRounds++; consecutiveFails = 0;
            typingAreaText.text = "<color=#00FF00>Đã gửi thành công!</color>";
            yield return new WaitForSeconds(1.5f);
            currentRoundIndex++;
            if (currentRoundIndex >= currentRounds.Length) CalculateFinalReward();
            else StartCoroutine(RestPhase());
        }
        else
        {
            consecutiveFails++;
            if (consecutiveFails == 1)
            {
                string targetScript = currentRounds[currentRoundIndex].scriptToType;
                string panicTyping = (currentTypedIndex > 0) ? targetScript.Substring(0, currentTypedIndex) + wrongCharsTyped + "gdh..." : targetScript.Split(' ')[0] + " asd...";
                typingAreaText.text = $"<color=#FF9900>Hết giờ! Bạn luống cuống bấm gửi:\n\"{panicTyping}\"</color>";
                yield return new WaitForSeconds(2.5f);
                currentRoundIndex++;
                if (currentRoundIndex >= currentRounds.Length) CalculateFinalReward();
                else StartCoroutine(RestPhase());
            }
            else if (consecutiveFails >= 2)
            {
                typingAreaText.text = "<color=#FF0000>Hết giờ! Bạn nổi điên gửi luôn:\n\"Tao lua may day! Nop tien vao!\"</color>";
                if (audioSource != null && shockSound != null) audioSource.PlayOneShot(shockSound);
                yield return new WaitForSeconds(2.5f);
                chatHistoryText.text = "<color=#FF0000>Nạn nhân đã phát hiện và Block bạn!</color>";
                typingAreaText.text = "";
                yield return new WaitForSeconds(3f);
                EndGame(false, 0);
            }
        }
    }

    IEnumerator RestPhase()
    {
        isResting = true;
        if (audioSource != null && waitingSound != null) { audioSource.clip = waitingSound; audioSource.loop = true; audioSource.Play(); }
        float restTime = 4f; float dotTimer = 0f; int dotCount = 0;

        while (restTime > 0)
        {
            if (timerSlider != null) timerSlider.value = restTime / 4f;
            restTime -= Time.deltaTime; dotTimer -= Time.deltaTime;
            if (dotTimer <= 0)
            {
                dotTimer = 0.5f; dotCount = (dotCount + 1) % 4;
                string dots = new string('.', dotCount);
                chatHistoryText.text = $"<i><color=#aaaaaa>đang gõ{dots}</color></i>";
            }
            yield return null;
        }
        if (audioSource != null) { audioSource.Stop(); audioSource.loop = false; }
        isResting = false; LoadRound(currentRoundIndex);
    }

    void CalculateFinalReward() { /* Giữ nguyên rút gọn */ FailEnd(); }
    void FailEnd() { EndGame(false, 0); }

    // ĐÂY LÀ CHỖ LOGIC ẨN THANH CHAT VÀ HIỆN NÚT BLOCK HOẠT ĐỘNG
    IEnumerator ShowSuccessAndWaitForBlock(string message, float finalMoney)
    {
        // 1. TẮT THANH MÀU XANH ĐI
        if (playerChatBubble != null) playerChatBubble.SetActive(false);

        // 2. HIỆN DÒNG THÔNG BÁO VÀ HƯỚNG DẪN BẤM CHẶN
        chatHistoryText.text = "<color=#00FF00>" + message + "</color>\n<color=#FFFF00>Lừa xong rồi! Hãy bấm nút [CHẶN] để thu tiền!</color>";

        // 3. HIỆN NÚT CHẶN (Nút này bạn có thể đặt nó to đùng giữa màn hình)
        if (btnBlockVictim != null)
        {
            btnBlockVictim.gameObject.SetActive(true);
            btnBlockVictim.onClick.RemoveAllListeners();
            btnBlockVictim.onClick.AddListener(() => {
                btnBlockVictim.gameObject.SetActive(false);
                EndGame(true, finalMoney);
            });
        }
        else
        {
            yield return new WaitForSeconds(3f);
            EndGame(true, finalMoney);
        }
    }

    void UpdateTypingUI()
    {
        if (typingAreaText == null || !isTypingPhase) return;
        string targetScript = currentRounds[currentRoundIndex].scriptToType;
        string typedPart = targetScript.Substring(0, currentTypedIndex);
        string wrongPart = wrongCharsTyped.Length > 0 ? $"<color=#FF0000><u>{wrongCharsTyped}</u></color>" : "";
        string remainingPart = targetScript.Substring(currentTypedIndex);
        typingAreaText.text = $"<color=#FFFFFF>{typedPart}</color>{wrongPart}<color=#DDDDDD>{remainingPart}</color>";
    }

    void EndGame(bool isSuccess, float moneyEarned)
    {
        isTypingPhase = false;
        if (phonePanel != null) phonePanel.SetActive(false);
        if (audioSource != null) { audioSource.Stop(); audioSource.loop = false; }
        if (GameManager.instance != null)
        {
            if (isSuccess) GameManager.instance.OnScamSuccess(moneyEarned, karmaPenalty);
            else GameManager.instance.OnScamFail();
        }
    }
}