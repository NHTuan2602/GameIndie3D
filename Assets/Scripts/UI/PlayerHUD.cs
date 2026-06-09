using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("--- HIỆU ỨNG NHẢY MÁU (DAMAGE POPUP) ---")]
    [Tooltip("Tạo 1 cái TextMeshPro nằm giữa màn hình (Hoặc trên đầu nhân vật), kéo vào đây")]
    public TextMeshProUGUI hpPopupText;
    private Coroutine popupRoutine;

    private int lastHp = -1;
    private float lastMoney = -1f;
    private int lastKpi = -1;
    private float lastCommission = -1f;

    void Start()
    {
        displayTimer = showDuration;
        if (hpPopupText != null) hpPopupText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (GameManager.instance == null) return;

        bool valueChanged = false;

        // Bắt sự kiện thay đổi
        if (GameManager.instance.hp != lastHp)
        {
            // Nếu máu thực sự bị thay đổi (không phải lúc mới load game), gọi hiệu ứng nảy số
            if (lastHp != -1)
            {
                int hpDiff = GameManager.instance.hp - lastHp;
                ShowHPPopup(hpDiff);
            }
            lastHp = GameManager.instance.hp;
            valueChanged = true;
        }

        if (GameManager.instance.money != lastMoney) { lastMoney = GameManager.instance.money; valueChanged = true; }
        if (GameManager.instance.successfulScamsToday != lastKpi) { lastKpi = GameManager.instance.successfulScamsToday; valueChanged = true; }
        if (GameManager.instance.currentCommissionRate != lastCommission) { lastCommission = GameManager.instance.currentCommissionRate; valueChanged = true; }

        if (valueChanged || Input.GetKeyDown(KeyCode.Tab))
        {
            displayTimer = showDuration;
            UpdateUIValues();
        }

        if (hudCanvasGroup != null)
        {
            if (displayTimer > 0)
            {
                displayTimer -= Time.deltaTime;
                hudCanvasGroup.alpha = Mathf.MoveTowards(hudCanvasGroup.alpha, 1f, Time.deltaTime * 5f);
            }
            else
            {
                hudCanvasGroup.alpha = Mathf.MoveTowards(hudCanvasGroup.alpha, 0f, Time.deltaTime * 2f);
            }
        }
    }

    private void UpdateUIValues()
    {
        if (hpSlider != null) hpSlider.value = (float)GameManager.instance.hp / GameManager.instance.maxHp;
        if (moneyText != null) moneyText.text = GameManager.instance.money.ToString("N0") + " VNĐ";
        if (kpiText != null)
        {
            float commissionPercent = GameManager.instance.currentCommissionRate * 100f;
            int baseKPI = 2;
            string displayTarget = (GameManager.instance.targetKPI > baseKPI) ? $"{baseKPI} (Thưởng: {GameManager.instance.targetKPI})" : $"{baseKPI}";
            kpiText.text = $"KPI: {GameManager.instance.successfulScamsToday} / {displayTarget}\nHoa hồng: <color=#00FF00>{commissionPercent}%</color>";
        }
    }

    // ==========================================
    // HÀM HIỆN CHỮ NHẢY MÁU
    // ==========================================
    public void ShowHPPopup(int amount)
    {
        if (hpPopupText == null || amount == 0) return;

        if (popupRoutine != null) StopCoroutine(popupRoutine);
        popupRoutine = StartCoroutine(AnimateHPPopup(amount));
    }

    IEnumerator AnimateHPPopup(int amount)
    {
        hpPopupText.gameObject.SetActive(true);

        // Thiết lập màu và dấu (+/-)
        if (amount > 0)
        {
            hpPopupText.text = $"+{amount} HP";
            hpPopupText.color = new Color(0f, 1f, 0f, 1f); // Xanh lá
        }
        else
        {
            hpPopupText.text = $"{amount} HP"; // amount đã có sẵn dấu âm
            hpPopupText.color = new Color(1f, 0f, 0f, 1f); // Đỏ
        }

        // Reset vị trí về giữa
        RectTransform rect = hpPopupText.GetComponent<RectTransform>();
        Vector2 startPos = Vector2.zero; // Hoặc set cứng vị trí bạn muốn
        rect.anchoredPosition = startPos;

        float timer = 0f;
        float duration = 1.5f;

        // Di chuyển bay lên trên và mờ dần
        while (timer < duration)
        {
            timer += Time.deltaTime;
            rect.anchoredPosition += new Vector2(0, 50f * Time.deltaTime); // Bay lên 50 pixel / giây

            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            hpPopupText.color = new Color(hpPopupText.color.r, hpPopupText.color.g, hpPopupText.color.b, alpha);

            yield return null;
        }

        hpPopupText.gameObject.SetActive(false);
    }
}