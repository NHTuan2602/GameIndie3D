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
    public TextMeshProUGUI typingAreaText;
    public Slider timerSlider;

    [Header("Giao diện Điện thoại (MESSENGER MỚI)")]
    public Image victimAvatarImage;     // Kéo Image Avatar vào đây
    public TextMeshProUGUI victimNameText; // Kéo Text Tên nạn nhân vào đây (nếu có)
    public Button btnBlockVictim;       // Kéo nút CHẶN vào đây

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
        if (btnBlockVictim != null) btnBlockVictim.gameObject.SetActive(false); // Giấu nút Block đi lúc mới vào

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

    public void CheatSkipMinigame()
    {
        StopAllCoroutines();
        isDistracted = false;
        if (distractionPanel != null) distractionPanel.SetActive(false);
        EndGame(true, maxMoneyReward + bossBonus);
    }

    public void CheatFailMinigame()
    {
        if (!isTypingPhase) return;
        isTypingPhase = false;
        StartCoroutine(ProcessRoundEnd(false));
    }

    // ĐÃ NÂNG CẤP: Nhận thêm biến Sprite Avatar (Để có thể truyền avatar từng người vào)
    public void StartMiniGame(ScamRound[] roundsData, string vName, Sprite vAvatar = null)
    {
        currentRounds = roundsData;
        currentVictimName = vName;

        currentRoundIndex = 0;
        successfulRounds = 0;
        consecutiveFails = 0;

        if (phonePanel != null) phonePanel.SetActive(true);
        if (distractionPanel != null) distractionPanel.SetActive(false);
        if (btnBlockVictim != null) btnBlockVictim.gameObject.SetActive(false);

        if (victimNameText != null) victimNameText.text = currentVictimName;
        if (victimAvatarImage != null && vAvatar != null) victimAvatarImage.sprite = vAvatar;

        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
        isTypingPhase = false;
        isResting = true;
        typingAreaText.text = "";
        if (timerSlider != null) timerSlider.value = 1f;

        chatHistoryText.text = $"<color=#FFFF00>Đang kết nối với {currentVictimName}...</color>";
        yield return new WaitForSeconds(2f);

        isResting = false;
        LoadRound(currentRoundIndex);
    }

    void LoadRound(int index)
    {
        ScamRound round = currentRounds[index];

        // ĐÃ NÂNG CẤP: Thời gian bị ép ngắn lại nếu leo chức cao!
        float difficulty = GameManager.instance != null ? GameManager.instance.typingDifficultyMultiplier : 1.0f;
        timeRemaining = round.timeLimit * difficulty;

        currentTypedIndex = 0;
        wrongCharsTyped = "";

        string msg = round.victimMessage;

        if (consecutiveFails == 1)
        {
            msg = "<color=#FF9900>[Nhắn nhầm à? Gì vậy?]</color>\n" + msg;
        }

        if (chatHistoryText != null)
            chatHistoryText.text = $"<color=#aaaaaa>{currentVictimName}:</color> {msg}";

        if (audioSource != null && messageSound != null) audioSource.PlayOneShot(messageSound);

        isDistracted = false;
        if (distractionPanel != null) distractionPanel.SetActive(false);

        StartCoroutine(ThinkingPhase(round));
    }
    IEnumerator ThinkingPhase(ScamRound round)
    {
        isTypingPhase = false;
        isResting = true;

        for (int i = 3; i > 0; i--)
        {
            typingAreaText.text = $"<i><color=#888888>Đang suy nghĩ kịch bản lừa đảo... ({i}s)</color></i>";
            if (timerSlider != null) timerSlider.value = 1f;
            yield return new WaitForSeconds(1f);
        }

        isResting = false;
        isTypingPhase = true;
        UpdateTypingUI();

        if (round.hasDistraction)
        {
            distractionCoroutine = StartCoroutine(TriggerDistraction(round.distractionMessage, round.timeLimit));
        }
    }

    IEnumerator TriggerDistraction(string message, float totalTime) { /* Code giữ nguyên để tiết kiệm dòng */ yield break; }
    public void CloseDistraction() { isDistracted = false; if (distractionPanel != null) distractionPanel.SetActive(false); }

    IEnumerator ProcessRoundEnd(bool isSuccess)
    {
        isTypingPhase = false;

        if (distractionCoroutine != null) StopCoroutine(distractionCoroutine);
        if (distractionPanel != null) distractionPanel.SetActive(false);

        if (isSuccess)
        {
            successfulRounds++;
            consecutiveFails = 0;
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

                EndGame(false, 0); // Thua 2 lần -> Thoát luônn
            }
        }
    }

    // TÍNH NĂNG MỚI: Hiệu ứng 3 chấm nảy lặp đi lặp lại
    IEnumerator RestPhase()
    {
        isResting = true;

        if (audioSource != null && waitingSound != null)
        {
            audioSource.clip = waitingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        float restTime = 4f; // Cho nghỉ 4 giây
        float dotTimer = 0f;
        int dotCount = 0;

        string prefix = (consecutiveFails == 1) ? "đang gõ" : "đang gõ";

        while (restTime > 0)
        {
            if (timerSlider != null) timerSlider.value = restTime / 4f;

            restTime -= Time.deltaTime;
            dotTimer -= Time.deltaTime;

            // Mỗi 0.5s nhảy thêm 1 dấu chấm
            if (dotTimer <= 0)
            {
                dotTimer = 0.5f;
                dotCount = (dotCount + 1) % 4; // Nhảy 0, 1, 2, 3
                string dots = new string('.', dotCount);
                chatHistoryText.text = $"<i><color=#aaaaaa>{currentVictimName} {prefix}{dots}</color></i>";
            }

            yield return null;
        }

        if (audioSource != null) { audioSource.Stop(); audioSource.loop = false; }

        isResting = false;
        LoadRound(currentRoundIndex);
    }

    void CalculateFinalReward()
    {
        if (successfulRounds < 2)
        {
            chatHistoryText.text = "<color=#FF0000>Lừa đảo thất bại. Nạn nhân không chuyển tiền.</color>";
            typingAreaText.text = "";
            Invoke("FailEnd", 3f);
            return;
        }

        // Tính tiền USD lừa được
        float rawMoneyUSDEarned = maxMoneyReward * ((float)successfulRounds / currentRounds.Length);
        if (successfulRounds == currentRounds.Length) rawMoneyUSDEarned += bossBonus;

        // Lấy % hoa hồng từ Sếp để hiển thị
        float commissionRate = GameManager.instance != null ? GameManager.instance.currentCommissionRate * 100f : 10f;

        string endMsg = $"Khách đã chuyển <color=#00FF00>{rawMoneyUSDEarned}$</color>!\nSếp chia <color=#FFFF00>{commissionRate}%</color> hoa hồng.";

        StartCoroutine(ShowSuccessAndWaitForBlock(endMsg, rawMoneyUSDEarned));
    }

    void FailEnd() { EndGame(false, 0); }

    // TÍNH NĂNG MỚI: Dừng lại chờ bấm nút Block
    IEnumerator ShowSuccessAndWaitForBlock(string message, float finalMoney)
    {
        chatHistoryText.text = "<color=#00FF00>" + message + "</color>";
        typingAreaText.text = "<color=#FFFF00>Lừa xong rồi! Hãy bấm nút [CHẶN] để thu tiền!</color>";

        if (btnBlockVictim != null)
        {
            btnBlockVictim.gameObject.SetActive(true);
            btnBlockVictim.onClick.RemoveAllListeners();
            btnBlockVictim.onClick.AddListener(() => {
                // Khi bấm nút thì tắt nút đi, nhét tiền vào túi và đóng màn hình
                btnBlockVictim.gameObject.SetActive(false);
                EndGame(true, finalMoney);
            });
        }
        else
        {
            // Tránh lỗi nếu bạn quên tạo nút Block
            Debug.Log("Không tìm thấy nút Block! Tự động kết thúc sau 3 giây.");
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