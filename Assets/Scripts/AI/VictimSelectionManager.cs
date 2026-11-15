using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý tìm kiếm con mồi theo cơ chế Ghép Trận kiểu Liên Minh (Queue -> Match Found -> Accept/Decline -> Lock-In).
/// Tự động dựng UI Hextech bên trong selectionPanel và giữ nguyên toàn bộ tham chiếu cũ.
/// </summary>
public class VictimSelectionManager : MonoBehaviour
{
    public enum MatchmakingState
    {
        Idle,
        Searching,
        MatchFound,
        LockInReveal
    }

    [Header("Danh sách 25 Nạn Nhân (Kéo thả vào đây)")]
    public List<VictimProfile> allVictims;

    [Header("UI Của 4 Thẻ Chọn (Kế thừa / Lấy Font)")]
    public GameObject selectionPanel;
    public Button[] btnSelectVictims;
    public TextMeshProUGUI[] nameTexts;
    public TextMeshProUGUI[] jobTexts;
    public TextMeshProUGUI[] rewardTexts;
    public Image[] avatarImages;

    [Header("Tham chiếu")]
    public ScamMinigame minigameController;

    [Header("--- CẤU HÌNH GHÉP TRẬN (KIỂU LIÊN MINH) ---")]
    public bool useMatchmakingMode = true;
    [Tooltip("Thời gian quét tìm mục tiêu tối thiểu (giây)")]
    public float queueDurationMin = 2.0f;
    [Tooltip("Thời gian quét tìm mục tiêu tối đa (giây)")]
    public float queueDurationMax = 4.2f;
    [Tooltip("Thời gian đếm ngược Chấp nhận / Từ chối (giây)")]
    public float matchAcceptDuration = 10f;
    [Tooltip("Số lần từ chối/bỏ lỡ trước khi Quản lý phát hiện & chích điện")]
    public int consecutiveDeclinesBeforePenalty = 2;
    [Tooltip("Sát thương roi điện khi từ chối quá giới hạn")]
    public int declineShockDamage = 10;
    [Tooltip("Thời gian hiển thị hồ sơ Lock-In trước khi vào chat")]
    public float lockInDuration = 1.6f;

    [Header("--- ÂM THANH GHÉP TRẬN (Tùy chọn, tự tạo âm thanh mẫu nếu trống) ---")]
    public AudioSource sfxSource;
    public AudioClip queueStartSound;
    public AudioClip matchFoundSound;
    public AudioClip countdownTickSound;
    public AudioClip acceptLockInSound;
    public AudioClip declineSound;

    // Pools
    private readonly List<VictimProfile> normalPool = new List<VictimProfile>();
    private readonly List<VictimProfile> trollPool = new List<VictimProfile>();
    private readonly VictimProfile[] currentChoices = new VictimProfile[4];

    // Runtime state
    private MatchmakingState state = MatchmakingState.Idle;
    private int activeDay = 1;
    private float queueElapsed = 0f;
    private float targetQueueDuration = 3f;
    private float matchFoundTimer = 0f;
    private int lastWholeSecond = -1;
    private float lockInElapsed = 0f;
    private int consecutiveDeclines = 0;
    private bool wasPanelActive = false;
    private VictimProfile matchedVictim;
    private BossIntroManager cachedBossIntro;

    // UI References (Runtime built inside selectionPanel)
    private TMP_FontAsset gameFont;
    private GameObject matchmakingRoot;
    private GameObject lobbyView;
    private GameObject matchFoundModal;
    private GameObject lockInView;

    // Header / Status
    private TextMeshProUGUI txtHeaderDay;
    private TextMeshProUGUI txtHeaderStats;
    private TextMeshProUGUI txtWarningBanner;

    // Lobby / Queue View
    private RectTransform radarOuterRing;
    private RectTransform radarInnerRing;
    private RectTransform radarPulseRing;
    private Image radarCenterGlow;
    private TextMeshProUGUI txtQueueTitle;
    private TextMeshProUGUI txtQueueTimer;
    private TextMeshProUGUI txtQueueFlavor;
    private Button btnToggleQueue;
    private Image btnToggleQueueBg;
    private TextMeshProUGUI txtToggleQueueLabel;
    private float flavorCycleTimer = 0f;
    private int flavorIndex = 0;

    // Match Found Modal
    private RectTransform matchModalCard;
    private Image radialTimerFill;
    private Image radialTimerGlow;
    private TextMeshProUGUI txtMatchFoundHeader;
    private TextMeshProUGUI txtMatchCountdown;
    private TextMeshProUGUI txtTeaserRisk;
    private TextMeshProUGUI txtTeaserReward;
    private TextMeshProUGUI txtTeaserHint;
    private Image imgTeaserAvatar;
    private Button btnAcceptMatch;
    private Button btnDeclineMatch;

    // Lock-In Reveal View
    private Image imgLockInAvatar;
    private TextMeshProUGUI txtLockInName;
    private TextMeshProUGUI txtLockInJob;
    private TextMeshProUGUI txtLockInReward;
    private TextMeshProUGUI txtLockInBadge;
    private Image imgLockInProgressFill;
    private Button btnEnterChatNow;

    // Procedural Assets to clean up
    private readonly List<Texture2D> generatedTextures = new List<Texture2D>();
    private readonly List<Sprite> generatedSprites = new List<Sprite>();
    private readonly List<AudioClip> generatedClips = new List<AudioClip>();
    private Sprite roundedBoxSprite;
    private Sprite solidCircleSprite;
    private Sprite ringSprite;
    private Sprite segmentedRingSprite;

    private static readonly string[] SearchingPhrases =
    {
        "Đang dò lỗ hổng dữ liệu từ mạng xã hội...",
        "Đang đối chiếu hồ sơ nợ tín dụng & tâm lý dễ tổn thương...",
        "Đang ngụy trang IP qua trạm chuyển tiếp Tam Giác Vàng...",
        "Đang lọc số điện thoại đang trực tuyến...",
        "Phát hiện mục tiêu có dấu hiệu nhẹ dạ..."
    };

    void Awake()
    {
        RefillPoolsIfNeeded(forceReset: true);
        ResolveFont();
        EnsureAudioSetup();

        if (useMatchmakingMode && selectionPanel != null)
        {
            BuildMatchmakingUI();
        }
    }

    void Start()
    {
        if (useMatchmakingMode && selectionPanel != null)
        {
            UIButtonJuice.EnhanceAllButtonsIn(matchmakingRoot != null ? matchmakingRoot : gameObject);
        }
        else
        {
            UIButtonJuice.EnhanceAllButtonsIn(gameObject);
        }

        cachedBossIntro = FindFirstObjectByType<BossIntroManager>(FindObjectsInactive.Include);
        int startDay = GameManager.instance != null ? GameManager.instance.currentDay : 1;

        // Tránh mở đè lên bảng dằn mặt Sáng Ngày 1 của BossIntroManager
        if (cachedBossIntro != null && cachedBossIntro.introPanel != null && cachedBossIntro.introPanel.activeSelf)
        {
            if (selectionPanel != null) selectionPanel.SetActive(false);
            StartCoroutine(WaitForBossIntroThenOpen(startDay));
        }
        else
        {
            ShowSelectionUI(startDay);
        }
    }

    IEnumerator WaitForBossIntroThenOpen(int day)
    {
        while (cachedBossIntro != null && cachedBossIntro.introPanel != null && cachedBossIntro.introPanel.activeSelf)
        {
            yield return null;
        }
        // Chờ 1 frame sau khi BossIntroManager.CloseIntro() khóa chuột để giành lại quyền mở UI
        yield return null;
        int currentDay = GameManager.instance != null ? GameManager.instance.currentDay : day;
        ShowSelectionUI(currentDay);
    }

