using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    [Header("--- NHÀ CÁI ---")]
    [Tooltip("Kéo con NPC Nhà Cái vào đây để gọi nó xóc đĩa")]
    public Animator dealerAnimator;

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

    [Header("--- CỐT TRUYỆN: TÍN DỤNG ĐEN & ENDING ---")]
    public GameObject mainGameUI;
    public GameObject loanSharkPanel;
    public Button btnAcceptLoan;
    public Button btnDeclineLoan;
    public GameObject badEndingPanel;
    public Button btnRestartGame;

    [Header("Cài đặt Số ván kích hoạt Sự kiện")]
    public int warnAtGambleCount = 8;
    public int trapAtGambleCount = 9;
    public int deathAtGambleCount = 14;

    private int gambleCount = 0;
    private bool isIndebted = false;

    private List<GameObject> spawnedChips = new List<GameObject>();
    private Vector2[] originalDicePos;
    private float currentTimer;
    private bool isBettingPhase = false;
    private int playerChoice = 0;
    private int totalDiceValue = 0;

    private List<Animator> casinoCrowd = new List<Animator>();

    void Start()
    {
        if (bowlObject != null) bowlObject.manager = this;

        gambleCount = PlayerPrefs.GetInt("Casino_GambleCount", 0);
        isIndebted = PlayerPrefs.GetInt("Casino_IsIndebted", 0) == 1;

        if (badEndingPanel != null) badEndingPanel.SetActive(false);
        if (loanSharkPanel != null) loanSharkPanel.SetActive(false);

        if (btnRestartGame != null) btnRestartGame.onClick.AddListener(RestartFromChoice);
        if (btnAcceptLoan != null) btnAcceptLoan.onClick.AddListener(AcceptLoanEvent);
        if (btnDeclineLoan != null) btnDeclineLoan.onClick.AddListener(DeclineLoanEvent);

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

        Animator[] allAnimators = FindObjectsOfType<Animator>();
        foreach (Animator anim in allAnimators)
        {
            if (anim.CompareTag("NPC_Gambler")) casinoCrowd.Add(anim);
        }

        // ==========================================
        // ĐÃ FIX: ÉP GAME CẬP NHẬT UI VÀ BẮT ĐẦU VÁN MỚI NGAY KHI LOAD XONG SCENE
        // ==========================================
        UpdateMoneyUI();
    }

    public void OpenCasino()
    {
        if (GameManager.instance != null && GameManager.instance.isCasinoLocked)
        {
            Debug.Log("<color=red>BẠN ĐÃ BỊ CẤM CỬA KHỎI SÒNG BẠC VĨNH VIỄN!</color>");
            return;
        }

        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        StartNewRound();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) { TuaNhanh(warnAtGambleCount, "Dụ dỗ (Sẽ Thắng)"); }
        if (Input.GetKeyDown(KeyCode.F2)) { TuaNhanh(trapAtGambleCount, "Sập hầm Lần 10 (Sẽ Thua)"); }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            isIndebted = true; PlayerPrefs.SetInt("Casino_IsIndebted", 1);
            if (btnCloseCasino != null) btnCloseCasino.gameObject.SetActive(false);
            TuaNhanh(deathAtGambleCount, "Ván 15 - Bán Nội Tạng");
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            gambleCount = 0; isIndebted = false; PlayerPrefs.DeleteAll();
            if (GameManager.instance != null) GameManager.instance.isCasinoLocked = false;
            if (btnCloseCasino != null) btnCloseCasino.gameObject.SetActive(true);
            Debug.Log("<color=red>DEV CHEAT F3: Xóa mọi dữ liệu, mở cửa lại sòng bạc!</color>");
            if (isBettingPhase) StartNewRound();
        }

        if (isBettingPhase)
        {
            currentTimer -= Time.deltaTime;
            timerText.text = Mathf.Ceil(currentTimer).ToString() + "s";
            if (currentTimer <= 0) StartCoroutine(RollDiceRoutine());
        }
    }

    void TuaNhanh(int targetCount, string msg)
    {
        gambleCount = targetCount;
        PlayerPrefs.SetInt("Casino_GambleCount", gambleCount);
        GameManager.instance.money = 5000000;
        UpdateMoneyUI();
        Debug.Log($"<color=yellow>DEV CHEAT: Tua đến ván {targetCount + 1}! Chế độ: {msg}</color>");
        if (isBettingPhase) StartNewRound();
    }

    void PlaySound(AudioClip clip) { if (audioSource != null && clip != null) audioSource.PlayOneShot(clip); }

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
            if (chipPrefab != null && pendingArea != null && moneySprites.Length > spriteIndex) SpawnSingleChip(spriteIndex);
        }
    }

    public void BetAllIn()
    {
        if (!isBettingPhase || playerChoice != 0) return;
        int allInAmount = Mathf.FloorToInt(GameManager.instance.money);
        if (allInAmount <= 0) return;
        GameManager.instance.money = 0;
        betAmount += allInAmount;
        UpdateMoneyUI();
        PlaySound(coinSound);

        int[] chipValues = { 500000, 200000, 100000, 50000 };
        int[] chipSpriteIndices = { 3, 2, 1, 0 };
        int tempAmount = allInAmount;
        int visualChipCount = 0;

        for (int i = 0; i < chipValues.Length; i++)
        {
            int numChips = tempAmount / chipValues[i];
            if (numChips > 0)
            {
                for (int j = 0; j < numChips; j++) { if (visualChipCount < 40) { SpawnSingleChip(chipSpriteIndices[i]); visualChipCount++; } }
                tempAmount %= chipValues[i];
            }
        }
        statusText.text = "TẤT TAY!!! KHÔ MÁU VÁN NÀY!";
        statusText.color = Color.yellow;
    }

    private void SpawnSingleChip(int spriteIndex)
    {
        GameObject newChip = Instantiate(chipPrefab, pendingArea);
        Image chipImage = newChip.GetComponent<Image>();
        if (chipImage != null) chipImage.sprite = moneySprites[spriteIndex];
        newChip.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(-60f, 60f), Random.Range(-40f, 40f));
        newChip.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, Random.Range(-30f, 30f));
        spawnedChips.Add(newChip);
    }

    public void ClearBetUI() { if (!isBettingPhase) return; ClearBetCore(); }

    private void ClearBetCore()
    {
        if (playerChoice != 0 || betAmount <= 0) return;
        GameManager.instance.money += betAmount;
        betAmount = 0;
        UpdateMoneyUI();
        PlaySound(coinSound);
        foreach (GameObject chip in spawnedChips) Destroy(chip);
        spawnedChips.Clear();
    }

    public void CloseCasino()
    {
        if (isIndebted) return;
        StopAllCoroutines();
        isBettingPhase = false;
        ClearBetCore();
        if (bgmSource != null) bgmSource.Stop();

        if (interactPoint != null) interactPoint.ExitMinigame();
        else { gameObject.SetActive(false); Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }

    public void StartNewRound()
    {
        if (mainGameUI != null) mainGameUI.SetActive(true);
        for (int i = 0; i < diceImages.Length; i++) { if (diceImages[i] != null) diceImages[i].enabled = true; }

        if (isIndebted && btnCloseCasino != null) btnCloseCasino.gameObject.SetActive(false);

        if (isIndebted && GameManager.instance.money <= 0 && gambleCount < deathAtGambleCount)
        {
            TriggerBadEnding();
            return;
        }

        playerChoice = 0;
        currentTimer = bettingDuration;
        isBettingPhase = true;
        betAmount = 0;
        foreach (GameObject chip in spawnedChips) Destroy(chip);
        spawnedChips.Clear();
        UpdateMoneyUI();
        if (flashingCoroutine != null) StopCoroutine(flashingCoroutine);
        ResetAreaLights();

        if (bowlObject != null) { bowlObject.gameObject.SetActive(true); bowlObject.ResetPosition(); }
        SetButtonsState(true);

        if (gambleCount == warnAtGambleCount)
        {
            statusText.text = "<color=yellow>NHÀ CÁI: 'Bạn đang đỏ đấy! Ván này tao cho mày ăn trọn, Tất tay thử xem?'</color>";
        }
        else if (gambleCount == trapAtGambleCount)
        {
            statusText.text = "<color=red>NHÀ CÁI: 'Đã ngồi xuống đây thì ván này BẮT BUỘC TẤT TAY!'</color>";
            KhoaNutNho();
        }
        else if (gambleCount == deathAtGambleCount)
        {
            statusText.text = "<color=red>ĐẠI CA: 'ĐẾN HẠN RỒI! VÁN NÀY MÀY PHẢI CƯỢC BẰNG MẠNG!'</color>";
            KhoaNutNho();
        }
        else if (isIndebted)
        {
            statusText.text = $"<color=red>CHƠI TIẾP ĐI! VÁN THỨ {gambleCount + 1}/15</color>";
        }
        else
        {
            if (GameManager.instance != null && GameManager.instance.currentDay == 1)
            {
                statusText.text = $"VÁN {gambleCount + 1}/5 CỦA ĐÊM NAY. ĐẶT ĐI!";
            }
            else
            {
                statusText.text = "CHỌN TIỀN VÀ CHỐT TÀI/XỈU!";
            }
            statusText.color = Color.white;
        }
    }

    void KhoaNutNho()
    {
        btn50k.interactable = false; btn100k.interactable = false;
        btn200k.interactable = false; btn500k.interactable = false;
        btnClearBet.interactable = false;
        if (btnCloseCasino != null) btnCloseCasino.interactable = false;
    }

    public void PlaceBet(int choice)
    {
        if (!isBettingPhase) return;

        if ((gambleCount == trapAtGambleCount || gambleCount == deathAtGambleCount) && GameManager.instance.money > 0)
        {
            statusText.text = "<color=red>PHẢI BẤM 'TẤT TAY' MỚI ĐƯỢC CHỐT KẾT QUẢ!</color>";
            return;
        }

        if (betAmount <= 0) return;
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
                chip.GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(-70f, 70f), Random.Range(-40f, 40f));
            }
        }
    }

    void SetButtonsState(bool state)
    {
        btnTai.interactable = state; btnXiu.interactable = state;
        btn50k.interactable = state; btn100k.interactable = state;
        btn200k.interactable = state; btn500k.interactable = state;
        btnAllIn.interactable = state; btnClearBet.interactable = state;

        if (btnCloseCasino != null)
        {
            if (isIndebted || gambleCount == trapAtGambleCount || gambleCount == deathAtGambleCount)
                btnCloseCasino.interactable = false;
            else
                btnCloseCasino.interactable = (playerChoice == 0) ? true : state;
        }
    }

    IEnumerator RollDiceRoutine()
    {
        isBettingPhase = false;
        timerText.text = "0s";
        SetButtonsState(false);

        if (playerChoice == 0) yield return new WaitForSeconds(1.5f);

        statusText.text = "ĐANG XÓC ĐĨA...";
        statusText.color = Color.red;

        if (dealerAnimator != null)
        {
            dealerAnimator.SetTrigger("onShake");
        }

        float rollTime = rollDuration;
        float soundTimer = 0f;

        // Hiệu ứng xúc xắc nhảy loạn xạ khi đang xóc
        while (rollTime > 0)
        {
            if (soundTimer <= 0f) { PlaySound(shakeSound); soundTimer = shakeSound != null ? shakeSound.length : 1f; }
            for (int i = 0; i < diceImages.Length; i++) diceImages[i].sprite = diceFaces[Random.Range(0, 6)];
            rollTime -= 0.1f; soundTimer -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // ==========================================
        // ĐÃ FIX: TÍNH TOÁN ĐIỂM VÀ THAO TÚNG HÌNH ẢNH
        // ==========================================
        totalDiceValue = 0;
        int[] finalFaces = new int[3];

        // 1. Đổ xúc xắc ngẫu nhiên (Người lương thiện)
        for (int i = 0; i < 3; i++)
        {
            finalFaces[i] = Random.Range(1, 7);
            totalDiceValue += finalFaces[i];
        }

        // 2. Kích hoạt bẫy nhà cái (Kiểm tra xem có bịp không)
        bool isManipulated = false;
        int forcedTotal = totalDiceValue;

        if (playerChoice != 0)
        {
            if (GameManager.instance != null && GameManager.instance.currentDay == 1 && gambleCount < 5)
            {
                isManipulated = true;
                forcedTotal = (playerChoice == 1) ? Random.Range(11, 18) : Random.Range(3, 11);
            }
            else if (gambleCount == warnAtGambleCount)
            {
                isManipulated = true;
                forcedTotal = (playerChoice == 1) ? Random.Range(11, 18) : Random.Range(3, 11);
            }
            else if (gambleCount == trapAtGambleCount || isIndebted)
            {
                isManipulated = true;
                forcedTotal = (playerChoice == 1) ? Random.Range(3, 11) : Random.Range(11, 18);
            }
        }

        // 3. Nếu bịp, phải "phù phép" lại 3 mặt xúc xắc cho khớp với điểm ảo
        if (isManipulated)
        {
            totalDiceValue = forcedTotal;

            // Thuật toán chia kẹo: Chia tổng điểm cho 3 viên, mỗi viên từ 1 đến 6
            do
            {
                finalFaces[0] = Random.Range(1, 7);
                finalFaces[1] = Random.Range(1, 7);
                finalFaces[2] = totalDiceValue - finalFaces[0] - finalFaces[1];
            } while (finalFaces[2] < 1 || finalFaces[2] > 6);
        }

        // 4. In hình ảnh ĐÃ BỊP LÊN MÀN HÌNH
        for (int i = 0; i < diceImages.Length; i++)
        {
            diceImages[i].sprite = diceFaces[finalFaces[i] - 1]; // -1 vì index mảng từ 0 đến 5
            diceImages[i].rectTransform.anchoredPosition = originalDicePos[i] + new Vector2(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnRadius, spawnRadius));
        }
        // ==============================================================

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

        if (lightToFlash != null) flashingCoroutine = StartCoroutine(FlashWinningArea(lightToFlash));

        if (playerChoice == 0)
        {
            statusText.text = $"KẾT QUẢ: {totalDiceValue} - {resultName}!\n<color=#FFFFFF>BẠN CHỈ XEM!</color>";
            UpdateCrowdReaction(Random.Range(0, 2) == 0);
        }
        else if (playerChoice == winningChoice)
        {
            GameManager.instance.money += betAmount * 2;
            // ==========================================
            // ĐÃ SỬA: LỜI CHÚC MỪNG MỚI ĐÂY NHÉ!
            // ==========================================
            statusText.text = $"KẾT QUẢ: {totalDiceValue} - {resultName}!\n<color=#00FF00>CHÚC MỪNG THẮNG LỚN: {betAmount:N0} VNĐ</color>";
            PlaySound(winSound);
            UpdateCrowdReaction(true);
            SaveGambleCount();
        }
        else
        {
            statusText.text = $"KẾT QUẢ: {totalDiceValue} - {resultName}!\n<color=#FF0000>THUA SẠCH TIỀN!</color>";
            PlaySound(loseSound);
            UpdateCrowdReaction(false);

            if (gambleCount == trapAtGambleCount)
            {
                gambleCount++;
                StartCoroutine(ShowLoanSharkPanel());
                return;
            }
            else if (gambleCount == deathAtGambleCount)
            {
                gambleCount++;
                TriggerBadEnding();
                return;
            }
            SaveGambleCount();
        }

        UpdateMoneyUI();

        if (GameManager.instance != null && GameManager.instance.currentDay == 1 && gambleCount >= 5)
        {
            StartCoroutine(ForceSleepRoutine());
        }
        else
        {
            StartCoroutine(WaitAndRestart());
        }
    }

    IEnumerator ForceSleepRoutine()
    {
        SetButtonsState(false);
        if (btnCloseCasino != null) btnCloseCasino.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        statusText.text = "<color=yellow>NHÀ CÁI: 'Sới nghỉ! Nay mày đỏ đấy ma mới. Mai mang tiền xuống đây chơi tiếp!'\n<i>Đang về phòng ngủ...</i></color>";

        yield return new WaitForSeconds(4f);

        CloseCasino();

        if (GameManager.instance != null)
        {
            GameManager.instance.AdvanceToNextDay();
        }
    }

    void SaveGambleCount()
    {
        gambleCount++;
        PlayerPrefs.SetInt("Casino_GambleCount", gambleCount);
        PlayerPrefs.Save();
    }

    IEnumerator ShowLoanSharkPanel()
    {
        yield return new WaitForSeconds(3f);
        foreach (GameObject chip in spawnedChips) Destroy(chip);
        spawnedChips.Clear();
        if (bowlObject != null) bowlObject.gameObject.SetActive(false);
        for (int i = 0; i < diceImages.Length; i++) { if (diceImages[i] != null) diceImages[i].enabled = false; }

        if (mainGameUI != null) mainGameUI.SetActive(false);
        if (loanSharkPanel != null) loanSharkPanel.SetActive(true);
    }

    public void AcceptLoanEvent()
    {
        if (loanSharkPanel != null) loanSharkPanel.SetActive(false);

        isIndebted = true;
        PlayerPrefs.SetInt("Casino_IsIndebted", 1);
        PlayerPrefs.Save();

        GameManager.instance.money += 10000000;
        UpdateMoneyUI();

        StartNewRound();
    }

    public void DeclineLoanEvent()
    {
        StartCoroutine(BanPlayerAndNextDay());
    }

    IEnumerator BanPlayerAndNextDay()
    {
        if (loanSharkPanel != null) loanSharkPanel.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);

        SetButtonsState(false);
        if (btnCloseCasino != null) btnCloseCasino.gameObject.SetActive(false);

        statusText.text = "<size=150%><color=red>BẠN ĐÃ BỊ ĐÁNH ĐẬP VÀ CẤM CỬA VĨNH VIỄN KHỎI SÒNG BẠC!</color></size>";

        yield return new WaitForSeconds(3.5f);

        if (GameManager.instance != null)
        {
            GameManager.instance.isCasinoLocked = true;
            PlayerPrefs.SetInt("Casino_Locked", 1);
            PlayerPrefs.Save();
            GameManager.instance.AdvanceToNextDay();
        }

        CloseCasino();
    }

    private void TriggerBadEnding()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (mainGameUI != null) mainGameUI.SetActive(false);
        if (badEndingPanel != null) badEndingPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartFromChoice()
    {
        Time.timeScale = 1f;

        isIndebted = false;
        PlayerPrefs.SetInt("Casino_IsIndebted", 0);
        gambleCount = trapAtGambleCount + 1;
        PlayerPrefs.SetInt("Casino_GambleCount", gambleCount);
        GameManager.instance.money = 0;
        UpdateMoneyUI();

        if (badEndingPanel != null) badEndingPanel.SetActive(false);
        if (loanSharkPanel != null) loanSharkPanel.SetActive(true);

        if (bgmSource != null && bgmClip != null) bgmSource.Play();
    }

    IEnumerator FlashWinningArea(Image targetArea)
    {
        for (int i = 0; i < 3; i++)
        {
            targetArea.color = new Color(0.3f, 1f, 0.3f, 0.5f); yield return new WaitForSeconds(0.3f);
            targetArea.color = new Color(0f, 0f, 0f, 0.2f); yield return new WaitForSeconds(0.3f);
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

    public void UpdateCrowdReaction(bool playerWon)
    {
        foreach (Animator anim in casinoCrowd)
        {
            if (anim != null) { if (playerWon) anim.SetTrigger("onWin"); else anim.SetTrigger("onLose"); }
        }
    }
}