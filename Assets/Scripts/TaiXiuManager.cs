using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaiXiuManager : MonoBehaviour
{
    [Header("Thời gian cược (Giây)")]
    public float bettingDuration = 55f;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    public Button btnTai;
    public Button btnXiu;
    public Image[] diceImages;
    public Sprite[] diceFaces;
    public DraggableBowl bowlObject;

    [Header("--- HỆ THỐNG TIỀN TỆ ---")]
    public int betAmount = 0;
    public TextMeshProUGUI txtPlayerMoney;
    public TextMeshProUGUI txtBetAmount;

    [Header("Ảnh Mệnh Giá Tiền")]
    public Sprite[] moneySprites;

    [Header("Hiệu Ứng Quăng Tiền")]
    public GameObject chipPrefab;
    public Transform pendingArea;
    public Transform taiArea;
    public Transform xiuArea;

    [Header("Nút Cược & Tương Tác")]
    public Button btn50k;
    public Button btn100k;
    public Button btn200k;
    public Button btn500k;
    public Button btnClearBet;
    public Button btnAllIn;
    public Button btnCloseCasino;

    [Header("--- KẾT NỐI VỚI BÀN 3D ---")]
    public MinigameInteract interactPoint;

    [Header("--- CÀI ĐẶT XÚC XẮC ---")]
    public float rollDuration = 7f;
    public float spawnRadius = 35f;
    public float diceSpacing = 40f;

    [Header("--- ÂM THANH ---")]
    public AudioSource audioSource;
    public AudioSource bgmSource;

    public AudioClip bgmClip;
    public AudioClip coinSound;
    public AudioClip shakeSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("--- ĐÈN CHÚC MỪNG ---")]
    public Image imgLightTai;
    public Image imgLightXiu;
    private Coroutine flashingCoroutine;

    private List<GameObject> spawnedChips = new List<GameObject>();
    private Vector2[] originalDicePos;
    private float currentTimer;
    private bool isBettingPhase = false;
    private int playerChoice = 0;
    private int totalDiceValue = 0;

    void Start()
    {
        if (bowlObject != null) bowlObject.manager = this;

        if (btnTai != null) btnTai.onClick.AddListener(() => PlaceBet(1));
        if (btnXiu != null) btnXiu.onClick.AddListener(() => PlaceBet(2));
        if (btn50k != null) btn50k.onClick.AddListener(() => AddMoneyToTable(50000, 0));
        if (btn100k != null) btn100k.onClick.AddListener(() => AddMoneyToTable(100000, 1));
        if (btn200k != null) btn200k.onClick.AddListener(() => AddMoneyToTable(200000, 2));
        if (btn500k != null) btn500k.onClick.AddListener(() => AddMoneyToTable(500000, 3));
        if (btnClearBet != null) btnClearBet.onClick.AddListener(ClearBetUI);
        if (btnAllIn != null) btnAllIn.onClick.AddListener(BetAllIn);
        if (btnCloseCasino != null) btnCloseCasino.onClick.AddListener(CloseCasino);

        originalDicePos = new Vector2[diceImages.Length];
        for (int i = 0; i < diceImages.Length; i++)
        {
            if (diceImages[i] != null) originalDicePos[i] = diceImages[i].rectTransform.anchoredPosition;
        }

        // ĐÃ XÓA TỰ ĐỘNG CHẠY Ở ĐÂY (Sửa Góc khuất 1)
    }

    // ================== HÀM MỚI: ĐÁNH THỨC SỚI BẠC ==================
    public void OpenCasino()
    {
        // 1. Bật nhạc nền
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // 2. Bắt đầu đếm ngược ván mới
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

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void UpdateMoneyUI()
    {
        if (txtPlayerMoney != null) txtPlayerMoney.text = $"Ví tiền: {GameManager.instance.money:N0} VNĐ";
        if (txtBetAmount != null) txtBetAmount.text = $"Đang cược: {betAmount:N0} VNĐ";
    }

    public void AddMoneyToTable(int amount, int spriteIndex)
    {
        if (!isBettingPhase || playerChoice != 0) return;

        if (GameManager.instance.money >= amount)
        {
            GameManager.instance.money -= amount;
            betAmount += amount;
            UpdateMoneyUI();
            PlaySound(coinSound);

            if (chipPrefab != null && pendingArea != null && moneySprites.Length > spriteIndex)
            {
                SpawnSingleChip(spriteIndex);
            }
        }
        else
        {
            statusText.text = "KHÔNG ĐỦ TIỀN ĐỂ CƯỢC!";
            statusText.color = Color.red;
        }
    }

    public void BetAllIn()
    {
        if (!isBettingPhase || playerChoice != 0) return;

        if (GameManager.instance.money <= 0)
        {
            statusText.text = "BẠN KHÔNG CÒN CÁI NỊT NÀO ĐỂ TẤT TAY!";
            statusText.color = Color.red;
            return;
        }

        int allInAmount = Mathf.FloorToInt(GameManager.instance.money);
        GameManager.instance.money = 0;
        betAmount += allInAmount;
        UpdateMoneyUI();
        PlaySound(coinSound);

        int[] chipValues = { 500000, 200000, 100000, 50000 };
        int[] chipSpriteIndices = { 3, 2, 1, 0 };

        int tempAmount = allInAmount;
        int visualChipCount = 0;
        int maxVisualChips = 40;

        for (int i = 0; i < chipValues.Length; i++)
        {
            int numChipsOfThisValue = tempAmount / chipValues[i];

            if (numChipsOfThisValue > 0)
            {
                for (int j = 0; j < numChipsOfThisValue; j++)
                {
                    if (visualChipCount < maxVisualChips)
                    {
                        SpawnSingleChip(chipSpriteIndices[i]);
                        visualChipCount++;
                    }
                }
                tempAmount %= chipValues[i];
            }
        }

        statusText.text = "TẤT TAY!!! KHÔ MÁU VÁN NÀY!";
        statusText.color = Color.yellow;
    }

    private void SpawnSingleChip(int spriteIndex)
    {
        if (chipPrefab == null || pendingArea == null || moneySprites.Length <= spriteIndex) return;

        GameObject newChip = Instantiate(chipPrefab, pendingArea);
        Image chipImage = newChip.GetComponent<Image>();
        if (chipImage != null && moneySprites[spriteIndex] != null)
        {
            chipImage.sprite = moneySprites[spriteIndex];
        }

        float randomX = Random.Range(-60f, 60f);
        float randomY = Random.Range(-40f, 40f);
        newChip.GetComponent<RectTransform>().anchoredPosition = new Vector2(randomX, randomY);
        newChip.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, Random.Range(-30f, 30f));
        spawnedChips.Add(newChip);
    }

    public void ClearBetUI()
    {
        if (!isBettingPhase) return;
        ClearBetCore();
    }

    private void ClearBetCore()
    {
        if (playerChoice != 0) return;
        if (betAmount <= 0) return;

        GameManager.instance.money += betAmount;
        betAmount = 0;
        UpdateMoneyUI();
        PlaySound(coinSound);

        foreach (GameObject chip in spawnedChips) Destroy(chip);
        spawnedChips.Clear();
    }

    // ================== HÀM ĐƯỢC NÂNG CẤP: ĐỨNG LÊN ==================
    public void CloseCasino()
    {
        // 1. Tiêu diệt toàn bộ "Bóng ma xúc xắc" đang chạy ngầm
        StopAllCoroutines();
        isBettingPhase = false;

        // 2. Trả lại tiền rác đang vứt trên bàn
        ClearBetCore();

        // 3. Tắt nhạc sòng bạc
        if (bgmSource != null) bgmSource.Stop();

        // 4. Mở khóa nhân vật
        if (interactPoint != null)
        {
            interactPoint.ExitMinigame();
        }
        else
        {
            gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void StartNewRound()
    {
        playerChoice = 0;
        currentTimer = bettingDuration;
        isBettingPhase = true;
        betAmount = 0;
        foreach (GameObject chip in spawnedChips) Destroy(chip);
        spawnedChips.Clear();
        UpdateMoneyUI();

        if (flashingCoroutine != null) StopCoroutine(flashingCoroutine);
        ResetAreaLights();

        if (bowlObject != null)
        {
            bowlObject.gameObject.SetActive(true);
            bowlObject.ResetPosition();
        }
        SetButtonsState(true);
        statusText.text = "CHỌN TIỀN VÀ CHỐT TÀI/XỈU!";
        statusText.color = Color.white;
    }

    public void PlaceBet(int choice)
    {
        if (!isBettingPhase) return;
        if (betAmount <= 0)
        {
            statusText.text = "BẠN CHƯA ĐẶT ĐỒNG NÀO!";
            statusText.color = Color.red;
            return;
        }
        playerChoice = choice;
        SetButtonsState(false);
        PlaySound(coinSound);

        string choiceName = (choice == 1) ? "TÀI" : "XỈU";
        statusText.text = $"Đã chốt {betAmount:N0} VNĐ vào {choiceName}!";
        statusText.color = Color.yellow;
        Transform targetArea = (choice == 1) ? taiArea : xiuArea;
        if (targetArea != null)
        {
            foreach (GameObject chip in spawnedChips)
            {
                chip.transform.SetParent(targetArea);
                float randX = Random.Range(-70f, 70f);
                float randY = Random.Range(-40f, 40f);
                chip.GetComponent<RectTransform>().anchoredPosition = new Vector2(randX, randY);
            }
        }
    }

    // ================== HÀM NÂNG CẤP: NHẬN DIỆN KHÁN GIẢ ==================
    void SetButtonsState(bool state)
    {
        if (btnTai != null) btnTai.interactable = state;
        if (btnXiu != null) btnXiu.interactable = state;
        if (btn50k != null) btn50k.interactable = state;
        if (btn100k != null) btn100k.interactable = state;
        if (btn200k != null) btn200k.interactable = state;
        if (btn500k != null) btn500k.interactable = state;
        if (btnClearBet != null) btnClearBet.interactable = state;
        if (btnAllIn != null) btnAllIn.interactable = state;

        // Xử lý góc khuất Nút Thoát
        if (btnCloseCasino != null)
        {
            if (playerChoice == 0)
            {
                // Nếu là khán giả (chưa chốt cược), cho phép bấm nút Thoát bất cứ lúc nào!
                btnCloseCasino.interactable = true;
            }
            else
            {
                // Nếu đã lỡ cược rồi thì bị khóa lại, chịu trận xem hết ván.
                btnCloseCasino.interactable = state;
            }
        }
    }

    IEnumerator RollDiceRoutine()
    {
        isBettingPhase = false;
        timerText.text = "0s";
        SetButtonsState(false);

        if (playerChoice == 0)
        {
            if (betAmount > 0)
            {
                statusText.text = "HẾT GIỜ! HOÀN TRẢ TIỀN RÁC TRÊN BÀN.";
                statusText.color = Color.green;
                ClearBetCore();
            }
            else
            {
                statusText.text = "BẠN ĐANG LÀ KHÁN GIẢ VÁN NÀY.";
                statusText.color = Color.white;
            }
            yield return new WaitForSeconds(1.5f);
        }

        statusText.text = "ĐANG XÓC ĐĨA...";
        statusText.color = Color.red;

        float rollTime = rollDuration;
        float soundTimer = 0f;

        while (rollTime > 0)
        {
            if (soundTimer <= 0f)
            {
                PlaySound(shakeSound);
                soundTimer = shakeSound != null ? shakeSound.length : 1f;
            }

            for (int i = 0; i < diceImages.Length; i++)
            {
                int randomFace = Random.Range(0, 6);
                diceImages[i].sprite = diceFaces[randomFace];
            }

            rollTime -= 0.1f;
            soundTimer -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        totalDiceValue = 0;
        List<Vector2> validPositionsThisRoll = new List<Vector2>();
        int maxSpawnAttempts = 100;

        for (int i = 0; i < diceImages.Length; i++)
        {
            int finalFace = Random.Range(0, 6);
            diceImages[i].sprite = diceFaces[finalFace];
            totalDiceValue += (finalFace + 1);

            bool validPosFound = false;
            Vector2 candidatePos = originalDicePos[i];

            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                float randX = Random.Range(-spawnRadius, spawnRadius);
                float randY = Random.Range(-spawnRadius, spawnRadius);
                candidatePos = originalDicePos[i] + new Vector2(randX, randY);

                bool tooClose = false;
                foreach (Vector2 placedPos in validPositionsThisRoll)
                {
                    if (Vector2.Distance(candidatePos, placedPos) < diceSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    validPosFound = true;
                    break;
                }
            }

            diceImages[i].rectTransform.anchoredPosition = candidatePos;
            validPositionsThisRoll.Add(candidatePos);
            diceImages[i].rectTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }

        statusText.text = "<size=120%>KÉO MẠNH BÁT RA ĐỂ XEM!</size>";
        statusText.color = Color.cyan;

        if (bowlObject != null)
        {
            bowlObject.isDraggable = true;
            StartCoroutine(AutoRevealCountdown());
        }
    }

    IEnumerator AutoRevealCountdown()
    {
        float timer = 5f;
        while (timer > 0)
        {
            if (bowlObject != null && bowlObject.hasRevealed) yield break;

            timer -= Time.deltaTime;
            timerText.text = Mathf.Ceil(timer).ToString() + "s";
            yield return null;
        }

        if (bowlObject != null && !bowlObject.hasRevealed)
        {
            bowlObject.hasRevealed = true;
            bowlObject.isDraggable = false;
            bowlObject.gameObject.SetActive(false);
            CheckResult();
        }
    }

    public void CheckResult()
    {
        int winningChoice = (totalDiceValue >= 11) ? 1 : 2;
        string resultName = (winningChoice == 1) ? "TÀI" : "XỈU";

        Image lightToFlash = (winningChoice == 1) ? imgLightTai : imgLightXiu;
        if (lightToFlash != null)
        {
            flashingCoroutine = StartCoroutine(FlashWinningArea(lightToFlash));
        }

        if (playerChoice == 0)
        {
            statusText.text = $"KẾT QUẢ: {totalDiceValue} - {resultName}!\n<color=#FFFFFF>BẠN CHỈ XEM, KHÔNG THẮNG THUA!</color>";
        }
        else if (playerChoice == winningChoice)
        {
            int winAmount = betAmount * 2;
            GameManager.instance.money += winAmount;
            statusText.text = $"KẾT QUẢ: {totalDiceValue} - {resultName}!\n<color=#00FF00>CHÚC MỪNG THẮNG LỚN {betAmount:N0} VNĐ</color>";
            PlaySound(winSound);
        }
        else
        {
            statusText.text = $"KẾT QUẢ: {totalDiceValue} - {resultName}!\n<color=#FF0000>THUA SẠCH TIỀN!</color>";
            PlaySound(loseSound);
        }

        UpdateMoneyUI();
        StartCoroutine(WaitAndRestart());
    }

    IEnumerator FlashWinningArea(Image targetArea)
    {
        for (int i = 0; i < 3; i++)
        {
            targetArea.color = new Color(0.3f, 1f, 0.3f, 0.5f);
            yield return new WaitForSeconds(0.3f);
            targetArea.color = new Color(0f, 0f, 0f, 0.2f);
            yield return new WaitForSeconds(0.3f);
        }
        targetArea.color = new Color(0.3f, 1f, 0.3f, 0.5f);
    }

    void ResetAreaLights()
    {
        if (imgLightTai != null) imgLightTai.color = new Color(0f, 0f, 0f, 0.2f);
        if (imgLightXiu != null) imgLightXiu.color = new Color(0f, 0f, 0f, 0.2f);
    }

    IEnumerator WaitAndRestart()
    {
        yield return new WaitForSeconds(4f);
        for (int i = 0; i < diceImages.Length; i++)
        {
            if (diceImages[i] != null)
            {
                diceImages[i].rectTransform.anchoredPosition = originalDicePos[i];
                diceImages[i].rectTransform.localRotation = Quaternion.identity;
            }
        }
        StartNewRound();
    }
}