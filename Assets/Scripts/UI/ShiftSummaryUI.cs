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

        // Lấy dữ liệu từ GameManager
        int success = GameManager.instance.successfulScamsToday;
        int target = GameManager.instance.targetKPI;
        float money = GameManager.instance.money;

        // Xử lý màu sắc KPI
        string kpiColor = (success >= target) ? "#00FF00" : "#FF0000";
        string performanceMessage = "";

        if (success == 5)
        {
            performanceMessage = "Xuất sắc! Bạn đã lừa hoàn hảo 5/5 người. Tổ chức rất hài lòng!\n<color=#FFFF00>Cơ hội thăng chức đang tới. Chú ý: KPI ngày mai sẽ tăng lên!</color>";
        }
        else if (success >= target)
        {
            performanceMessage = "Tốt lắm! Bạn đã đạt đủ chỉ tiêu KPI hôm nay.\nHãy giữ vững phong độ này để bảo toàn mạng sống.";
        }
        else
        {
            performanceMessage = "<color=#FF0000>TỆ HẠI! Bạn không đạt đủ KPI hôm nay.</color>\nHãy chuẩn bị tinh thần đón nhận hình phạt điện giật từ quản lý!";
        }

        statsText.text = $"Bạn đã kết thúc ca làm việc hôm nay.\n\n" +
                         $"KPI Đạt được: <color={kpiColor}>{success}/{target}</color>\n" +
                         $"Tổng tiền hiện có: <color=#FFFF00>{money.ToString("N0")} VNĐ</color>\n\n" +
                         performanceMessage;

        stopButton.GetComponentInChildren<TextMeshProUGUI>().text = "Tổng kết & Nghỉ ngơi";

        // ==========================================
        // ĐÃ FIX: CHUYỂN SANG NHẠC TỔNG KẾT
        // ==========================================
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