    void Update()
    {
        if (selectionPanel == null) return;

        bool isActiveNow = selectionPanel.activeInHierarchy;
        if (isActiveNow && !wasPanelActive)
        {
            int currentDay = GameManager.instance != null ? GameManager.instance.currentDay : activeDay;
            activeDay = currentDay;
            RefreshStatsHUD();
            if (useMatchmakingMode && state != MatchmakingState.Idle)
            {
                EnterIdleState();
            }
        }
        wasPanelActive = isActiveNow;

        if (!isActiveNow) return;

        // Giữ chuột mở & khóa camera khi panel đang hiển thị (trừ lúc BossIntro đang mở)
        bool bossBlocking = cachedBossIntro != null && cachedBossIntro.introPanel != null && cachedBossIntro.introPanel.activeSelf;
        if (!bossBlocking)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!useMatchmakingMode || matchmakingRoot == null) return;

        float dt = Time.unscaledDeltaTime;

        switch (state)
        {
            case MatchmakingState.Idle:
                UpdateIdleVisuals(dt);
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    StartQueue();
                }
                break;

            case MatchmakingState.Searching:
                UpdateSearchingState(dt);
                break;

            case MatchmakingState.MatchFound:
                UpdateMatchFoundState(dt);
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    AcceptMatch();
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    DeclineMatch(isTimeout: false);
                }
                break;

            case MatchmakingState.LockInReveal:
                UpdateLockInState(dt);
                break;
        }
    }

    void OnDestroy()
    {
        foreach (var clip in generatedClips)
        {
            if (clip != null) Destroy(clip);
        }
        generatedClips.Clear();

        foreach (var sp in generatedSprites)
        {
            if (sp != null) Destroy(sp);
        }
        generatedSprites.Clear();

        foreach (var tex in generatedTextures)
        {
            if (tex != null) Destroy(tex);
        }
        generatedTextures.Clear();
    }

    // =========================================================================
    // PUBLIC API (GIỮ NGUYÊN CHỮ KÝ ĐỂ GAMEMANAGER & CÁC SCRIPT KHÁC GỌI)
    // =========================================================================

    public void ShowSelectionUI(int currentDay)
    {
        activeDay = currentDay;
        if (selectionPanel != null) selectionPanel.SetActive(true);
        wasPanelActive = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LockPlayerCamera(true);

        if (useMatchmakingMode)
        {
            if (matchmakingRoot == null) BuildMatchmakingUI();
            HideLegacyChildren();
            if (matchmakingRoot != null) matchmakingRoot.SetActive(true);
            RefreshStatsHUD();
            EnterIdleState();
        }
        else
        {
            ShowLegacyCards(currentDay);
        }
    }

    // =========================================================================
    // MATCHMAKING STATE MACHINE
    // =========================================================================

    private void EnterIdleState()
    {
        state = MatchmakingState.Idle;
        matchedVictim = null;
        queueElapsed = 0f;

        if (lobbyView != null) lobbyView.SetActive(true);
        if (matchFoundModal != null) matchFoundModal.SetActive(false);
        if (lockInView != null) lockInView.SetActive(false);

        if (txtQueueTitle != null)
            txtQueueTitle.text = "SẴN SÀNG QUÉT MỤC TIÊU";
        if (txtQueueTimer != null)
            txtQueueTimer.text = "CHỜ LỆNH";
        if (txtQueueFlavor != null)
            txtQueueFlavor.text = "Bấm <b>[TÌM TRẬN]</b> để hệ thống rà soát con mồi trên mạng. Từ chối hoặc bỏ lỡ nhiều lần sẽ bị Quản lý chích điện!";

        if (btnToggleQueueBg != null)
            btnToggleQueueBg.color = new Color(0.05f, 0.45f, 0.56f, 1f);
        if (txtToggleQueueLabel != null)
            txtToggleQueueLabel.text = "TÌM CON MỒI";

        if (radarPulseRing != null) radarPulseRing.localScale = Vector3.one;
        RefreshStatsHUD();
        UpdateWarningBanner(null);
    }

    private void ToggleQueueClicked()
    {
        if (state == MatchmakingState.Idle)
        {
            StartQueue();
        }
        else if (state == MatchmakingState.Searching)
        {
            CancelQueue();
        }
    }

    private void StartQueue()
    {
        if (allVictims == null || allVictims.Count == 0)
        {
            if (txtWarningBanner != null)
                txtWarningBanner.text = "<color=#EF476F>LỖI DỮ LIỆU: Chưa có VictimProfile nào trong danh sách!</color>";
            return;
        }

        state = MatchmakingState.Searching;
        queueElapsed = 0f;
        flavorCycleTimer = 0f;
        flavorIndex = Random.Range(0, SearchingPhrases.Length);
        targetQueueDuration = Random.Range(Mathf.Max(0.8f, queueDurationMin), Mathf.Max(queueDurationMin + 0.5f, queueDurationMax));

        if (txtQueueTitle != null)
            txtQueueTitle.text = "<color=#00E5FF>ĐANG TÌM MỤC TIÊU...</color>";
        if (txtQueueTimer != null)
            txtQueueTimer.text = "00:00";
        if (txtQueueFlavor != null)
            txtQueueFlavor.text = SearchingPhrases[flavorIndex];

        if (btnToggleQueueBg != null)
            btnToggleQueueBg.color = new Color(0.55f, 0.14f, 0.2f, 1f);
        if (txtToggleQueueLabel != null)
            txtToggleQueueLabel.text = "HỦY TÌM";

        PlayClip(queueStartSound, 0.7f);
    }

    private void CancelQueue()
    {
        EnterIdleState();
        UpdateWarningBanner("<color=#94A3B8>Đã hủy quét mạng. Bấm TÌM CON MỒI để tiếp tục chỉ tiêu KPI.</color>");
    }

    private void UpdateIdleVisuals(float dt)
    {
        if (radarOuterRing != null) radarOuterRing.Rotate(0f, 0f, -12f * dt);
        if (radarInnerRing != null) radarInnerRing.Rotate(0f, 0f, 18f * dt);
        if (radarCenterGlow != null)
        {
            float a = 0.22f + Mathf.PingPong(Time.unscaledTime * 0.4f, 0.15f);
            radarCenterGlow.color = new Color(0.06f, 0.72f, 0.85f, a);
        }
    }

    private void UpdateSearchingState(float dt)
    {
        queueElapsed += dt;

        if (radarOuterRing != null) radarOuterRing.Rotate(0f, 0f, -75f * dt);
        if (radarInnerRing != null) radarInnerRing.Rotate(0f, 0f, 120f * dt);

        if (radarPulseRing != null)
        {
            float pulsePhase = Mathf.Repeat(queueElapsed * 1.15f, 1f);
            float scale = Mathf.Lerp(0.8f, 1.45f, pulsePhase);
            radarPulseRing.localScale = new Vector3(scale, scale, 1f);
            Image pulseImg = radarPulseRing.GetComponent<Image>();
            if (pulseImg != null)
            {
                pulseImg.color = new Color(0.0f, 0.9f, 1.0f, (1f - pulsePhase) * 0.55f);
            }
        }

        int mins = Mathf.FloorToInt(queueElapsed / 60f);
        int secs = Mathf.FloorToInt(queueElapsed % 60f);
        if (txtQueueTimer != null)
        {
            txtQueueTimer.text = $"{mins:00}:{secs:00}";
        }

        flavorCycleTimer += dt;
        if (flavorCycleTimer >= 1.4f)
        {
            flavorCycleTimer = 0f;
            flavorIndex = (flavorIndex + 1) % SearchingPhrases.Length;
            if (txtQueueFlavor != null) txtQueueFlavor.text = SearchingPhrases[flavorIndex];
        }

        if (queueElapsed >= targetQueueDuration)
        {
            TriggerMatchFound();
        }
    }

    private void TriggerMatchFound()
    {
        matchedVictim = PickMatchmakingVictim(activeDay);
        if (matchedVictim == null)
        {
            EnterIdleState();
            return;
        }

        state = MatchmakingState.MatchFound;
        matchFoundTimer = matchAcceptDuration;
        lastWholeSecond = Mathf.CeilToInt(matchAcceptDuration);

        if (matchFoundModal != null) matchFoundModal.SetActive(true);
        if (matchModalCard != null) matchModalCard.localScale = new Vector3(0.86f, 0.86f, 1f);

        PopulateMatchFoundModal(matchedVictim);
        PlayClip(matchFoundSound, 0.9f);
    }

    private void PopulateMatchFoundModal(VictimProfile victim)
    {
        if (txtMatchFoundHeader != null)
            txtMatchFoundHeader.text = victim.isTroll ? "PHÁT HIỆN TÍN HIỆU LẠ!" : "ĐÃ TÌM THẤY MỤC TIÊU!";

        if (imgTeaserAvatar != null)
        {
            if (victim.avatar != null)
            {
                imgTeaserAvatar.sprite = victim.avatar;
                // Làm tối nhẹ như bóng silhouette chờ Accept để tạo kịch tính
                imgTeaserAvatar.color = new Color(0.75f, 0.88f, 0.96f, 1f);
            }
            else
            {
                imgTeaserAvatar.sprite = solidCircleSprite;
                imgTeaserAvatar.color = new Color(0.14f, 0.22f, 0.30f, 1f);
            }
        }

        if (txtTeaserRisk != null)
            txtTeaserRisk.text = GetDifficultyBadge(victim.difficultyLevel, victim.isTroll);

        string estMoney = victim.isTroll
            ? "<color=#C77DFF>Bất thường / Không xác định</color>"
            : $"<color=#FFD166><b>~{victim.potentialReward:N0} VNĐ</b></color>";
        if (txtTeaserReward != null)
            txtTeaserReward.text = $"Ước tính thu lợi: {estMoney}";

        int roundCount = victim.rounds != null ? victim.rounds.Length : 5;
        string diffFlavor = victim.isTroll
            ? "Cảnh báo: Hồ sơ không rõ danh tính, có thể phản tác dụng hoặc không tính KPI!"
            : $"Kịch bản {roundCount} hiệp • Tâm lý mục tiêu: {(victim.difficultyLevel == VictimProfile.Difficulty.De ? "Nhẹ dạ, dễ dẫn dắt" : victim.difficultyLevel == VictimProfile.Difficulty.TrungBinh ? "Cảnh giác trung bình" : "Khó tính, đòi hỏi gõ cực chuẩn")}";

        if (txtTeaserHint != null)
            txtTeaserHint.text = diffFlavor;

        UpdateMatchCountdownVisuals();
    }

    private void UpdateMatchFoundState(float dt)
    {
        matchFoundTimer -= dt;
        if (matchModalCard != null)
        {
            matchModalCard.localScale = Vector3.Lerp(matchModalCard.localScale, Vector3.one, dt * 14f);
        }

        UpdateMatchCountdownVisuals();

        int wholeSecond = Mathf.Max(0, Mathf.CeilToInt(matchFoundTimer));
        if (wholeSecond != lastWholeSecond)
        {
            lastWholeSecond = wholeSecond;
            if (wholeSecond > 0 && wholeSecond <= 4)
            {
                PlayClip(countdownTickSound, 0.55f);
            }
        }

        if (matchFoundTimer <= 0f)
        {
            DeclineMatch(isTimeout: true);
        }
    }

    private void UpdateMatchCountdownVisuals()
    {
        float ratio = matchAcceptDuration > 0f ? Mathf.Clamp01(matchFoundTimer / matchAcceptDuration) : 0f;
        if (radialTimerFill != null)
        {
            radialTimerFill.fillAmount = ratio;
            radialTimerFill.color = ratio > 0.35f
                ? new Color(0.04f, 0.85f, 0.86f, 1f)
                : Color.Lerp(new Color(0.94f, 0.25f, 0.38f, 1f), new Color(0.98f, 0.75f, 0.22f, 1f), ratio / 0.35f);
        }
        if (radialTimerGlow != null)
        {
            radialTimerGlow.color = ratio > 0.35f
                ? new Color(0.85f, 0.68f, 0.28f, 0.45f)
                : new Color(0.94f, 0.22f, 0.32f, 0.4f + Mathf.PingPong(Time.unscaledTime * 3f, 0.35f));
        }
        if (txtMatchCountdown != null)
        {
            txtMatchCountdown.text = $"{Mathf.Max(0, Mathf.CeilToInt(matchFoundTimer))}s";
        }
    }

    private void AcceptMatch()
    {
        if (matchedVictim == null || state != MatchmakingState.MatchFound) return;

        // Rút con mồi khỏi pool giống hệ thống cũ
        if (matchedVictim.isTroll) trollPool.Remove(matchedVictim);
        else normalPool.Remove(matchedVictim);

        consecutiveDeclines = 0;
        PlayClip(acceptLockInSound, 0.9f);

        EnterLockInReveal();
    }

    private void DeclineMatch(bool isTimeout)
    {
        if (state != MatchmakingState.MatchFound) return;

        consecutiveDeclines++;
        PlayClip(declineSound, 0.85f);

        string notice;
        if (consecutiveDeclines >= consecutiveDeclinesBeforePenalty)
        {
            consecutiveDeclines = 0;
            if (GameManager.instance != null)
            {
                GameManager.instance.TakeShockDamage(declineShockDamage);
                if (GameManager.instance.sfxSource != null && GameManager.instance.shockSound != null)
                {
                    GameManager.instance.sfxSource.PlayOneShot(GameManager.instance.shockSound);
                }
            }
            notice = isTimeout
                ? $"<color=#EF476F><b>QUẢN LÝ NỔI ĐIÊN!</b> Bỏ lỡ mục tiêu liên tiếp — Mày bị dí roi điện (-{declineShockDamage} HP)!</color>"
                : $"<color=#EF476F><b>QUẢN LÝ CHÍCH ĐIỆN (-{declineShockDamage} HP)!</b> Ở Tam Giác Vàng không có quyền chê con mồi!</color>";
        }
        else
        {
            int left = consecutiveDeclinesBeforePenalty - consecutiveDeclines;
            notice = isTimeout
                ? $"<color=#FFD166>Hết giờ chấp nhận trận! Quá cảnh báo ({left} lần nữa) Quản lý sẽ chích điện.</color>"
                : $"<color=#FFD166>Đã từ chối mục tiêu. Quản lý đang để mắt tới mày (Còn {left} lần cảnh cáo).</color>";
        }

        EnterIdleState();
        UpdateWarningBanner(notice);
    }

    private void EnterLockInReveal()
    {
        state = MatchmakingState.LockInReveal;
        lockInElapsed = 0f;

        if (matchFoundModal != null) matchFoundModal.SetActive(false);
        if (lobbyView != null) lobbyView.SetActive(false);
        if (lockInView != null) lockInView.SetActive(true);

        if (imgLockInAvatar != null)
        {
            if (matchedVictim.avatar != null)
            {
                imgLockInAvatar.sprite = matchedVictim.avatar;
                imgLockInAvatar.color = Color.white;
            }
            else
            {
                imgLockInAvatar.sprite = solidCircleSprite;
                imgLockInAvatar.color = new Color(0.18f, 0.28f, 0.36f, 1f);
            }
        }

        if (txtLockInName != null)
            txtLockInName.text = $"<b>{matchedVictim.victimName}</b>";
        if (txtLockInJob != null)
            txtLockInJob.text = matchedVictim.jobOrAge;
        if (txtLockInReward != null)
            txtLockInReward.text = $"Tiềm năng: <color=#FFD166><b>{matchedVictim.potentialReward:N0} VNĐ</b></color>   |   Ám ảnh lương tâm: <color=#EF476F>-{matchedVictim.karmaPenalty}</color>";
        if (txtLockInBadge != null)
            txtLockInBadge.text = GetDifficultyBadge(matchedVictim.difficultyLevel, matchedVictim.isTroll);
        if (imgLockInProgressFill != null)
            imgLockInProgressFill.fillAmount = 0f;

        RefreshStatsHUD();
    }

    private void UpdateLockInState(float dt)
    {
        lockInElapsed += dt;
        float progress = lockInDuration > 0f ? Mathf.Clamp01(lockInElapsed / lockInDuration) : 1f;
        if (imgLockInProgressFill != null)
        {
            imgLockInProgressFill.fillAmount = progress;
        }

        if (lockInElapsed >= lockInDuration || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            LaunchMinigameForMatchedVictim();
        }
    }

    private void LaunchMinigameForMatchedVictim()
    {
        if (matchedVictim == null)
        {
            EnterIdleState();
            return;
        }

        VictimProfile chosen = matchedVictim;
        matchedVictim = null;
        state = MatchmakingState.Idle;

        if (selectionPanel != null) selectionPanel.SetActive(false);
        wasPanelActive = false;

        if (minigameController != null)
        {
            minigameController.maxMoneyReward = chosen.potentialReward;
            minigameController.karmaPenalty = chosen.karmaPenalty;
            minigameController.StartMiniGame(chosen.rounds, chosen.victimName, chosen.avatar, chosen.isTroll);
        }
    }

    // =========================================================================
    // VICTIM PICKING & HUD REFRESH
    // =========================================================================

    private void RefillPoolsIfNeeded(bool forceReset = false)
    {
        if (allVictims == null) return;

        if (forceReset || normalPool.Count == 0)
        {
            normalPool.Clear();
            foreach (var v in allVictims)
            {
                if (v != null && !v.isTroll) normalPool.Add(v);
            }
        }

        if (forceReset || trollPool.Count == 0)
        {
            trollPool.Clear();
            foreach (var v in allVictims)
            {
                if (v != null && v.isTroll) trollPool.Add(v);
            }
        }
    }

    private VictimProfile PickMatchmakingVictim(int currentDay)
    {
        RefillPoolsIfNeeded(forceReset: false);

        // Từ ngày 2 trở đi có tỷ lệ gặp đối tượng troll/bất thường (~22%)
        bool rollTroll = currentDay >= 2 && trollPool.Count > 0 && Random.value < 0.22f;
        if (rollTroll)
        {
            return trollPool[Random.Range(0, trollPool.Count)];
        }

        if (normalPool.Count == 0) return null;

        // Ưu tiên độ khó phù hợp theo ngày
        List<VictimProfile> preferred = new List<VictimProfile>();
        foreach (var v in normalPool)
        {
            if (currentDay == 1 && v.difficultyLevel != VictimProfile.Difficulty.Kho)
                preferred.Add(v);
            else if (currentDay == 2 && v.difficultyLevel != VictimProfile.Difficulty.Kho)
                preferred.Add(v);
            else if (currentDay >= 3)
                preferred.Add(v);
        }

        List<VictimProfile> source = preferred.Count > 0 ? preferred : normalPool;
        return source[Random.Range(0, source.Count)];
    }

    private void RefreshStatsHUD()
    {
        int day = activeDay;
        int hp = 100;
        int maxHp = 100;
        int successes = 0;
        int kpi = 2;
        int attemptsUsed = 0;
        int maxAttempts = 5;
        float commission = 0.1f;
        string phaseLabel = "CA SÁNG";

        if (GameManager.instance != null)
        {
            day = GameManager.instance.currentDay;
            hp = GameManager.instance.hp;
            maxHp = GameManager.instance.maxHp;
            successes = GameManager.instance.successfulScamsToday;
            kpi = GameManager.instance.targetKPI;
            attemptsUsed = GameManager.instance.attemptedScamsToday;
            maxAttempts = GameManager.instance.maxAttemptsPerDay;
            commission = GameManager.instance.currentCommissionRate;
            phaseLabel = GameManager.instance.currentPhase == GamePhase.Afternoon ? "CA CHIỀU" : "CA SÁNG";
        }

        int remaining = Mathf.Max(0, maxAttempts - attemptsUsed);

        if (txtHeaderDay != null)
        {
            txtHeaderDay.text = $"<b>TAM GIÁC VÀNG  |  NGÀY {day}/5 — {phaseLabel}</b>";
        }
        if (txtHeaderStats != null)
        {
            string kpiColor = successes >= 2 ? "#06D6A0" : "#FFD166";
            string hpColor = hp <= 30 ? "#EF476F" : "#38BDF8";
            txtHeaderStats.text =
                $"Sinh lực: <color={hpColor}><b>{hp}/{maxHp}</b></color>   •   " +
                $"KPI: <color={kpiColor}><b>{successes}/{kpi}</b></color> (Tối thiểu 2)   •   " +
                $"Hoa hồng: <color=#F4D03F><b>{commission * 100f:0}%</b></color>   •   " +
                $"Lượt còn: <b>{remaining}</b>";
        }
    }

    private void UpdateWarningBanner(string customNotice)
    {
        if (txtWarningBanner == null) return;

        if (!string.IsNullOrEmpty(customNotice))
        {
            txtWarningBanner.text = customNotice;
            return;
        }

        if (consecutiveDeclines > 0)
        {
            int left = Mathf.Max(0, consecutiveDeclinesBeforePenalty - consecutiveDeclines);
            txtWarningBanner.text = $"<color=#FFD166>Cảnh báo Quản lý: Đã từ chối {consecutiveDeclines}/{consecutiveDeclinesBeforePenalty} lần (Còn {left} lần trước khi bị chích điện).</color>";
        }
        else
        {
            txtWarningBanner.text = "<color=#94A3B8>Mạng nội bộ Khu Tự Trị ổn định • Đạt KPI tối thiểu 2 con mồi/ngày để tránh roi điện.</color>";
        }
    }

    private void LockPlayerCamera(bool locked)
    {
        PlayerMovement playerMove = FindFirstObjectByType<PlayerMovement>();
        if (playerMove != null)
        {
            playerMove.canLook = !locked;
            playerMove.canWalk = !locked;
        }

        MouseLook mouseLook = FindFirstObjectByType<MouseLook>();
        if (mouseLook != null)
        {
            mouseLook.enabled = !locked;
        }
    }

    // =========================================================================
    // LEGACY 4-CARD FALLBACK (CHỈ DÙNG NẾU TẮT useMatchmakingMode)
    // =========================================================================

    private void ShowLegacyCards(int currentDay)
    {
        if (matchmakingRoot != null) matchmakingRoot.SetActive(false);

        if (selectionPanel != null)
        {
            foreach (Transform child in selectionPanel.transform)
            {
                if (child.gameObject != matchmakingRoot) child.gameObject.SetActive(true);
            }
        }

        RefillPoolsIfNeeded(forceReset: false);
        List<VictimProfile> tempChoices = new List<VictimProfile>();
        if (currentDay == 1 || trollPool.Count == 0)
        {
            tempChoices = GetRandomVictims(normalPool, 4);
        }
        else
        {
            tempChoices.AddRange(GetRandomVictims(trollPool, 1));
            tempChoices.AddRange(GetRandomVictims(normalPool, 3));
        }

        ShuffleList(tempChoices);

        for (int i = 0; i < 4; i++)
        {
            if (i < tempChoices.Count)
            {
                currentChoices[i] = tempChoices[i];
                if (nameTexts != null && i < nameTexts.Length && nameTexts[i] != null)
                    nameTexts[i].text = $"<b>{tempChoices[i].victimName}</b>";
                if (jobTexts != null && i < jobTexts.Length && jobTexts[i] != null)
                    jobTexts[i].text = $"<color=#CCCCCC>{tempChoices[i].jobOrAge}</color>";

                string diffBadge = GetDifficultyBadge(tempChoices[i].difficultyLevel, tempChoices[i].isTroll);
                string rewardStr = tempChoices[i].potentialReward.ToString("N0") + " VNĐ";
                if (rewardTexts != null && i < rewardTexts.Length && rewardTexts[i] != null)
                    rewardTexts[i].text = $"Tiềm năng: <color=#FFD166><b>{rewardStr}</b></color>\n{diffBadge}";

                if (avatarImages != null && avatarImages.Length > i && avatarImages[i] != null && tempChoices[i].avatar != null)
                    avatarImages[i].sprite = tempChoices[i].avatar;

                int index = i;
                if (btnSelectVictims != null && btnSelectVictims.Length > i && btnSelectVictims[i] != null)
                {
                    btnSelectVictims[i].onClick.RemoveAllListeners();
                    btnSelectVictims[i].onClick.AddListener(() => OnLegacyVictimChosen(index));
                }
            }
        }
    }

    private void OnLegacyVictimChosen(int index)
    {
        VictimProfile chosenVictim = currentChoices[index];
        if (chosenVictim == null) return;

        if (chosenVictim.isTroll) trollPool.Remove(chosenVictim);
        else normalPool.Remove(chosenVictim);

        if (selectionPanel != null) selectionPanel.SetActive(false);

        if (minigameController != null)
        {
            minigameController.maxMoneyReward = chosenVictim.potentialReward;
            minigameController.karmaPenalty = chosenVictim.karmaPenalty;
            minigameController.StartMiniGame(chosenVictim.rounds, chosenVictim.victimName, chosenVictim.avatar, chosenVictim.isTroll);
        }
    }

    private string GetDifficultyBadge(VictimProfile.Difficulty diff, bool isTroll)
    {
        if (isTroll) return "<color=#C77DFF><b>[ ⚠ NHÂN VẬT ĐẶC BIỆT ]</b></color>";

        switch (diff)
        {
            case VictimProfile.Difficulty.De:
                return "<color=#06D6A0><b>[ ĐỘ KHÓ: DỄ ]</b></color>";
            case VictimProfile.Difficulty.TrungBinh:
                return "<color=#FFD166><b>[ ĐỘ KHÓ: TRUNG BÌNH ]</b></color>";
            case VictimProfile.Difficulty.Kho:
                return "<color=#EF476F><b>[ ĐỘ KHÓ: CAO ]</b></color>";
            default:
                return "<color=#AAAAAA><b>[ CHƯA XÁC ĐỊNH ]</b></color>";
        }
    }

    private List<VictimProfile> GetRandomVictims(List<VictimProfile> pool, int amount)
    {
        List<VictimProfile> result = new List<VictimProfile>();
        List<VictimProfile> tempPool = new List<VictimProfile>(pool);

        for (int i = 0; i < amount; i++)
        {
            if (tempPool.Count == 0) break;
            int rand = Random.Range(0, tempPool.Count);
            result.Add(tempPool[rand]);
            tempPool.RemoveAt(rand);
        }
        return result;
    }

    private void ShuffleList(List<VictimProfile> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            VictimProfile temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // =========================================================================
    // PROCEDURAL HEXTECH UI BUILDER
    // =========================================================================

    private void ResolveFont()
    {
        if (nameTexts != null)
        {
            foreach (var t in nameTexts)
            {
                if (t != null && t.font != null) { gameFont = t.font; return; }
            }
        }
        if (jobTexts != null)
        {
            foreach (var t in jobTexts)
            {
                if (t != null && t.font != null) { gameFont = t.font; return; }
            }
        }
        if (selectionPanel != null)
        {
            var anyTmp = selectionPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (anyTmp != null && anyTmp.font != null) { gameFont = anyTmp.font; return; }
        }
        gameFont = TMP_Settings.defaultFontAsset;
    }

    private void HideLegacyChildren()
    {
        if (selectionPanel == null) return;
        foreach (Transform child in selectionPanel.transform)
        {
            if (matchmakingRoot == null || child.gameObject != matchmakingRoot)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void BuildMatchmakingUI()
    {
        if (matchmakingRoot != null) return;

        GenerateProceduralSprites();
        HideLegacyChildren();

        // Chỉnh màu nền selectionPanel sang tone tối sâu kiểu Hextech / Cyberpunk Compound
        Image panelBg = selectionPanel.GetComponent<Image>();
        if (panelBg != null) panelBg.color = new Color(0.03f, 0.05f, 0.09f, 0.96f);

        matchmakingRoot = CreateUIObj("LoLMatchmakingRoot", selectionPanel.transform);
        StretchFull(matchmakingRoot.GetComponent<RectTransform>());

        // Khung tổng trung tâm 1640x940 để cân đối ở mọi màn hình
        GameObject mainFrame = CreatePanel("HextechFrame", matchmakingRoot.transform,
            new Color(0.05f, 0.09f, 0.14f, 0.98f), new Color(0.78f, 0.62f, 0.25f, 0.65f), 2f);
        RectTransform frameRt = mainFrame.GetComponent<RectTransform>();
        frameRt.sizeDelta = new Vector2(1560f, 920f);

        // ----- TOP HEADER BAR -----
        GameObject headerBar = CreatePanel("HeaderBar", frameRt,
            new Color(0.04f, 0.07f, 0.12f, 1f), new Color(0.78f, 0.62f, 0.25f, 0.45f), 1.5f);
        RectTransform headerRt = headerBar.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0f, 1f);
        headerRt.anchorMax = new Vector2(1f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = new Vector2(0f, -18f);
        headerRt.sizeDelta = new Vector2(-48f, 92f);

        txtHeaderDay = CreateTMP("TxtHeaderDay", headerRt, "TAM GIÁC VÀNG | NGÀY 1", 30, TextAlignmentOptions.MidlineLeft, new Color(0.95f, 0.84f, 0.45f, 1f));
        RectTransform hdRt = txtHeaderDay.rectTransform;
        hdRt.anchorMin = new Vector2(0f, 0f);
        hdRt.anchorMax = new Vector2(0.45f, 1f);
        hdRt.offsetMin = new Vector2(28f, 0f);
        hdRt.offsetMax = new Vector2(0f, 0f);

        txtHeaderStats = CreateTMP("TxtHeaderStats", headerRt, "", 24, TextAlignmentOptions.MidlineRight, Color.white);
        RectTransform hsRt = txtHeaderStats.rectTransform;
        hsRt.anchorMin = new Vector2(0.42f, 0f);
        hsRt.anchorMax = new Vector2(1f, 1f);
        hsRt.offsetMin = new Vector2(0f, 0f);
        hsRt.offsetMax = new Vector2(-28f, 0f);

        // ----- BOTTOM WARNING / STATUS BAR -----
        GameObject footerBar = CreatePanel("FooterBar", frameRt,
            new Color(0.035f, 0.06f, 0.10f, 1f), new Color(0.25f, 0.38f, 0.48f, 0.4f), 1.2f);
        RectTransform footerRt = footerBar.GetComponent<RectTransform>();
        footerRt.anchorMin = new Vector2(0f, 0f);
        footerRt.anchorMax = new Vector2(1f, 0f);
        footerRt.pivot = new Vector2(0.5f, 0f);
        footerRt.anchoredPosition = new Vector2(0f, 18f);
        footerRt.sizeDelta = new Vector2(-48f, 64f);

        txtWarningBanner = CreateTMP("TxtWarningBanner", footerRt, "", 22, TextAlignmentOptions.Center, new Color(0.75f, 0.85f, 0.92f, 1f));
        StretchFull(txtWarningBanner.rectTransform, 20f, 8f);

        // Khu vực nội dung ở giữa
        GameObject bodyArea = CreateUIObj("BodyArea", frameRt);
        RectTransform bodyRt = bodyArea.GetComponent<RectTransform>();
        bodyRt.anchorMin = new Vector2(0f, 0f);
        bodyRt.anchorMax = new Vector2(1f, 1f);
        bodyRt.offsetMin = new Vector2(28f, 96f);
        bodyRt.offsetMax = new Vector2(-28f, -124f);

        BuildLobbyView(bodyRt);
        BuildLockInView(bodyRt);
        BuildMatchFoundModal(matchmakingRoot.transform);
    }

    private void BuildLobbyView(Transform parent)
    {
        lobbyView = CreateUIObj("LobbyView", parent);
        StretchFull(lobbyView.GetComponent<RectTransform>());

        // Dải tag chế độ phía trên
        GameObject roleRibbon = CreatePanel("ModeRibbon", lobbyView.transform,
            new Color(0.07f, 0.12f, 0.19f, 0.9f), new Color(0.08f, 0.65f, 0.78f, 0.45f), 1.5f);
        RectTransform ribRt = roleRibbon.GetComponent<RectTransform>();
        ribRt.anchorMin = new Vector2(0.5f, 1f);
        ribRt.anchorMax = new Vector2(0.5f, 1f);
        ribRt.pivot = new Vector2(0.5f, 1f);
        ribRt.anchoredPosition = new Vector2(0f, -14f);
        ribRt.sizeDelta = new Vector2(960f, 54f);

        TextMeshProUGUI txtMode = CreateTMP("TxtModeInfo", ribRt,
            "<color=#00E5FF><b>GHÉP TRẬN TRỰC TUYẾN</b></color>   •   KHU VỰC: <color=#FFD166>ĐÔNG NAM Á (ẨN IP)</color>   •   CƠ CHẾ: <color=#E2E8F0>QUÉT CON MỒI NGẪU NHIÊN</color>",
            21, TextAlignmentOptions.Center, Color.white);
        StretchFull(txtMode.rectTransform);

        // Vòng Radar Trung Tâm (Hextech Crest)
        GameObject radarHolder = CreateUIObj("RadarCenter", lobbyView.transform);
        RectTransform rhRt = radarHolder.GetComponent<RectTransform>();
        rhRt.anchoredPosition = new Vector2(0f, 70f);
        rhRt.sizeDelta = new Vector2(360f, 360f);

        GameObject pulseGo = CreateImageObj("RadarPulse", rhRt, ringSprite, new Color(0f, 0.9f, 1f, 0.25f));
        radarPulseRing = pulseGo.GetComponent<RectTransform>();
        radarPulseRing.sizeDelta = new Vector2(310f, 310f);

        GameObject outerGo = CreateImageObj("RadarOuterRing", rhRt, segmentedRingSprite, new Color(0.84f, 0.67f, 0.27f, 0.92f));
        radarOuterRing = outerGo.GetComponent<RectTransform>();
        radarOuterRing.sizeDelta = new Vector2(320f, 320f);

        GameObject innerGo = CreateImageObj("RadarInnerRing", rhRt, ringSprite, new Color(0.05f, 0.82f, 0.92f, 0.85f));
        radarInnerRing = innerGo.GetComponent<RectTransform>();
        radarInnerRing.sizeDelta = new Vector2(246f, 246f);

        GameObject coreBg = CreateImageObj("RadarCore", rhRt, solidCircleSprite, new Color(0.04f, 0.08f, 0.13f, 0.96f));
        coreBg.GetComponent<RectTransform>().sizeDelta = new Vector2(216f, 216f);

        GameObject glowGo = CreateImageObj("RadarCenterGlow", rhRt, solidCircleSprite, new Color(0.06f, 0.72f, 0.85f, 0.25f));
        radarCenterGlow = glowGo.GetComponent<Image>();
        glowGo.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 180f);

        txtQueueTimer = CreateTMP("TxtQueueTimer", rhRt, "CHỜ LỆNH", 42, TextAlignmentOptions.Center, Color.white);
        txtQueueTimer.fontStyle = FontStyles.Bold;
        txtQueueTimer.rectTransform.sizeDelta = new Vector2(210f, 80f);

        // Chữ trạng thái & flavor text
        txtQueueTitle = CreateTMP("TxtQueueTitle", lobbyView.transform, "SẴN SÀNG QUÉT MỤC TIÊU", 32, TextAlignmentOptions.Center, Color.white);
        txtQueueTitle.fontStyle = FontStyles.Bold;
        txtQueueTitle.rectTransform.anchoredPosition = new Vector2(0f, -145f);
        txtQueueTitle.rectTransform.sizeDelta = new Vector2(1000f, 48f);

        txtQueueFlavor = CreateTMP("TxtQueueFlavor", lobbyView.transform, "", 22, TextAlignmentOptions.Center, new Color(0.74f, 0.82f, 0.90f, 1f));
        txtQueueFlavor.rectTransform.anchoredPosition = new Vector2(0f, -198f);
        txtQueueFlavor.rectTransform.sizeDelta = new Vector2(1060f, 64f);

        // Nút TÌM CON MỒI / HỦY TÌM phong cách nút FIND MATCH Liên Minh
        btnToggleQueue = CreateStyledButton("BtnToggleQueue", lobbyView.transform,
            new Vector2(380f, 82f), new Vector2(0f, -295f),
            new Color(0.05f, 0.45f, 0.56f, 1f), new Color(0.88f, 0.71f, 0.30f, 1f),
            "TÌM CON MỒI", 30, out btnToggleQueueBg, out txtToggleQueueLabel);
        btnToggleQueue.onClick.AddListener(ToggleQueueClicked);
    }

    private void BuildMatchFoundModal(Transform parent)
    {
        // Lớp phủ tối che nền
        matchFoundModal = CreateUIObj("MatchFoundModal", parent);
        StretchFull(matchFoundModal.GetComponent<RectTransform>());
        Image overlay = matchFoundModal.AddComponent<Image>();
        overlay.color = new Color(0.01f, 0.02f, 0.05f, 0.86f);

        // Thẻ popup chính ở giữa
        GameObject card = CreatePanel("MatchModalCard", matchFoundModal.transform,
            new Color(0.05f, 0.09f, 0.14f, 0.99f), new Color(0.88f, 0.70f, 0.28f, 0.95f), 3f);
        matchModalCard = card.GetComponent<RectTransform>();
        matchModalCard.sizeDelta = new Vector2(760f, 640f);

        txtMatchFoundHeader = CreateTMP("TxtMatchHeader", matchModalCard, "ĐÃ TÌM THẤY MỤC TIÊU!", 38, TextAlignmentOptions.Center, new Color(0.96f, 0.84f, 0.44f, 1f));
        txtMatchFoundHeader.fontStyle = FontStyles.Bold;
        txtMatchFoundHeader.rectTransform.anchoredPosition = new Vector2(0f, 250f);
        txtMatchFoundHeader.rectTransform.sizeDelta = new Vector2(700f, 56f);

        // Vòng tròn đếm ngược Radial 360 kiểu Liên Minh
        GameObject dialHolder = CreateUIObj("DialHolder", matchModalCard);
        RectTransform dialRt = dialHolder.GetComponent<RectTransform>();
        dialRt.anchoredPosition = new Vector2(0f, 88f);
        dialRt.sizeDelta = new Vector2(236f, 236f);

        GameObject glowBg = CreateImageObj("DialGlow", dialRt, ringSprite, new Color(0.85f, 0.68f, 0.28f, 0.45f));
        radialTimerGlow = glowBg.GetComponent<Image>();
        glowBg.GetComponent<RectTransform>().sizeDelta = new Vector2(236f, 236f);

        GameObject trackBg = CreateImageObj("DialTrack", dialRt, ringSprite, new Color(0.12f, 0.18f, 0.26f, 0.9f));
        trackBg.GetComponent<RectTransform>().sizeDelta = new Vector2(214f, 214f);

        GameObject fillGo = CreateImageObj("RadialTimerFill", dialRt, ringSprite, new Color(0.04f, 0.85f, 0.86f, 1f));
        radialTimerFill = fillGo.GetComponent<Image>();
        radialTimerFill.type = Image.Type.Filled;
        radialTimerFill.fillMethod = Image.FillMethod.Radial360;
        radialTimerFill.fillOrigin = (int)Image.Origin360.Top;
        radialTimerFill.fillClockwise = false;
        radialTimerFill.fillAmount = 1f;
        fillGo.GetComponent<RectTransform>().sizeDelta = new Vector2(214f, 214f);

        GameObject avatarFrame = CreateImageObj("TeaserAvatarBg", dialRt, solidCircleSprite, new Color(0.08f, 0.12f, 0.18f, 1f));
        avatarFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(168f, 168f);

        GameObject avatarGo = CreateImageObj("TeaserAvatar", avatarFrame.transform, solidCircleSprite, Color.white);
        imgTeaserAvatar = avatarGo.GetComponent<Image>();
        imgTeaserAvatar.preserveAspect = true;
        avatarGo.GetComponent<RectTransform>().sizeDelta = new Vector2(146f, 146f);

        // Góc đếm ngược giây nổi trên dial
        GameObject countdownPill = CreatePanel("CountdownPill", dialRt,
            new Color(0.03f, 0.06f, 0.10f, 0.96f), new Color(0.88f, 0.70f, 0.28f, 0.8f), 1.5f);
        RectTransform cpRt = countdownPill.GetComponent<RectTransform>();
        cpRt.anchoredPosition = new Vector2(0f, -104f);
        cpRt.sizeDelta = new Vector2(96f, 42f);

        txtMatchCountdown = CreateTMP("TxtCountdown", cpRt, "10s", 26, TextAlignmentOptions.Center, Color.white);
        txtMatchCountdown.fontStyle = FontStyles.Bold;
        StretchFull(txtMatchCountdown.rectTransform);

        // Teaser Dossier Box
        GameObject teaserBox = CreatePanel("TeaserBox", matchModalCard,
            new Color(0.07f, 0.11f, 0.17f, 0.95f), new Color(0.24f, 0.36f, 0.48f, 0.55f), 1.5f);
        RectTransform tbRt = teaserBox.GetComponent<RectTransform>();
        tbRt.anchoredPosition = new Vector2(0f, -92f);
        tbRt.sizeDelta = new Vector2(640f, 134f);

        txtTeaserRisk = CreateTMP("TxtTeaserRisk", tbRt, "", 25, TextAlignmentOptions.Center, Color.white);
        txtTeaserRisk.rectTransform.anchoredPosition = new Vector2(0f, 36f);
        txtTeaserRisk.rectTransform.sizeDelta = new Vector2(600f, 36f);

        txtTeaserReward = CreateTMP("TxtTeaserReward", tbRt, "", 26, TextAlignmentOptions.Center, Color.white);
        txtTeaserReward.rectTransform.anchoredPosition = new Vector2(0f, 0f);
        txtTeaserReward.rectTransform.sizeDelta = new Vector2(600f, 38f);

        txtTeaserHint = CreateTMP("TxtTeaserHint", tbRt, "", 20, TextAlignmentOptions.Center, new Color(0.75f, 0.84f, 0.92f, 1f));
        txtTeaserHint.rectTransform.anchoredPosition = new Vector2(0f, -38f);
        txtTeaserHint.rectTransform.sizeDelta = new Vector2(600f, 40f);

        // Nút CHẤP NHẬN / TỪ CHỐI
        btnAcceptMatch = CreateStyledButton("BtnAcceptMatch", matchModalCard,
            new Vector2(280f, 76f), new Vector2(-155f, -225f),
            new Color(0.05f, 0.52f, 0.48f, 1f), new Color(0.18f, 0.92f, 0.82f, 0.95f),
            "CHẤP NHẬN", 28, out _, out _);
        btnAcceptMatch.onClick.AddListener(AcceptMatch);

        btnDeclineMatch = CreateStyledButton("BtnDeclineMatch", matchModalCard,
            new Vector2(240f, 76f), new Vector2(155f, -225f),
            new Color(0.45f, 0.12f, 0.17f, 1f), new Color(0.92f, 0.32f, 0.42f, 0.9f),
            "TỪ CHỐI", 26, out _, out _);
        btnDeclineMatch.onClick.AddListener(() => DeclineMatch(isTimeout: false));

        matchFoundModal.SetActive(false);
    }

    private void BuildLockInView(Transform parent)
    {
        lockInView = CreateUIObj("LockInView", parent);
        StretchFull(lockInView.GetComponent<RectTransform>());

        GameObject lockCard = CreatePanel("LockInCard", lockInView.transform,
            new Color(0.06f, 0.11f, 0.17f, 0.98f), new Color(0.88f, 0.71f, 0.28f, 0.9f), 2.5f);
        RectTransform lcRt = lockCard.GetComponent<RectTransform>();
        lcRt.sizeDelta = new Vector2(920f, 520f);

        TextMeshProUGUI txtBanner = CreateTMP("TxtLockBanner", lcRt, "ĐÃ KHÓA MỤC TIÊU — ĐANG MỞ ĐƯỜNG DÂY", 28, TextAlignmentOptions.Center, new Color(0.04f, 0.88f, 0.86f, 1f));
        txtBanner.fontStyle = FontStyles.Bold;
        txtBanner.rectTransform.anchoredPosition = new Vector2(0f, 200f);
        txtBanner.rectTransform.sizeDelta = new Vector2(860f, 44f);

        GameObject avatarHolder = CreateImageObj("LockAvatarFrame", lcRt, solidCircleSprite, new Color(0.85f, 0.68f, 0.28f, 0.9f));
        avatarHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260f, 18f);
        avatarHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 210f);

        GameObject avatarInner = CreateImageObj("LockAvatar", avatarHolder.transform, solidCircleSprite, Color.white);
        imgLockInAvatar = avatarInner.GetComponent<Image>();
        imgLockInAvatar.preserveAspect = true;
        avatarInner.GetComponent<RectTransform>().sizeDelta = new Vector2(192f, 192f);

        txtLockInName = CreateTMP("TxtLockName", lcRt, "", 38, TextAlignmentOptions.MidlineLeft, Color.white);
        txtLockInName.rectTransform.anchoredPosition = new Vector2(120f, 82f);
        txtLockInName.rectTransform.sizeDelta = new Vector2(500f, 52f);

        txtLockInJob = CreateTMP("TxtLockJob", lcRt, "", 24, TextAlignmentOptions.MidlineLeft, new Color(0.78f, 0.85f, 0.92f, 1f));
        txtLockInJob.rectTransform.anchoredPosition = new Vector2(120f, 34f);
        txtLockInJob.rectTransform.sizeDelta = new Vector2(500f, 38f);

        txtLockInBadge = CreateTMP("TxtLockBadge", lcRt, "", 24, TextAlignmentOptions.MidlineLeft, Color.white);
        txtLockInBadge.rectTransform.anchoredPosition = new Vector2(120f, -14f);
        txtLockInBadge.rectTransform.sizeDelta = new Vector2(500f, 38f);

        txtLockInReward = CreateTMP("TxtLockReward", lcRt, "", 24, TextAlignmentOptions.MidlineLeft, Color.white);
        txtLockInReward.rectTransform.anchoredPosition = new Vector2(120f, -64f);
        txtLockInReward.rectTransform.sizeDelta = new Vector2(500f, 48f);

        // Thanh tải kết nối
        GameObject barTrack = CreatePanel("ProgressTrack", lcRt,
            new Color(0.03f, 0.05f, 0.08f, 1f), new Color(0.28f, 0.42f, 0.52f, 0.6f), 1f);
        RectTransform btRt = barTrack.GetComponent<RectTransform>();
        btRt.anchoredPosition = new Vector2(0f, -142f);
        btRt.sizeDelta = new Vector2(780f, 20f);

        GameObject barFill = CreateImageObj("ProgressFill", btRt, roundedBoxSprite, new Color(0.04f, 0.88f, 0.86f, 1f));
        imgLockInProgressFill = barFill.GetComponent<Image>();
        imgLockInProgressFill.type = Image.Type.Filled;
        imgLockInProgressFill.fillMethod = Image.FillMethod.Horizontal;
        imgLockInProgressFill.fillAmount = 0f;
        StretchFull(barFill.GetComponent<RectTransform>(), 2f, 2f);

        btnEnterChatNow = CreateStyledButton("BtnEnterChatNow", lcRt,
            new Vector2(300f, 56f), new Vector2(0f, -205f),
            new Color(0.08f, 0.36f, 0.48f, 1f), new Color(0.85f, 0.68f, 0.28f, 0.9f),
            "VÀO KỊCH BẢN NGAY", 22, out _, out _);
        btnEnterChatNow.onClick.AddListener(LaunchMinigameForMatchedVictim);

        lockInView.SetActive(false);
    }

    // =========================================================================
    // UI FACTORY HELPERS
    // =========================================================================

    private GameObject CreateUIObj(string objName, Transform parent)
    {
        GameObject go = new GameObject(objName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private void StretchFull(RectTransform rt, float padX = 0f, float padY = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padX, padY);
        rt.offsetMax = new Vector2(-padX, -padY);
    }

    private GameObject CreatePanel(string objName, Transform parent, Color bgColor, Color borderColor, float borderWidth)
    {
        GameObject go = CreateUIObj(objName, parent);
        Image img = go.AddComponent<Image>();
        img.sprite = roundedBoxSprite;
        img.type = Image.Type.Sliced;
        img.color = bgColor;

        if (borderWidth > 0f && borderColor.a > 0f)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(borderWidth, borderWidth);
        }
        return go;
    }

    private GameObject CreateImageObj(string objName, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = CreateUIObj(objName, parent);
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return go;
    }

    private TextMeshProUGUI CreateTMP(string objName, Transform parent, string text, float fontSize, TextAlignmentOptions align, Color color)
    {
        GameObject go = CreateUIObj(objName, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (gameFont != null) tmp.font = gameFont;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.richText = true;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    private Button CreateStyledButton(string objName, Transform parent, Vector2 size, Vector2 pos, Color bgColor, Color borderColor, string labelText, float fontSize, out Image bgImg, out TextMeshProUGUI tmpLabel)
    {
        GameObject go = CreatePanel(objName, parent, bgColor, borderColor, 2f);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        bgImg = go.GetComponent<Image>();

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        btn.colors = cb;

        tmpLabel = CreateTMP("Label", rt, labelText, fontSize, TextAlignmentOptions.Center, Color.white);
        tmpLabel.fontStyle = FontStyles.Bold;
        StretchFull(tmpLabel.rectTransform, 10f, 4f);

        return btn;
    }

    // =========================================================================
    // PROCEDURAL TEXTURES & SYNTHESIZED AUDIO
    // =========================================================================

    private void GenerateProceduralSprites()
    {
        if (roundedBoxSprite != null) return;

        roundedBoxSprite = CreateRoundedRectSprite(64, 64, 12);
        solidCircleSprite = CreateCircleSprite(256, 0f, 1f);
        ringSprite = CreateCircleSprite(256, 0.78f, 1f);
        segmentedRingSprite = CreateSegmentedRingSprite(256, 0.84f, 0.98f, 6, 0.18f);
    }

    private Sprite CreateRoundedRectSprite(int size, int radius, int borderSlice)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        generatedTextures.Add(tex);

        Color[] pixels = new Color[size * size];
        float r = 10f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Abs(x - (size - 1) * 0.5f) - (size * 0.5f - r - 1f));
                float dy = Mathf.Max(0f, Mathf.Abs(y - (size - 1) * 0.5f) - (size * 0.5f - r - 1f));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(r - dist + 0.5f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(borderSlice, borderSlice, borderSlice, borderSlice));
        generatedSprites.Add(sp);
        return sp;
    }

    private Sprite CreateCircleSprite(int size, float innerRatio, float outerRatio)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        generatedTextures.Add(tex);

        Color[] pixels = new Color[size * size];
        float half = (size - 1) * 0.5f;
        float aa = 2.0f / half;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - half) / half;
                float ny = (y - half) / half;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                float outerAlpha = Mathf.Clamp01((outerRatio - dist) / aa);
                float innerAlpha = innerRatio <= 0f ? 1f : Mathf.Clamp01((dist - innerRatio) / aa);
                pixels[y * size + x] = new Color(1f, 1f, 1f, outerAlpha * innerAlpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        generatedSprites.Add(sp);
        return sp;
    }

    private Sprite CreateSegmentedRingSprite(int size, float innerRatio, float outerRatio, int segments, float gapRadians)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        generatedTextures.Add(tex);

        Color[] pixels = new Color[size * size];
        float half = (size - 1) * 0.5f;
        float aa = 2.0f / half;
        float segStep = (Mathf.PI * 2f) / segments;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - half) / half;
                float ny = (y - half) / half;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                float angle = Mathf.Atan2(ny, nx);
                if (angle < 0f) angle += Mathf.PI * 2f;

                float mod = Mathf.Repeat(angle, segStep);
                float segAlpha = (mod > gapRadians * 0.5f && mod < segStep - gapRadians * 0.5f) ? 1f : 0f;

                float outerAlpha = Mathf.Clamp01((outerRatio - dist) / aa);
                float innerAlpha = Mathf.Clamp01((dist - innerRatio) / aa);
                pixels[y * size + x] = new Color(1f, 1f, 1f, outerAlpha * innerAlpha * segAlpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        generatedSprites.Add(sp);
        return sp;
    }

    private void EnsureAudioSetup()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
            }
        }

        if (queueStartSound == null) queueStartSound = SynthesizeSweep("QueueStartSFX", 0.22f, 380f, 660f, 0.25f);
        if (matchFoundSound == null) matchFoundSound = SynthesizeChord("MatchFoundSFX", 1.1f, new[] { 293.66f, 440f, 587.33f, 880f }, 0.32f);
        if (countdownTickSound == null) countdownTickSound = SynthesizeSweep("TickSFX", 0.08f, 920f, 740f, 0.22f);
        if (acceptLockInSound == null) acceptLockInSound = SynthesizeChord("LockInSFX", 0.55f, new[] { 440f, 659.25f, 880f }, 0.28f);
        if (declineSound == null) declineSound = SynthesizeSweep("DeclineSFX", 0.30f, 220f, 130f, 0.30f);
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    private AudioClip SynthesizeSweep(string clipName, float duration, float startFreq, float endFreq, float gain)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * sampleRate));
        float[] samples = new float[sampleCount];

        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            phase += 2f * Mathf.PI * freq / sampleRate;
            float env = Mathf.Sin(Mathf.Min(1f, t * 12f) * Mathf.PI * 0.5f) * Mathf.Pow(1f - t, 1.5f);
            samples[i] = Mathf.Sin(phase) * env * gain;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        generatedClips.Add(clip);
        return clip;
    }

    private AudioClip SynthesizeChord(string clipName, float duration, float[] freqs, float gain)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * sampleRate));
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float attack = Mathf.Clamp01(t * 40f);
            float decay = Mathf.Exp(-3.5f * t);
            float sum = 0f;
            for (int f = 0; f < freqs.Length; f++)
            {
                sum += Mathf.Sin(2f * Mathf.PI * freqs[f] * (i / (float)sampleRate));
            }
            samples[i] = (sum / freqs.Length) * attack * decay * gain;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        generatedClips.Add(clip);
        return clip;
    }
}
