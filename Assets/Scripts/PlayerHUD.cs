using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Giao diện Tổng")]
    public CanvasGroup hudCanvasGroup; // SẼ KÉO HUDCanvas VÀO ĐÂY
    public float showDuration = 3f;    // Thời gian hiển thị (3 giây)
    private float displayTimer = 0f;

    [Header("Thanh Trạng Thái (Sliders)")]
    public Slider hpSlider;
    public Slider staminaSlider;

    [Header("Chỉ số (Texts)")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI karmaText;
    public TextMeshProUGUI kpiText;

    // Các biến "Trí nhớ" để so sánh sự thay đổi
    private int lastHp = -1;
    private int lastStamina = -1;
    private float lastMoney = -1f;
    private int lastKarma = -1;
    private int lastKpi = -1;

    void Start()
    {
        // Khi mới bật game lên, ép nó hiện ra 3 giây để người chơi nhìn thấy trạng thái hiện tại
        displayTimer = showDuration;
    }

    void Update()
    {
        if (GameManager.instance == null) return;

        bool valueChanged = false;

        // 1. NGƯỜI CANH GÁC: Phát hiện xem có chỉ số nào bị thay đổi không?
        if (GameManager.instance.hp != lastHp) { lastHp = GameManager.instance.hp; valueChanged = true; }
        if (GameManager.instance.stamina != lastStamina) { lastStamina = GameManager.instance.stamina; valueChanged = true; }
        if (GameManager.instance.money != lastMoney) { lastMoney = GameManager.instance.money; valueChanged = true; }
        if (GameManager.instance.karma != lastKarma) { lastKarma = GameManager.instance.karma; valueChanged = true; }
        if (GameManager.instance.successfulScamsToday != lastKpi) { lastKpi = GameManager.instance.successfulScamsToday; valueChanged = true; }

        // 2. NẾU BỊ RÚT MÁU / CỘNG TIỀN -> Cập nhật số liệu và Bật đồng hồ đếm ngược 3 giây!
        if (valueChanged)
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
                // Hiện rõ lên dần dần (Tốc độ x5)
                hudCanvasGroup.alpha = Mathf.MoveTowards(hudCanvasGroup.alpha, 1f, Time.deltaTime * 5f);
            }
            else
            {
                // Mờ đi dần dần khi hết 3 giây (Tốc độ x2)
                hudCanvasGroup.alpha = Mathf.MoveTowards(hudCanvasGroup.alpha, 0f, Time.deltaTime * 2f);
            }
        }
    }

    private void UpdateUIValues()
    {
        if (hpSlider != null)
            hpSlider.value = (float)GameManager.instance.hp / GameManager.instance.maxHp;

        if (staminaSlider != null)
            staminaSlider.value = (float)GameManager.instance.stamina / GameManager.instance.maxStamina;

        if (moneyText != null)
        {
            // Hiển thị VNĐ. Dùng "N0" để có dấu phẩy phân cách hàng ngàn (VD: 25,000,000 VNĐ)
            moneyText.text = GameManager.instance.money.ToString("N0") + " VNĐ";
        }

        if (karmaText != null)
            karmaText.text = "Nghiệp: " + GameManager.instance.karma;

        if (kpiText != null)
            kpiText.text = "KPI: " + GameManager.instance.successfulScamsToday + "/" + GameManager.instance.targetKPI;
    }
}