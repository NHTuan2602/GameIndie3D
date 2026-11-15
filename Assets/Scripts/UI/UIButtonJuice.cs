using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Tự động thêm hiệu ứng nảy (Juice), chuyển động mượt và âm thanh cho các Nút bấm UI.
/// Hoạt động ngay cả khi tạm dừng game (TimeScale = 0).
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hiệu ứng Co giãn (Scale Juice)")]
    public float hoverScale = 1.05f;
    public float pressedScale = 0.95f;
    public float animationSpeed = 16f;

    [Header("Hiệu ứng Màu viền")]
    public bool enableOutlineGlow = true;
    public Color hoverOutlineColor = new Color(0.95f, 0.4f, 0.2f, 0.9f);

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Button button;
    private Outline outline;
    private Color originalOutlineColor;

    void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
        targetScale = originalScale;

        // Tự động kiểm tra hoặc thêm Outline mềm nếu muốn tạo hiệu ứng viền sáng
        if (enableOutlineGlow)
        {
            outline = GetComponent<Outline>();
            if (outline != null)
            {
                originalOutlineColor = outline.effectColor;
            }
        }
    }

    void OnEnable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Sử dụng unscaledDeltaTime để chạy mượt ngay cả khi Pause Menu làm TimeScale = 0
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetScale = originalScale * hoverScale;

        if (outline != null)
        {
            outline.effectColor = hoverOutlineColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;

        if (outline != null)
        {
            outline.effectColor = originalOutlineColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetScale = originalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetScale = originalScale * hoverScale;
    }

    /// <summary>
    /// Hàm tiện ích tự động gắn UIButtonJuice cho tất cả Button con trong GameObject này.
    /// </summary>
    public static void EnhanceAllButtonsIn(GameObject root)
    {
        if (root == null) return;
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            if (b.GetComponent<UIButtonJuice>() == null)
            {
                b.gameObject.AddComponent<UIButtonJuice>();
            }
        }
    }
}
