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
    [Header("Giao diện Điện thoại")]
    public GameObject phonePanel;
    public RectTransform phoneScreenRect;
    public TextMeshProUGUI chatHistoryText;
    public TextMeshProUGUI typingAreaText;
    public Slider timerSlider;

    [Header("Giao diện Pop-up Cản trở")]
    public GameObject distractionPanel;
    public TextMeshProUGUI distractionText;
    public Button closeDistractionButton;

    [Header("Âm thanh (Audio) - KÉO FILE MP3 VÀO ĐÂY")]
    public AudioSource audioSource;
    public AudioClip typingSound;
    public AudioClip waitingSound;
    public AudioClip messageSound;
    public AudioClip shockSound;        // MỚI THÊM: Tiếng chích điện (Xẹt xẹt)

    [Header("Phần thưởng / Hình phạt")]
    public float maxMoneyReward = 10000f;
    public float bossBonus = 5000f;
    public int karmaPenalty = 20;

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

        if (closeDistractionButton != null)
        {
            closeDistractionButton.onClick.AddListener(CloseDistraction);
        }
    }

    void Update()
    {
        if (isDistracted && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDistraction();
        }

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

            if (audioSource != null && typingSound != null)
            {
                audioSource.PlayOneShot(typingSound, 0.7f);
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
                }
            }
        }
    }

    public void StartMiniGame(ScamRound[] roundsData, string vName)
    {
        currentRounds = roundsData;
        currentVictimName = vName;

        currentRoundIndex = 0;
        successfulRounds = 0;
        consecutiveFails = 0;

        if (phonePanel != null) phonePanel.SetActive(true);
        if (distractionPanel != null) distractionPanel.SetActive(false);

        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
        isTypingPhase = false;
        isResting = true;
        typingAreaText.text = "";
        if (timerSlider != null) timerSlider.value = 1f;

        chatHistoryText.text = $"<color=#FFFF00>Đang kết nối với {currentVictimName}...</color>";
        yield return new WaitForSeconds(1f);

        for (int i = 3; i > 0; i--)
        {
            chatHistoryText.text = $"<color=#FFFF00>Bắt đầu trong: {i}</color>";
            yield return new WaitForSeconds(1f);
        }

        isResting = false;
        LoadRound(currentRoundIndex);
    }

    void LoadRound(int index)
    {
        ScamRound round = currentRounds[index];
        timeRemaining = round.timeLimit;
        currentTypedIndex = 0;
        wrongCharsTyped = "";

        if (chatHistoryText != null)
            chatHistoryText.text = $"<color=#aaaaaa>{currentVictimName}:</color> {round.victimMessage}";

        if (audioSource != null && messageSound != null)
        {
            audioSource.PlayOneShot(messageSound);
        }

        isDistracted = false;
        if (distractionPanel != null) distractionPanel.SetActive(false);

        StartCoroutine(ThinkingPhase(round));
    }

    IEnumerator ThinkingPhase(ScamRound round)
    {
        isTypingPhase = false;
        isResting = true;

        for (int i = 5; i > 0; i--)
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

    IEnumerator TriggerDistraction(string message, float totalTime)
    {
        int spawnCount = 0;
        int maxSpawns = 7;

        while (isTypingPhase && spawnCount < maxSpawns)
        {
            float randomWait = Random.Range(1.5f, 3.5f);
            yield return new WaitForSeconds(randomWait);

            if (!isTypingPhase || timeRemaining <= 1.5f) break;

            isDistracted = true;
            spawnCount++;
            if (distractionText != null) distractionText.text = message;

            if (phoneScreenRect != null && distractionPanel != null && closeDistractionButton != null)
            {
                RectTransform distRect = distractionPanel.GetComponent<RectTransform>();
                RectTransform btnRect = closeDistractionButton.GetComponent<RectTransform>();

                float xMax = (phoneScreenRect.rect.width / 2) - (distRect.rect.width / 2);
                float yMax = (phoneScreenRect.rect.height / 2) - (distRect.rect.height / 2);

                float randomX = Random.Range(-xMax, xMax);
                float randomY = Random.Range(-yMax, yMax);
                distRect.anchoredPosition = new Vector2(randomX, randomY);

                float btnXMax = (distRect.rect.width / 2) - (btnRect.rect.width / 2);
                float btnYMax = (distRect.rect.height / 2) - (btnRect.rect.height / 2);

                float btnRandomX = Random.Range(-btnXMax, btnXMax);
                float btnRandomY = Random.Range(-btnYMax, btnYMax);
                btnRect.anchoredPosition = new Vector2(btnRandomX, btnRandomY);
            }

            if (distractionPanel != null) distractionPanel.SetActive(true);
            Input.ResetInputAxes();

            yield return new WaitUntil(() => !isDistracted || !isTypingPhase);
        }
    }

    public void CloseDistraction()
    {
        isDistracted = false;
        if (distractionPanel != null) distractionPanel.SetActive(false);
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

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
        }
        else
        {
            consecutiveFails++;

            if (consecutiveFails == 1)
            {
                string targetScript = currentRounds[currentRoundIndex].scriptToType;
                string panicTyping = "";

                if (currentTypedIndex > 0)
                    panicTyping = targetScript.Substring(0, currentTypedIndex) + wrongCharsTyped + "gdh...";
                else
                    panicTyping = targetScript.Split(' ')[0] + " asd...";

                typingAreaText.text = $"<color=#FF9900>Hết giờ! Bạn luống cuống bấm gửi:\n\"{panicTyping}\"</color>";
                yield return new WaitForSeconds(2.5f);
            }
            else if (consecutiveFails >= 2)
            {
                typingAreaText.text = "<color=#FF0000>Hết giờ! Bạn nổi điên gửi luôn:\n\"Tao lua may day! Nop tien vao!\"</color>";

                // ==========================================================
                // MỚI: PHÁT TIẾNG CHÍCH ĐIỆN VÀ TRỪ THẲNG 30 MÁU!
                // ==========================================================
                if (audioSource != null && shockSound != null)
                {
                    audioSource.PlayOneShot(shockSound); // Bật tiếng Xẹt xẹt
                }

                if (GameManager.instance != null)
                {
                    GameManager.instance.hp -= 30; // Trừ thẳng 30 máu
                    Debug.Log("Bị Block! Quản lý chích điện trừ 30 máu. Máu còn: " + GameManager.instance.hp);
                }

                yield return new WaitForSeconds(2.5f);

                StartCoroutine(ShowFailAndEndGame($"{currentVictimName} đã phát hiện và Block bạn!"));
                yield break;
            }
        }

        currentRoundIndex++;

        if (currentRoundIndex >= currentRounds.Length) CalculateFinalReward();
        else StartCoroutine(RestPhase());
    }

    IEnumerator RestPhase()
    {
        isResting = true;

        if (consecutiveFails == 1) chatHistoryText.text = $"<i>{currentVictimName} đang nghi ngờ và gõ chậm lại...</i>";
        else chatHistoryText.text = $"<i>{currentVictimName} đang gõ...</i>";

        if (audioSource != null && waitingSound != null)
        {
            audioSource.clip = waitingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        float restTime = 5f;
        while (restTime > 0)
        {
            if (timerSlider != null) timerSlider.value = restTime / 5f;
            restTime -= Time.deltaTime;
            yield return null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        isResting = false;
        LoadRound(currentRoundIndex);
    }

    void CalculateFinalReward()
    {
        if (successfulRounds < 2)
        {
            StartCoroutine(ShowFailAndEndGame("Lừa đảo thất bại. Nạn nhân không chuyển tiền."));
            return;
        }

        float moneyEarned = maxMoneyReward * ((float)successfulRounds / currentRounds.Length);
        string endMsg = "Đã lừa được " + moneyEarned + "$ (" + successfulRounds + "/5 câu).";

        if (successfulRounds == currentRounds.Length)
        {
            moneyEarned += bossBonus;
            endMsg = "HOÀN HẢO 5/5! Thưởng nóng từ Sếp: +" + bossBonus + "$!";
        }

        StartCoroutine(ShowSuccessAndEndGame(endMsg, moneyEarned));
    }

    IEnumerator ShowFailAndEndGame(string message)
    {
        chatHistoryText.text = "<color=#FF0000>" + message + "</color>";
        typingAreaText.text = "";
        yield return new WaitForSeconds(3f);
        EndGame(false, 0);
    }

    IEnumerator ShowSuccessAndEndGame(string message, float finalMoney)
    {
        chatHistoryText.text = "<color=#00FF00>" + message + "</color>";
        typingAreaText.text = "";
        yield return new WaitForSeconds(3f);
        EndGame(true, finalMoney);
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
        if (phonePanel != null) phonePanel.SetActive(false);

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (GameManager.instance != null)
        {
            if (isSuccess) GameManager.instance.OnScamSuccess(moneyEarned, karmaPenalty);
            else GameManager.instance.OnScamFail();
        }
    }
}