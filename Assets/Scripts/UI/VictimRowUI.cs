using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VictimRowUI : MonoBehaviour
{
    [Header("Kéo thả 4 thành phần UI vào đây")]
    public TextMeshProUGUI nameText;       // Cột 1: Tên nạn nhân
    public TextMeshProUGUI infoText;       // Cột 2: Dòng "Giải ngân vay App..."
    public TextMeshProUGUI difficultyText; // Cột 3: Dòng chữ sẽ thay thế "Tài sản..."
    public Button actionButton;            // Cột 4: Nút Kết nối

    public void SetupRow(string vName, string vInfo, string vDiff, int staminaCost)
    {
        if (nameText != null) nameText.text = vName;

        // Giữ nguyên cột 2 hiển thị kiểu lừa đảo
        if (infoText != null) infoText.text = vInfo;

        // ÉP CỘT 3 HIỂN THỊ ĐỘ KHÓ & THỂ LỰC (Chữ Tài sản gõ tay sẽ bị bay màu)
        if (difficultyText != null)
        {
            difficultyText.text = $"Độ khó: <color=#FFFF00>{vDiff}</color>\n<color=#FF5555>-{staminaCost} Thể lực</color>";
        }
    }
}