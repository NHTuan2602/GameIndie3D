using UnityEngine;
using UnityEngine.UI; // Bắt buộc thêm dòng này để dùng UI Image

public class CrosshairController : MonoBehaviour
{
    [Header("Giao diện")]
    // Kéo và thả cái CrosshairImage ở Hierarchy vào ô này
    [SerializeField] GameObject crosshairObject;

    [Header("Cài đặt")]
    [Tooltip("Tâm ngắm có hiện ra khi mới vào màn hình không?")]
    public bool showOnStart = false;

    void Start()
    {
        // Khi mới vào, đặt trạng thái cho cái tâm ngắm
        if (crosshairObject != null)
        {
            SetCrosshairActive(showOnStart);
        }
    }

    // Hàm để Hiện cái tâm ngắm lên
    public void Show()
    {
        if (crosshairObject != null)
        {
            SetCrosshairActive(true);
        }
    }

    // Hàm để Ẩn cái tâm ngắm đi
    public void Hide()
    {
        if (crosshairObject != null)
        {
            SetCrosshairActive(false);
        }
    }

    // Hàm để Bật/Tắt cái tâm ngắm (Toggle)
    public void Toggle()
    {
        if (crosshairObject != null)
        {
            SetCrosshairActive(!crosshairObject.activeSelf);
        }
    }

    // Hàm hỗ trợ để bật tắt component Image (hoặc GameObject)
    private void SetCrosshairActive(bool active)
    {
        // Cách tốt nhất là bật tắt component Image để tránh lỗi khi GameObject bị tắt
        Image img = crosshairObject.GetComponent<Image>();
        if (img != null)
        {
            img.enabled = active;
        }
        else
        {
            // Nếu không tìm thấy component Image, ta bật tắt cả GameObject
            crosshairObject.SetActive(active);
        }
    }
}