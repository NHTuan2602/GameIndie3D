using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Thanh Trạng Thái (Sliders)")]
    public Slider hpSlider;
    public Slider staminaSlider;

    [Header("Chỉ số (Texts)")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI karmaText;
    public TextMeshProUGUI kpiText;

    void Update()
    {
        // Kiểm tra xem não bộ GameManager có đang tồn tại không
        if (GameManager.instance == null) return;

        // 1. Cập nhật thanh Máu & Thể lực (Value từ 0.0 đến 1.0)
        if (hpSlider != null)
            hpSlider.value = (float)GameManager.instance.hp / GameManager.instance.maxHp;

        if (staminaSlider != null)
            staminaSlider.value = (float)GameManager.instance.stamina / GameManager.instance.maxStamina;

        // 2. Cập nhật Tiền, Đạo đức và KPI
        if (moneyText != null)
            moneyText.text = "$" + GameManager.instance.money.ToString("F0"); // F0 để làm tròn số

        if (karmaText != null)
            karmaText.text = "Nghiệp: " + GameManager.instance.karma;

        if (kpiText != null)
            kpiText.text = "KPI: " + GameManager.instance.successfulScamsToday + "/" + GameManager.instance.targetKPI;
    }
}