using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShiftSummaryUI : MonoBehaviour
{
    [Header("Giao diện UI")]
    public GameObject summaryPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statsText;

    [Header("Nút bấm")]
    public Button stopButton;

    [Header("Âm thanh (KÉO LOA VÀ NHẠC VÀO ĐÂY)")]
    // BẠN PHẢI KÉO CÙNG CÁI AUDIO SOURCE MÀ SCAM MINIGAME ĐANG DÙNG VÀO Ô NÀY!
    public AudioSource bgmSource;
    public AudioClip endShiftBgm;

    void Start()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false);
        stopButton.onClick.AddListener(EndShift);
    }

    public void ShowForceEndShift()
    {
        summaryPanel.SetActive(true);
        titleText.text = "KẾT THÚC CA LÀM!";

        int success = GameManager.instance.successfulScamsToday;
        int target = GameManager.instance.targetKPI;
        float money = GameManager.instance.money;
        int baseKPI = 2; // Chỉ tiêu sinh tồn

        string performanceMessage = "";

        if (success >= baseKPI)
        {
            if (success >= target && success > baseKPI)
            {
                int nextTarget = Mathf.Min(success + 1, 5);
                float nextCommission = (GameManager.instance.currentCommissionRate + 0.05f) * 100f;

                performanceMessage = $"<color=#00FF00>VƯỢT CHỈ TIÊU SINH TỒN!</color>\n" +
                                     $"Bạn đang làm việc rất chăm chỉ để kiếm thêm tiền.\n" +
                                     $"<color=#00FFFF>+ THƯỞNG:</color> Hoa hồng ngày mai tăng lên {nextCommission}%.\n" +
                                     $"<color=#FF9900>- ÁP LỰC:</color> Quản lý nâng KPI thưởng ngày mai lên {nextTarget} người!";
            }
            else if (success < target)
            {
                performanceMessage = $"<color=#FFFF00>ĐẠT CHỈ TIÊU CƠ BẢN (An toàn)</color>\n" +
                                     $"Bạn không bị chích điện, nhưng do không giữ được phong độ xuất sắc...\n" +
                                     $"<color=#FF0000>- MẤT THƯỞNG:</color> Hoa hồng bị cắt giảm, KPI reset về mức cơ bản (2 người)!";
            }
            else
            {
                performanceMessage = $"<color=#FFFF00>ĐẠT CHỈ TIÊU CƠ BẢN (An toàn)</color>\n" +
                                     $"Bạn đã hoàn thành đủ số lượng tối thiểu để giữ mạng sống.";
            }
        }
        else
        {
            performanceMessage = $"<color=#FF0000>TỆ HẠI! Không đạt KPI sinh tồn ({baseKPI} người).</color>\n\n" +
                                 "Bạn sẽ bị <color=#FF0000>CHÍCH ĐIỆN PHẠT NẶNG</color> ngay bây giờ!\n" +
                                 "Mọi mốc thưởng đều bị hủy bỏ.";
        }

        string displayTarget = (target > baseKPI) ? $"{baseKPI} (Thưởng: {target})" : $"{baseKPI}";

        statsText.text = $"Bạn đã kết thúc ca làm việc hôm nay.\n\n" +
                         $"Đã lừa được: <color=#00FF00>{success}</color> / Chỉ tiêu: {displayTarget}\n" +
                         $"Tổng tiền hiện có: <color=#FFFF00>{money.ToString("N0")} VNĐ</color>\n\n" +
                         performanceMessage;

        stopButton.GetComponentInChildren<TextMeshProUGUI>().text = "Tổng kết & Trở về buồng giam";

        if (bgmSource != null && endShiftBgm != null)
        {
            bgmSource.clip = endShiftBgm;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    private void EndShift()
    {
        summaryPanel.SetActive(false);
        GameManager.instance.EndDaySummary();
        GameManager.instance.TransitionToPhase(GamePhase.Night);
    }
}