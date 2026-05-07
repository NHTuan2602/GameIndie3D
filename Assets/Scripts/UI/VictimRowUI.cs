using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VictimRowUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;       // Kéo Cột 1 vào đây
    public TextMeshProUGUI infoText;       // Kéo Cột 2 vào đây
    public TextMeshProUGUI difficultyText; // Kéo Cột 3 vào đây
    public Button actionButton;            // Kéo Cột 4 vào đây

    // Hàm này sẽ được Manager gọi để điền chữ vào 4 ô
    public void SetupRow(string vName, string vInfo, string vDiff, int staminaCost)
    {
        nameText.text = vName;
        infoText.text = vInfo;

        // Tự động phối màu: Stamina đỏ, Độ khó vàng
        difficultyText.text = $"<color=#FF5555>-{staminaCost} Thể lực</color>\n<color=#FFFF00>{vDiff}</color>";
    }
}