using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class DraggableBowl : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 startPosition;
    public bool isDraggable = false;
    public bool hasRevealed = false; // Đổi thành public để Manager kiểm tra
    private Canvas canvas;
    private RectTransform rectTransform;

    [Header("Khoảng cách mở bát")]
    public float revealThreshold = 200f;

    [HideInInspector] public TaiXiuManager manager;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        startPosition = rectTransform.anchoredPosition;
    }

    public void ResetPosition()
    {
        rectTransform.anchoredPosition = startPosition;
        isDraggable = false;
        hasRevealed = false;
        gameObject.SetActive(true); // Đảm bảo bát luôn hiện lại đầu ván
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        if (!isDraggable) return;

        // Vẫn cho phép kéo mượt mà dù đã qua vạch đích
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // ĐO KHOẢNG CÁCH: Nếu vượt mốc lần đầu tiên -> Chốt kết quả (nhưng KHÔNG khóa kéo)
        if (!hasRevealed)
        {
            float distance = Vector2.Distance(rectTransform.anchoredPosition, startPosition);
            if (distance >= revealThreshold)
            {
                hasRevealed = true;
                if (manager != null) manager.CheckResult();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // HIỆU ỨNG DÂY THUN: Kéo chưa đủ xa mà lười thả ra -> Bay về úp lại
        if (!hasRevealed)
        {
            rectTransform.anchoredPosition = startPosition;
        }
        else
        {
            // Đã mở bát thành công rồi thả tay -> Bát nằm ngoan ngoãn tại đó
            isDraggable = false;
        }
    }
}