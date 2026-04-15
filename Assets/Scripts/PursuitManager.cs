using UnityEngine;
using TMPro; // Cần thư viện này nếu bạn dùng UI TextMeshPro

public class PursuitManager : MonoBehaviour
{
    public static PursuitManager instance; // Kỹ thuật Singleton (Gọi cực nhanh từ mọi nơi)

    [Header("Chỉ số Truy đuổi")]
    public float pursuerDistance = 50f;
    public float catchDistance = 5f;
    public float maxDistance = 100f; // Địch không bao giờ bị bỏ xa quá 100m
    public float basePursuerSpeed = 30f;

    [Header("Kết nối")]
    public EscapeBikeController player;
    public TextMeshProUGUI distanceUI; // Kéo Text UI hiển thị khoảng cách vào đây

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // 1. Tính toán khoảng cách
        float speedDelta = basePursuerSpeed - player.forwardSpeed;
        pursuerDistance -= speedDelta * Time.deltaTime;

        // 2. Khóa khoảng cách không cho vượt quá Max (Tránh lỗi toán học)
        pursuerDistance = Mathf.Clamp(pursuerDistance, 0, maxDistance);

        // 3. Cập nhật UI cho người chơi thấy (Rất quan trọng để tạo áp lực)
        if (distanceUI != null)
        {
            distanceUI.text = $"Địch cách: {pursuerDistance:F1}m";
        }

        // 4. Kiểm tra Game Over
        if (pursuerDistance <= catchDistance)
        {
            GameOver("BẠN ĐÃ BỊ BỌN TRUY ĐUỔI TÚM GỌN!");
        }
    }

    public void GameOver(string reason)
    {
        Debug.Log("<color=red>" + reason + "</color>");
        // Tắt code xe đạp để xe dừng lại
        if (player != null) player.enabled = false;
        Time.timeScale = 0; // Dừng thời gian toàn game

        // GỌI HIỂN THỊ UI GAME OVER TẠI ĐÂY LÀ ĐẸP NHẤT
    }
}