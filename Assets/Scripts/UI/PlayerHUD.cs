using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Giao diện Tổng")]
    public CanvasGroup hudCanvasGroup;
    public float showDuration = 3f;
    private float displayTimer = 0f;

    [Header("Thanh Trạng Thái (Sliders)")]
    public Slider hpSlider;

    [Header("Chỉ số (Texts)")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI kpiText;

    // Các biến "Trí nhớ" để so sánh sự thay đổi
    private int lastHp = -1;
    private float lastMoney = -1f;
    private int lastKpi = -1;

    void Start()
    {
        displayTimer = showDuration;
    }

    void Update()
    {
        if (GameManager.instance == null) return;

        bool valueChanged = false;

        // 1. NGƯỜI CANH GÁC: Phát hiện xem có chỉ số nào bị thay đổi không?
        if (GameManager.instance.hp != lastHp) { lastHp = GameManager.instance.hp; valueChanged = true; }
        if (GameManager.instance.money != lastMoney) { lastMoney = GameManager.instance.money; valueChanged = true; }
        if (GameManager.instance.successfulScamsToday != lastKpi) { lastKpi = GameManager.instance.successfulScamsToday; valueChanged = true; }

        // 2. KÍCH HOẠT HIỂN THỊ: Bấm TAB hoặc khi có chỉ số thay đổi
        if (valueChanged || Input.GetKeyDown(KeyCode.Tab))
        {
            displayTimer = showDuration;
            UpdateUIValues();
        }

        // 3. XỬ LÝ LÀM MỜ TỰ ĐỘNG (FADE IN / FADE OUT) RẤT MƯỢT MÀ
        if (hudCanvasGroup != null)
        {
            if (displayTimer > 0)
            {
                displayTimer -= Time.deltaTime;
                // Tăng tốc độ hiện lên (Fade In)
                hudCanvasGroup.alpha = Mathf.MoveTowards(hudCanvasGroup.alpha, 1f, Time.deltaTime * 5f);
            }
            else
            {
                // Từ từ mờ đi (Fade Out)
                hudCanvasGroup.alpha = Mathf.MoveTowards(hudCanvasGroup.alpha, 0f, Time.deltaTime * 2f);
            }
        }
    }

    private void UpdateUIValues()
    {
        if (hpSlider != null)
            hpSlider.value = (float)GameManager.instance.hp / GameManager.instance.maxHp;

        if (moneyText != null)
        {
            moneyText.text = GameManager.instance.money.ToString("N0") + " VNĐ";
        }

        if (kpiText != null)
            kpiText.text = "KPI: " + GameManager.instance.successfulScamsToday + "/" + GameManager.instance.targetKPI;
    }
}