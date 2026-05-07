using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShiftSummaryUI : MonoBehaviour
{
    [Header("Giao diện UI")]
    public GameObject summaryPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statsText;

    [Header("Nút bấm")]
    public Button stopButton;      // Chỉ cần 1 nút duy nhất để kết thúc

    void Start()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false);
        stopButton.onClick.AddListener(EndShift);
    }

    // HÀM NÀY SẼ ĐƯỢC GỌI KHI LÀM XONG 5 NGƯỜI
    public void ShowForceEndShift()
    {
        summaryPanel.SetActive(true);
        titleText.text = "KẾT THÚC CA LÀM!";

        // Hiện màu Xanh nếu Đạt KPI, màu Đỏ nếu Trượt KPI
        string kpiColor = (GameManager.instance.successfulScamsToday >= GameManager.instance.targetKPI) ? "#00FF00" : "#FF0000";

        statsText.text = $"Bạn đã tiếp cận đủ 5 nạn nhân hôm nay.\n\n" +
                         $"KPI Đạt được: <color={kpiColor}>{GameManager.instance.successfulScamsToday}/{GameManager.instance.targetKPI}</color>\n" +
                         $"Tổng tiền hiện có: <color=#FFFF00>${GameManager.instance.money}</color>\n" +
                         $"Thể lực còn lại: <color=#FF5555>{GameManager.instance.stamina}/{GameManager.instance.maxStamina}</color>\n\n" +
                         "Hãy chuẩn bị tinh thần nhận báo cáo từ quản lý!";

        stopButton.GetComponentInChildren<TextMeshProUGUI>().text = "Tổng kết & Nghỉ ngơi";
    }

    private void EndShift()
    {
        summaryPanel.SetActive(false);
        // Bấm nút này sẽ gọi GameManager trừ máu (nếu trượt KPI) hoặc cộng tiền (nếu vượt KPI)
        GameManager.instance.EndDaySummary();

        // MỞ KHÓA DÒNG NÀY ĐỂ CHUYỂN SANG SCENE BAN ĐÊM (CAMP SCREEN) KHI BẠN LÀM XONG!
        // UnityEngine.SceneManagement.SceneManager.LoadScene("CampuchiaNightScene"); 
    }
}