using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Cài đặt Đèn pin")]
    [Tooltip("Kéo thả vật thể Spotlight vào ô này")]
    public Light flashlight;
    [Tooltip("Phím dùng để bật/tắt đèn")]
    public KeyCode toggleKey = KeyCode.F; // Mặc định là phím F

    private bool isFlashlightOn = false;

    void Start()
    {
        // Khi mới vào game, tự động tắt đèn pin để người chơi tự bật
        if (flashlight != null)
        {
            flashlight.enabled = false;
            isFlashlightOn = false;
        }
        else
        {
            Debug.LogWarning("Bạn chưa kéo thả Spotlight vào script FlashlightController!");
        }
    }

    void Update()
    {
        // Kiểm tra xem người chơi có bấm phím F không
        if (Input.GetKeyDown(toggleKey))
        {
            if (flashlight != null)
            {
                // Đảo ngược trạng thái (đang bật thành tắt, đang tắt thành bật)
                isFlashlightOn = !isFlashlightOn;

                // Áp dụng trạng thái cho bóng đèn
                flashlight.enabled = isFlashlightOn;
            }
        }
    }
}