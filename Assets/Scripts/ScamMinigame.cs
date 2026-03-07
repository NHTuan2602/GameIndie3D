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
    public bool hasDistraction; // Hiệp này có bị Sếp chửi không?
    public string distractionMessage; // Nội dung câu chửi
}

public class ScamMinigame : MonoBehaviour
{
    [Header("Giao diện Điện thoại")]
    public GameObject phonePanel;
    public TextMeshProUGUI chatHistoryText;
    public TextMeshProUGUI typingAreaText;
    public Slider timerSlider;

    [Header("Giao diện Pop-up Cản trở")]
    public GameObject distractionPanel;
    public TextMeshProUGUI distractionText;
    public Button closeDistractionButton;

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
    private bool isDistracted = false; // Biến khóa gõ phím khi có Pop-up

    private Coroutine distractionCoroutine; // Lưu trữ luồng Pop-up để hủy khi hết giờ

    void Start()
    {
        if (phonePanel != null) phonePanel.SetActive(false);
        if (distractionPanel != null) distractionPanel.SetActive(false);

        // Gắn lệnh tự động cho nút X của Pop-up
        if (closeDistractionButton != null)
        {
            closeDistractionButton.onClick.AddListener(CloseDistraction);
        }
    }

    void Update()
    {
        // Phím tắt để tắt Pop-up cực nhanh
        if (isDistracted && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDistraction();
        }

        if (!isTypingPhase || isResting) return;

        // ĐỒNG HỒ VẪN CHẠY KỂ CẢ KHI CÓ POP-UP MÀN HÌNH!
        timeRemaining -= Time.deltaTime;
        if (timerSlider != null) timerSlider.value = timeRemaining / currentRounds[currentRoundIndex].timeLimit;

        if (timeRemaining <= 0)
        {
            StartCoroutine(ProcessRoundEnd(false));
            return;
        }

        // Nếu đang bị Pop-up che thì KHÔNG CHO GÕ
        if (isDistracted) return;

        // XỬ LÝ GÕ PHÍM CHÂN THỰC
        foreach (char c in Input.inputString)
        {
            if (c == '\n' || c == '\r' || c == (char)27) continue; // Bỏ qua phím Enter và ESC

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

    public void StartMiniGame(ScamRound[] roundsData)
    {
        currentRounds = roundsData;
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

        chatHistoryText.text = "<color=#FFFF00>Đang kết nối với Nạn nhân...</color>";
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
            chatHistoryText.text = "<color=#aaaaaa>Nạn nhân:</color> " + round.victimMessage;

        isTypingPhase = true;
        isDistracted = false;
        if (distractionPanel != null) distractionPanel.SetActive(false);

        UpdateTypingUI();

        // KÍCH HOẠT POP-UP NẾU HIỆP NÀY BỊ CÀI CẮM
        if (round.hasDistraction)
        {
            distractionCoroutine = StartCoroutine(TriggerDistraction(round.distractionMessage, round.timeLimit));
        }
    }

    // =========================================================
    // HỆ THỐNG GÂY ỨC CHẾ: BẬT VÀ TẮT POP-UP
    // =========================================================
    IEnumerator TriggerDistraction(string message, float totalTime)
    {
        // Chờ một khoảng thời gian ngẫu nhiên (từ 2s đến nửa thời gian hiệp)
        float randomWait = Random.Range(2f, totalTime / 2f);
        yield return new WaitForSeconds(randomWait);

        // Chỉ bật Pop-up nếu thời gian còn dư nhiều hơn 3 giây (Tránh chết oan)
        if (timeRemaining > 3f && isTypingPhase)
        {
            isDistracted = true;
            if (distractionText != null) distractionText.text = message;
            if (distractionPanel != null) distractionPanel.SetActive(true);

            // Xóa sạch các phím đang ngậm trong bộ nhớ đệm Input của Unity
            Input.ResetInputAxes();
        }
    }

    public void CloseDistraction()
    {
        isDistracted = false;
        if (distractionPanel != null) distractionPanel.SetActive(false);

        // Đưa con trỏ focus về trạng thái bình thường để gõ phím mượt mà
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    // =========================================================
    // XỬ LÝ KẾT THÚC VÀ CẬP NHẬT UI
    // =========================================================
    IEnumerator ProcessRoundEnd(bool isSuccess)
    {
        isTypingPhase = false;

        // Dừng luôn cái Pop-up nếu nó đang chuẩn bị bật lên
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
                yield return new WaitForSeconds(2.5f);
                StartCoroutine(ShowFailAndEndGame("Nạn nhân đã phát hiện và Block bạn!"));
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

        if (consecutiveFails == 1) chatHistoryText.text = "<i>Nạn nhân đang nghi ngờ và gõ chậm lại...</i>";
        else chatHistoryText.text = "<i>Nạn nhân đang gõ...</i>";

        float restTime = 5f;
        while (restTime > 0)
        {
            if (timerSlider != null) timerSlider.value = restTime / 5f;
            restTime -= Time.deltaTime;
            yield return null;
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

        typingAreaText.text = $"<color=#00FF00>{typedPart}</color>{wrongPart}<color=#888888>{remainingPart}</color>";
    }

    void EndGame(bool isSuccess, float moneyEarned)
    {
        if (phonePanel != null) phonePanel.SetActive(false);

        if (GameManager.instance != null)
        {
            if (isSuccess) GameManager.instance.OnScamSuccess(moneyEarned, karmaPenalty);
            else GameManager.instance.OnScamFail();
        }
    }

  
}