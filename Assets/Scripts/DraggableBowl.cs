using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class DraggableBowl : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 startPosition;
    public bool isDraggable = false;
    private bool hasRevealed = false;
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
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable || hasRevealed) return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable || hasRevealed) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        float distance = Vector2.Distance(rectTransform.anchoredPosition, startPosition);
        if (distance >= revealThreshold)
        {
            hasRevealed = true;
            isDraggable = false;

            if (manager != null)
            {
                manager.CheckResult();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData) { }
}