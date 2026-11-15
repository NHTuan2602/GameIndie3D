using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("Giao diện Tổng")]
    public CanvasGroup hudCanvasGroup;
    public float showDuration = 4f;
    private float displayTimer = 0f;

    [Header("Thanh Trạng Thái (Sliders)")]
    public Slider hpSlider;
    [Tooltip("Thanh máu trễ (Ghost Bar) màu cam/trắng để tạo hiệu ứng tụt máu điện ảnh")]
    public Slider ghostHpSlider;

    [Header("Chỉ số (Texts)")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI kpiText;
    [Tooltip("Hiển thị gợi ý phím tắt tiện lợi")]
    public TextMeshProUGUI hotkeyHintText;

    [Header("--- HIỆU ỨNG NHẢY MÁU (DAMAGE POPUP) ---")]
    [Tooltip("Tạo 1 cái TextMeshPro nằm giữa màn hình (Hoặc trên đầu nhân vật), kéo vào đây")]
    public TextMeshProUGUI hpPopupText;
    private Coroutine popupRoutine;

    private int lastHp = -1;
    private float lastMoney = -1f;
    private int lastKpi = -1;
    private float lastCommission = -1f;

    private float targetHpFill = 1f;
    private float ghostHpFill = 1f;
    private Image hpFillImage;
    private Color originalHpColor = new Color(0.9f, 0.2f, 0.2f, 1f);

    void Start()
    {
        displayTimer = showDuration;
        if (hpPopupText != null) hpPopupText.gameObject.SetActive(false);

        if (hpSlider != null && hpSlider.fillRect != null)
        {
            hpFillImage = hpSlider.fillRect.GetComponent<Image>();
            if (hpFillImage != null) originalHpColor = hpFillImage.color;
        }

        // Tự động tìm ghost slider nếu người dùng đặt dưới cùng slider
        if (ghostHpSlider == null && hpSlider != null)
        {
            Transform ghostTr = hpSlider.transform.Find("GhostFillArea");
            if (ghostTr != null) ghostHpSlider = ghostTr.GetComponent<Slider>();
        }

        // Tự động làm đẹp tất cả nút nếu có
        UIButtonJuice.EnhanceAllButtonsIn(gameObject);
    }

    void Update()
    {
        if (GameManager.instance == null) return;

        bool valueChanged = false;

        // Bắt sự kiện thay đổi máu
        if (GameManager.instance.hp != lastHp)
        {
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

        // Cập nhật chuyển động mượt của thanh máu
        AnimateHealthBars();

        // Cảnh báo nhấp nháy khi máu nguy kịch (HP <= 30%)
        UpdateLowHealthPulse();

        // Ẩn hiện mượt mà CanvasGroup HUD
        if (hudCanvasGroup != null)
        {
            if (displayTimer > 0)
            {
                displayTimer -= Time.deltaTime;
                hudCanvasGroup.alpha = Mathf.MoveTowards(hudCanvasGroup.alpha, 1f, Time.deltaTime * 6f);
            }
            else
            {
                hudCanvasGroup.alpha = Mathf.MoveTowards(hudCanvasGroup.alpha, 0f, Time.deltaTime * 2.5f);
            }
        }
    }

    private void UpdateUIValues()
    {
        targetHpFill = Mathf.Clamp01((float)GameManager.instance.hp / GameManager.instance.maxHp);

        // Hiển thị tiền định dạng chuẩn kèm icon
        if (moneyText != null)
        {
            moneyText.text = $"<color=#FFD166>💰 {GameManager.instance.money:N0} VNĐ</color>";
        }

        // Hiển thị KPI và hoa hồng với badge trạng thái đẹp mắt
        if (kpiText != null)
        {
            float commissionPercent = GameManager.instance.currentCommissionRate * 100f;
            int baseKPI = 2;
            int current = GameManager.instance.successfulScamsToday;
            int target = GameManager.instance.targetKPI;

            string statusBadge;
            if (current >= baseKPI)
            {
                statusBadge = "<color=#06D6A0>✔ [ĐẠT CHỈ TIÊU SINH TỒN]</color>";
            }
            else
            {
                statusBadge = "<color=#EF476F>⚠ [NGUY HIỂM: CẦN THÊM NẠN NHÂN]</color>";
            }

            string displayTarget = (target > baseKPI) ? $"{baseKPI} (Thưởng: {target})" : $"{baseKPI}";
            kpiText.text = $"{statusBadge}\n" +
                           $"KPI Hôm nay: <b>{current} / {displayTarget}</b>\n" +
                           $"Hoa hồng: <color=#00F5D4>+{commissionPercent:0.#}%</color>";
        }

        if (hotkeyHintText != null)
        {
            hotkeyHintText.text = "<size=80%><color=#A8DADC>[TAB]</color> Hiện HUD   <color=#A8DADC>[E]</color> Tương tác   <color=#A8DADC>[ESC]</color> Tạm dừng</size>";
        }
    }

    private void AnimateHealthBars()
    {
        if (hpSlider != null)
        {
            // Thanh chính trượt nhanh và mượt
            hpSlider.value = Mathf.MoveTowards(hpSlider.value, targetHpFill, Time.deltaTime * 4f);
        }

        if (ghostHpSlider != null)
        {
            // Thanh ghost trượt chậm hơn để tạo vệt trễ màu cam/trắng
            if (ghostHpSlider.value > targetHpFill)
            {
                ghostHpSlider.value = Mathf.Lerp(ghostHpSlider.value, targetHpFill, Time.deltaTime * 2f);
            }
            else
            {
                ghostHpSlider.value = targetHpFill;
            }
        }
    }

    private void UpdateLowHealthPulse()
    {
        if (hpFillImage == null || GameManager.instance == null) return;

        float hpRatio = (float)GameManager.instance.hp / GameManager.instance.maxHp;
        if (hpRatio <= 0.3f && hpRatio > 0f)
        {
            // Nhấp nháy màu đỏ tươi báo động
            float ping = Mathf.PingPong(Time.time * 4f, 1f);
            hpFillImage.color = Color.Lerp(originalHpColor, new Color(1f, 0.1f, 0.1f, 1f), ping);
        }
        else
        {
            hpFillImage.color = originalHpColor;
        }
    }

    // ==========================================
    // HÀM HIỆN CHỮ NHẢY MÁU VỚI HIỆU ỨNG BOUNCE & POPUP
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
        RectTransform rect = hpPopupText.GetComponent<RectTransform>();

        // Thiết lập màu sắc và định dạng phong cách indie điện ảnh
        if (amount > 0)
        {
            hpPopupText.text = $"<size=120%><b>+{amount} HP</b></size>\n<size=70%><color=#80FF80>HỒI PHỤC</color></size>";
            hpPopupText.color = new Color(0.2f, 1f, 0.4f, 1f);
        }
        else
        {
            hpPopupText.text = $"<size=130%><b>{amount} HP</b></size>\n<size=70%><color=#FF8080>BỊ THƯƠNG</color></size>";
            hpPopupText.color = new Color(1f, 0.2f, 0.2f, 1f);
        }

        Vector2 startPos = Vector2.zero;
        rect.anchoredPosition = startPos;
        rect.localScale = Vector3.one * 1.4f; // Nảy to ban đầu

        float timer = 0f;
        float duration = 1.4f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Hiệu ứng scale hồi về 1.0 mượt mà
            rect.localScale = Vector3.Lerp(rect.localScale, Vector3.one, Time.deltaTime * 12f);

            // Bay lên trên với tốc độ giảm dần
            float speed = Mathf.Lerp(70f, 15f, progress);
            rect.anchoredPosition += new Vector2(0, speed * Time.deltaTime);

            // Mờ dần ở nửa cuối animation
            float alpha = (progress > 0.4f) ? Mathf.Lerp(1f, 0f, (progress - 0.4f) / 0.6f) : 1f;
            Color c = hpPopupText.color;
            hpPopupText.color = new Color(c.r, c.g, c.b, alpha);

            yield return null;
        }

        rect.localScale = Vector3.one;
        hpPopupText.gameObject.SetActive(false);
    }
}
