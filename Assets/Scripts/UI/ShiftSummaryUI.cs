using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ShiftSummaryUI : MonoBehaviour
{
    [Header("Giao diện UI")]
    public GameObject summaryPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statsText;

    [Header("Nút bấm")]
    public Button stopButton;

    [Header("Âm thanh (KÉO LOA VÀ NHẠC VÀO ĐÂY)")]
    public AudioSource bgmSource;
    public AudioClip endShiftBgm;

    private CanvasGroup panelCanvasGroup;

    void Start()
    {
        if (summaryPanel != null)
        {
            panelCanvasGroup = summaryPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = summaryPanel.AddComponent<CanvasGroup>();
            summaryPanel.SetActive(false);
        }

        if (stopButton != null)
        {
            stopButton.onClick.AddListener(EndShift);
        }

        UIButtonJuice.EnhanceAllButtonsIn(gameObject);
    }

    public void ShowForceEndShift()
    {
        summaryPanel.SetActive(true);
        if (panelCanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInRoutine());
        }

        if (titleText != null)
        {
            titleText.text = "<b><color=#E63946>TỔNG KẾT CA LÀM VIỆC</color></b>\n<size=55%><color=#A8DADC>BÁO CÁO HIỆU SUẤT TRONG NGÀY</color></size>";
        }

        int success = GameManager.instance != null ? GameManager.instance.successfulScamsToday : 0;
        int target = GameManager.instance != null ? GameManager.instance.targetKPI : 2;
        float money = GameManager.instance != null ? GameManager.instance.money : 0f;
        int baseKPI = 2; // Chỉ tiêu sinh tồn

        string performanceCard = "";
        string statusBadge = "";

        if (success >= baseKPI)
        {
            if (success >= target && success > baseKPI)
            {
                int nextTarget = Mathf.Min(success + 1, 5);
                float nextCommission = (GameManager.instance != null ? GameManager.instance.currentCommissionRate + 0.05f : 0.1f) * 100f;

                statusBadge = "<color=#06D6A0><b>[ ★ XUẤT SẮC: VƯỢT CHỈ TIÊU ★ ]</b></color>";
                performanceCard = $"<color=#00F5D4>✔ Phần Thưởng:</color> Tăng hoa hồng ngày mai lên <b>{nextCommission:0.#}%</b>.\n" +
                                  $"<color=#FFB703>▲ Áp Lực:</color> Quản lý nâng chỉ tiêu KPI ngày mai lên <b>{nextTarget} người</b>!";
            }
            else if (success < target)
            {
                statusBadge = "<color=#FFD166><b>[ ✔ ĐẠT CHỈ TIÊU CƠ BẢN ]</b></color>";
                performanceCard = "Bạn an toàn, không bị quản lý chích điện hôm nay.\n" +
                                  "<color=#EF476F>▼ Chú ý:</color> Không giữ được phong độ, hoa hồng bị reset về mức cơ bản.";
            }
            else
            {
                statusBadge = "<color=#06D6A0><b>[ ✔ AN TOÀN SINH TỒN ]</b></color>";
                performanceCard = "Bạn đã hoàn thành đủ chỉ tiêu tối thiểu để bảo toàn tính mạng trong hôm nay.";
            }
        }
        else
        {
            statusBadge = "<color=#EF476F><b>[ ✖ TỆ HẠI: KHÔNG ĐẠT KPI SINH TỒN ✖ ]</b></color>";
            performanceCard = $"<color=#FF0054>⚠ HÌNH PHẠT:</color> Bạn sẽ bị <b>CHÍCH ĐIỆN TRA TẤN DÃ MAN</b>!\n" +
                              "Mọi khoản thưởng hoa hồng đều bị tước đoạt.";
        }

        string displayTarget = (target > baseKPI) ? $"{baseKPI} (Thưởng: {target})" : $"{baseKPI}";

        if (statsText != null)
        {
            statsText.text = $"{statusBadge}\n\n" +
                             $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                             $"• Nạn nhân đã sập bẫy: <color=#06D6A0><b>{success}</b></color> / Mục tiêu: <b>{displayTarget}</b>\n" +
                             $"• Tổng tiền tích lũy: <color=#FFD166><b>{money:N0} VNĐ</b></color>\n" +
                             $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                             $"{performanceCard}";
        }

        if (stopButton != null)
        {
            TextMeshProUGUI btnText = stopButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = "<b>XÁC NHẬN & VỀ BUỒNG GIAM  ➔</b>";
            }
        }

        if (bgmSource != null && endShiftBgm != null)
        {
            bgmSource.clip = endShiftBgm;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    IEnumerator FadeInRoutine()
    {
        panelCanvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.3f);
            yield return null;
        }
        panelCanvasGroup.alpha = 1f;
    }

    private void EndShift()
    {
        summaryPanel.SetActive(false);
        if (GameManager.instance != null)
        {
            GameManager.instance.EndDaySummary();
            GameManager.instance.TransitionToPhase(GamePhase.Night);
        }
    }
}
