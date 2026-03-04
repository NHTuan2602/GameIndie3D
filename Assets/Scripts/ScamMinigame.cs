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
}

public class ScamMinigame : MonoBehaviour
{
    [Header("Giao diện Điện thoại")]
    public GameObject phonePanel;
    public TextMeshProUGUI chatHistoryText;
    public TextMeshProUGUI typingAreaText;
    public Slider timerSlider;

    [Header("Phần thưởng / Hình phạt")]
    public float maxMoneyReward = 10000f;
    public float bossBonus = 5000f;
    public int karmaPenalty = 20;

    private ScamRound[] currentRounds;
    private int currentRoundIndex = 0;

    private int successfulRounds = 0;
    private int consecutiveFails = 0;

    private int currentTypedIndex = 0;
    private float timeRemaining;

    private bool isTypingPhase = false;
    private bool isResting = false;

    void Start()
    {
        if (phonePanel != null) phonePanel.SetActive(false);
    }

    void Update()
    {
        if (!isTypingPhase || isResting) return;

        // 1. ĐẾM NGƯỢC THỜI GIAN
        timeRemaining -= Time.deltaTime;
        if (timerSlider != null) timerSlider.value = timeRemaining / currentRounds[currentRoundIndex].timeLimit;

        if (timeRemaining <= 0)
        {
            // HẾT GIỜ -> Chạy luồng xử lý thất bại
            StartCoroutine(ProcessRoundEnd(false));
            return;
        }

        // 2. XỬ LÝ GÕ PHÍM (SAI LÀ XÓA SẠCH)
        foreach (char c in Input.inputString)
        {
            if (c == '\b' || c == '\n' || c == '\r') continue;

            string targetScript = currentRounds[currentRoundIndex].scriptToType;

            if (char.ToLower(c) == char.ToLower(targetScript[currentTypedIndex]))
            {
                currentTypedIndex++;
                UpdateTypingUI();

                if (currentTypedIndex >= targetScript.Length)
                {
                    // GÕ XONG -> Chạy luồng xử lý thành công
                    StartCoroutine(ProcessRoundEnd(true));
                    return;
                }
            }
            else
            {
                currentTypedIndex = 0;
                UpdateTypingUI();
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

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = false;

        LoadRound(currentRoundIndex);
    }

    void LoadRound(int index)
    {
        ScamRound round = currentRounds[index];
        timeRemaining = round.timeLimit;
        currentTypedIndex = 0;

        if (chatHistoryText != null)
            chatHistoryText.text = "<color=#aaaaaa>Nạn nhân:</color> " + round.victimMessage;

        UpdateTypingUI();
        isTypingPhase = true;
    }

    // =========================================================
    // XỬ LÝ KẾT THÚC HIỆP VÀ CÁC PHA "LỠ TAY" KHÔNG ĐỠ ĐƯỢC
    // =========================================================
    IEnumerator ProcessRoundEnd(bool isSuccess)
    {
        isTypingPhase = false; // Tạm khóa bàn phím và đồng hồ

        if (isSuccess)
        {
            successfulRounds++;
            consecutiveFails = 0; // Reset đếm ngược nghi ngờ
            typingAreaText.text = "<color=#00FF00>Đã gửi thành công!</color>";
            yield return new WaitForSeconds(1.5f); // Đợi 1.5s rồi mới nghỉ
        }
        else
        {
            consecutiveFails++;

            // XỬ LÝ THEO SỐ LẦN HẾT GIỜ (TIMEOUT)
            if (consecutiveFails == 1)
            {
                // Lần 1: Luống cuống nhắn nhầm, lộ sơ hở
                typingAreaText.text = "<color=#FF9900>Hết giờ! Bạn luống cuống bấm gửi nhầm:\n\"Nap tien le di ban oi\"</color>";
                yield return new WaitForSeconds(2.5f); // Dừng lại 2.5s để người chơi tự dằn vặt
            }
            else if (consecutiveFails >= 2)
            {
                // Lần 2: Mất kiên nhẫn, chửi luôn nạn nhân
                typingAreaText.text = "<color=#FF0000>Hết giờ! Bạn nổi điên gửi luôn:\n\"Tao lua may day! Nop tien vao!\"</color>";
                yield return new WaitForSeconds(2.5f);

                // Block ngay lập tức, kết thúc game luôn
                StartCoroutine(ShowFailAndEndGame("Nạn nhân đã phát hiện và Block bạn!"));
                yield break; // Ngắt Coroutine tại đây, không cho chạy tiếp nữa
            }
        }

        currentRoundIndex++;

        if (currentRoundIndex >= currentRounds.Length)
        {
            CalculateFinalReward();
        }
        else
        {
            StartCoroutine(RestPhase());
        }
    }

    IEnumerator RestPhase()
    {
        isResting = true;

        // Nếu vừa bị lỗi lần 1, nạn nhân sẽ sinh nghi
        if (consecutiveFails == 1)
        {
            chatHistoryText.text = "<i>Nạn nhân đang nghi ngờ và gõ chậm lại...</i>";
        }
        else
        {
            chatHistoryText.text = "<i>Nạn nhân đang gõ...</i>";
        }

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

    // =========================================================
    // TÍNH TOÁN TIỀN VÀ KẾT THÚC
    // =========================================================
    void CalculateFinalReward()
    {
        if (successfulRounds < 2)
        {
            StartCoroutine(ShowFailAndEndGame("Lừa đảo thất bại. Nạn nhân không chuyển tiền."));
            return;
        }

        float moneyEarned = maxMoneyReward * ((float)successfulRounds / currentRounds.Length);
        string endMsg = "Đã lừa được " + moneyEarned + "$ (" + successfulRounds + "/5 câu).";

        if (successfulRounds == 5)
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
        string remainingPart = targetScript.Substring(currentTypedIndex);

        typingAreaText.text = "<color=#00FF00>" + typedPart + "</color><color=#888888>" + remainingPart + "</color>";
    }

    void EndGame(bool isSuccess, float moneyEarned)
    {
        if (phonePanel != null) phonePanel.SetActive(false);

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = true;

        if (GameManager.instance != null)
        {
            if (isSuccess) GameManager.instance.OnScamSuccess(moneyEarned, karmaPenalty);
            else GameManager.instance.OnScamFail();
        }
    }

    // NÚT TEST TỰ ĐỘNG
    public void TestStart()
    {
        ScamRound[] testRounds = new ScamRound[5];

        testRounds[0] = new ScamRound { victimMessage = "Alo, cong viec gi vay em?", scriptToType = "Chao chi, ben em la cong ty thuong mai.", timeLimit = 15f };
        testRounds[1] = new ScamRound { victimMessage = "Chi lam bot thoi gian duoc khong?", scriptToType = "Viec nhe luong cao chi ranh luc nao lam luc do.", timeLimit = 15f };
        testRounds[2] = new ScamRound { victimMessage = "The chi phai lam gi?", scriptToType = "Chi chi can an like san pham roi nhan tien thoi.", timeLimit = 12f };
        testRounds[3] = new ScamRound { victimMessage = "O the thoi a? De chi thu.", scriptToType = "Vang chi chuyen khoan 500k de mo tai khoan nhe.", timeLimit = 12f };
        testRounds[4] = new ScamRound { victimMessage = "Sao bao khong mat phi?", scriptToType = "Tien nay de dam bao thoi sau nay rut duoc chi a.", timeLimit = 10f };

        StartMiniGame(testRounds);
    }
}