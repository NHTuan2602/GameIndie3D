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

    public GameObject playerChatBubble;
    public TextMeshProUGUI typingAreaText;
    public Slider timerSlider;

    [Header("Giao diện Điện thoại (MESSENGER)")]
    public Image headerAvatarImage;
    public Image chatAvatarImage;
    public TextMeshProUGUI victimNameText;
    public Button btnBlockVictim;

    [Header("Giao diện Pop-up Cản trở")]
    public GameObject distractionPanel;
    public TextMeshProUGUI distractionText;
    public Button closeDistractionButton;

    [Header("--- HIỆU ỨNG CĂNG THẲNG (THEME) ---")]
    public Image dangerVignette;
    public AudioSource bgmSource;
    public AudioClip bgmNormal;
    public AudioClip tickTockSound;
    public AudioClip errorKeystrokeSound;

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
    public int shockDamage = 30;

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
    private bool isAutoPlayTroll = false;

    private bool isPanicMode = false;

    private Coroutine distractionCoroutine;
    private string currentVictimName = "";
    private Vector3 originalPhonePos;

    void Start()
    {
        if (phonePanel != null) phonePanel.SetActive(false);
        if (distractionPanel != null) distractionPanel.SetActive(false);
        if (btnBlockVictim != null) btnBlockVictim.gameObject.SetActive(false);
        if (dangerVignette != null) dangerVignette.color = new Color(1, 0, 0, 0);

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

        if (!isTypingPhase || isResting || isAutoPlayTroll) return;

        timeRemaining -= Time.deltaTime;
        if (timerSlider != null) timerSlider.value = timeRemaining / currentRounds[currentRoundIndex].timeLimit;

        if (timeRemaining <= 5f && !isPanicMode)
        {
            isPanicMode = true;
            if (bgmSource != null) bgmSource.Stop();
            if (audioSource != null && tickTockSound != null)
            {
                audioSource.PlayOneShot(tickTockSound, 1.2f);
            }
        }

        if (isPanicMode && dangerVignette != null)
        {
            float alpha = Mathf.PingPong(Time.time * 4f, 0.4f);
            dangerVignette.color = new Color(1, 0, 0, alpha);
        }

        if (timeRemaining <= 0)
        {
            StartCoroutine(ProcessRoundEnd(false));
            return;
        }

        if (isDistracted) return;

        foreach (char c in Input.inputString)
        {
            if (c == '\n' || c == '\r' || c == (char)27) continue;

            if (audioSource != null && typingSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(typingSound, 0.7f);
                audioSource.pitch = 1f;
            }

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
                    if (audioSource != null && errorKeystrokeSound != null) audioSource.PlayOneShot(errorKeystrokeSound, 0.5f);
                    StartCoroutine(MicroShake());
                }
            }
        }
    }

    public void StartMiniGame(ScamRound[] roundsData, string vName, Sprite vAvatar = null, bool isTroll = false)
    {
        currentRounds = roundsData;
        currentVictimName = vName;
        isAutoPlayTroll = isTroll;

        currentRoundIndex = 0; successfulRounds = 0; consecutiveFails = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (phonePanel != null)
        {
            phonePanel.SetActive(true);
            originalPhonePos = phonePanel.transform.localPosition;
        }

        if (distractionPanel != null) distractionPanel.SetActive(false);
        if (btnBlockVictim != null) btnBlockVictim.gameObject.SetActive(false);
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

        if (timerSlider != null) timerSlider.gameObject.SetActive(!isAutoPlayTroll);

        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
        isTypingPhase = false; isResting = true; typingAreaText.text = "";
        ResetPanicMode();
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

        ResetPanicMode();

        if (consecutiveFails == 1)
        {
            string[] wtfReactions = new string[]
            {
                "??? Bạn nhắn cái ngôn ngữ gì vậy?",
                "Bị lag à? Gõ chữ kiểu gì đấy?",
                "Đang nhắn tin rớt điện thoại vào mặt à? Viết lại xem nào?",
                "Ủa alo? Chó mèo đi ngang qua bàn phím à?"
            };
            string randomReaction = wtfReactions[Random.Range(0, wtfReactions.Length)];
            msg = $"<color=#FF9900>[{randomReaction}]</color>\n" + msg;
        }

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
            if (timerSlider != null && !isAutoPlayTroll) timerSlider.value = 1f;
            yield return new WaitForSeconds(1f);
        }
        isResting = false; isTypingPhase = true; UpdateTypingUI();

        if (isAutoPlayTroll) StartCoroutine(AutoTypeRoutine(round));
        else if (round.hasDistraction) distractionCoroutine = StartCoroutine(TriggerDistraction(round.distractionMessage, round.timeLimit));
    }

    void ResetPanicMode()
    {
        isPanicMode = false;
        if (dangerVignette != null) dangerVignette.color = new Color(1, 0, 0, 0);
        if (bgmSource != null && bgmNormal != null)
        {
            bgmSource.clip = bgmNormal;
            if (!bgmSource.isPlaying) bgmSource.Play();
        }
    }

    IEnumerator MicroShake()
    {
        if (phonePanel == null) yield break;
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            phonePanel.transform.localPosition = originalPhonePos + (Vector3)Random.insideUnitCircle * 5f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        phonePanel.transform.localPosition = originalPhonePos;
    }

    IEnumerator AutoTypeRoutine(ScamRound round)
    {
        string targetScript = round.scriptToType;
        float delayPerChar = 0.05f;

        for (int i = 0; i < targetScript.Length; i++)
        {
            currentTypedIndex++;
            UpdateTypingUI();
            if (audioSource != null && typingSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(typingSound, 0.7f);
                audioSource.pitch = 1f;
            }

            if (targetScript[i] == ' ' || targetScript[i] == ',' || targetScript[i] == '.') yield return new WaitForSeconds(0.12f);
            else yield return new WaitForSeconds(delayPerChar);
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(ProcessRoundEnd(true));
    }

    IEnumerator TriggerDistraction(string message, float totalTime)
    {
        float randomDelay = Random.Range(1f, totalTime / 2f);
        yield return new WaitForSeconds(randomDelay);

        if (!isTypingPhase) yield break;

        isDistracted = true;
        if (distractionText != null) distractionText.text = message;

        if (distractionPanel != null && phoneScreenRect != null)
        {
            RectTransform distRect = distractionPanel.GetComponent<RectTransform>();
            float maxX = (phoneScreenRect.rect.width - distRect.rect.width) / 2f;
            float maxY = (phoneScreenRect.rect.height - distRect.rect.height) / 2f;

            distRect.anchoredPosition = new Vector2(Random.Range(-maxX, maxX), Random.Range(-maxY, maxY));
        }

        if (distractionPanel != null) distractionPanel.SetActive(true);
    }

    public void CloseDistraction() { isDistracted = false; if (distractionPanel != null) distractionPanel.SetActive(false); }

    IEnumerator ProcessRoundEnd(bool isSuccess)
    {
        isTypingPhase = false;
        ResetPanicMode();
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
                string[] panicEndings = { " ak...", " ủa...", " nhâm...", " ggh...", " v.v..." };
                string randomEnding = panicEndings[Random.Range(0, panicEndings.Length)];

                string panicTyping = (currentTypedIndex > 0)
                    ? targetScript.Substring(0, currentTypedIndex) + wrongCharsTyped + randomEnding
                    : targetScript.Split(' ')[0] + randomEnding;

                typingAreaText.text = $"<color=#FF9900>Hết giờ! Bạn luống cuống bấm gửi:\n\"{panicTyping}\"</color>";

                yield return new WaitForSeconds(2.5f);
                currentRoundIndex++;
                if (currentRoundIndex >= currentRounds.Length) CalculateFinalReward();
                else StartCoroutine(RestPhase());
            }
            else if (consecutiveFails >= 2)
            {
                string[] rageQuits = new string[]
                {
                    "Đm lừa mệt quá, nạp tiền vào lẹ đi!",
                    "Tao lừa mày đấy! Thằng ngu, block đây!"
                };
                string randomRage = rageQuits[Random.Range(0, rageQuits.Length)];

                typingAreaText.text = $"<color=#FF0000>Hết giờ! Bạn nổi điên chửi luôn:\n\"{randomRage}\"</color>";

                // ĐÃ FIX: XÓA CHÍCH ĐIỆN 30 HP Ở ĐÂY. CUỐI NGÀY MỚI PHẠT!
                Debug.Log("<color=yellow>Bị block! Báo về GameManager ghi nhận 1 lần Thất Bại.</color>");

                if (audioSource != null && shockSound != null) audioSource.PlayOneShot(shockSound);
                StartCoroutine(PhoneShakeEffect());

                yield return new WaitForSeconds(2.5f);
                chatHistoryText.text = "<color=#FF0000>Nạn nhân đã phát hiện và Block bạn!</color>";
                typingAreaText.text = "";
                yield return new WaitForSeconds(3f);
                FailEnd();
            }
        }
    }

    IEnumerator PhoneShakeEffect()
    {
        if (phonePanel == null) yield break;
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            phonePanel.transform.localPosition = originalPhonePos + new Vector3(Random.Range(-1f, 1f) * 15f, Random.Range(-1f, 1f) * 15f, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        phonePanel.transform.localPosition = originalPhonePos;
    }

    IEnumerator RestPhase()
    {
        isResting = true;
        if (audioSource != null && waitingSound != null) { audioSource.clip = waitingSound; audioSource.loop = true; audioSource.Play(); }
        float restTime = 4f; float dotTimer = 0f; int dotCount = 0;

        while (restTime > 0)
        {
            if (timerSlider != null && !isAutoPlayTroll) timerSlider.value = restTime / 4f;
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

    void CalculateFinalReward()
    {
        float finalMoney = 0f;
        string message = "";

        if (successfulRounds == currentRounds.Length)
        {
            finalMoney = maxMoneyReward + bossBonus;
            message = isAutoPlayTroll ? "KỊCH BẢN KẾT THÚC! Bạn đã troll thành công!" : "LỪA ĐẢO HOÀN HẢO! Nạn nhân đã sập bẫy hoàn toàn. Chuyển khoản thành công!";
        }
        else if (successfulRounds >= currentRounds.Length / 2)
        {
            finalMoney = maxMoneyReward * 0.5f;
            message = "LỪA ĐẢO TẠM ỔN. Nạn nhân có chút nghi ngờ nhưng vẫn chuyển một nửa số tiền.";
        }
        else
        {
            FailEnd();
            return;
        }

        StartCoroutine(ShowSuccessAndWaitForBlock(message, finalMoney));
    }

    void FailEnd() { EndGame(false, 0); }

    IEnumerator ShowSuccessAndWaitForBlock(string message, float finalMoney)
    {
        if (playerChatBubble != null) playerChatBubble.SetActive(false);

        string hintText = isAutoPlayTroll ? "Troll xong rồi! Hãy bấm [CHẶN] để kết thúc màn kịch!" : "Lừa xong rồi! Hãy bấm nút [CHẶN] để thu tiền!";
        chatHistoryText.text = "<color=#00FF00>" + message + "</color>\n<color=#FFFF00>" + hintText + "</color>";

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

    public void CheatSkipMinigame()
    {
        StopAllCoroutines();
        isDistracted = false;
        if (distractionPanel != null) distractionPanel.SetActive(false);
        Debug.Log("<color=magenta>DEV CHEAT: Bỏ qua và THẮNG!</color>");
        EndGame(true, maxMoneyReward + bossBonus);
    }

    public void CheatFailMinigame()
    {
        if (!isTypingPhase) return;
        isTypingPhase = false;

        // ĐÃ FIX: Chặn việc Spam F10 gọi ra nhiều vòng xử lý làm treo game!
        StopAllCoroutines();

        Debug.Log("<color=magenta>DEV CHEAT: Ép THUA ngay lập tức!</color>");
        StartCoroutine(ProcessRoundEnd(false));
    }

    void EndGame(bool isSuccess, float moneyEarned)
    {
        isTypingPhase = false;
        ResetPanicMode();
        if (phonePanel != null) phonePanel.SetActive(false);
        if (audioSource != null) { audioSource.Stop(); audioSource.loop = false; }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GameManager.instance != null)
        {
            if (isSuccess) GameManager.instance.OnScamSuccess(moneyEarned, karmaPenalty);
            else GameManager.instance.OnScamFail();
        }
    }
}