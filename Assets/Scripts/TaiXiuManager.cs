using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TaiXiuManager : MonoBehaviour
{
    [Header("Thời gian cược (Giây)")]
    public float bettingDuration = 55f;

    [Header("Giao diện UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    public Button btnTai;
    public Button btnXiu;

    [Header("Xúc Xắc & Chén")]
    public Image[] diceImages;
    public Sprite[] diceFaces;
    public DraggableBowl bowlObject;

    [Header("Cài đặt Cược")]
    public int betAmount = 100;

    private float currentTimer;
    private bool isBettingPhase = false;

    // 0: Chưa cược, 1: Tài, 2: Xỉu
    private int playerChoice = 0;
    private int totalDiceValue = 0;

    void Start()
    {
        // Gắn kết nối ngầm cho bát
        if (bowlObject != null) bowlObject.manager = this;

        if (btnTai != null) btnTai.onClick.AddListener(() => PlaceBet(1));
        if (btnXiu != null) btnXiu.onClick.AddListener(() => PlaceBet(2));

        StartNewRound();
    }

    void Update()
    {
        if (isBettingPhase)
        {
            currentTimer -= Time.deltaTime;
            timerText.text = Mathf.Ceil(currentTimer).ToString() + "s";

            if (currentTimer <= 0)
            {
                StartCoroutine(RollDiceRoutine());
            }
        }
    }

    public void StartNewRound()
    {
        playerChoice = 0;
        currentTimer = bettingDuration;
        isBettingPhase = true;

        if (bowlObject != null)
        {
            bowlObject.gameObject.SetActive(true);
            bowlObject.ResetPosition();
        }

        btnTai.interactable = true;
        btnXiu.interactable = true;
        statusText.text = "VUI LÒNG ĐẶT CƯỢC!";
        statusText.color = Color.white;
    }

    public void PlaceBet(int choice)
    {
        if (!isBettingPhase) return;

        playerChoice = choice;
        btnTai.interactable = false;
        btnXiu.interactable = false;

        string choiceName = (choice == 1) ? "TÀI" : "XỈU";
        statusText.text = $"Đã cược {choiceName}. Chờ xóc đĩa...";
        statusText.color = Color.yellow;
    }

    IEnumerator RollDiceRoutine()
    {
        isBettingPhase = false;
        timerText.text = "0s";

        if (playerChoice == 0)
        {
            statusText.text = "BẠN CHƯA CƯỢC! Bỏ qua phiên này.";
            yield return new WaitForSeconds(2f);
            StartNewRound();
            yield break;
        }

        statusText.text = "ĐANG XÓC ĐĨA...";
        statusText.color = Color.red;

        // Xúc xắc chạy ngẫu nhiên 1.5 giây
        float rollTime = 1.5f;
        while (rollTime > 0)
        {
            for (int i = 0; i < diceImages.Length; i++)
            {
                int randomFace = Random.Range(0, 6);
                diceImages[i].sprite = diceFaces[randomFace];
            }
            rollTime -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // Chốt kết quả ngầm
        totalDiceValue = 0;
        for (int i = 0; i < diceImages.Length; i++)
        {
            int finalFace = Random.Range(0, 6);
            diceImages[i].sprite = diceFaces[finalFace];
            totalDiceValue += (finalFace + 1);
        }

        statusText.text = "<size=120%>KÉO MẠNH BÁT RA ĐỂ XEM!</size>";
        statusText.color = Color.cyan;
        if (bowlObject != null) bowlObject.isDraggable = true;
    }

    public void CheckResult()
    {
        int winningChoice = (totalDiceValue >= 11) ? 1 : 2;
        string resultName = (winningChoice == 1) ? "TÀI" : "XỈU";

        if (playerChoice == winningChoice)
        {
            statusText.text = $"KẾT QUẢ: {totalDiceValue} - {resultName}!\n<color=#00FF00>BẠN THẮNG {betAmount}$</color>";
        }
        else
        {
            statusText.text = $"KẾT QUẢ: {totalDiceValue} - {resultName}!\n<color=#FF0000>BẠN THUA {betAmount}$</color>";
        }

        StartCoroutine(WaitAndRestart());
    }

    IEnumerator WaitAndRestart()
    {
        yield return new WaitForSeconds(4f);
        StartNewRound();
    }
}