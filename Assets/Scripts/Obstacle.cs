using UnityEngine;
using System.Collections; // Bắt buộc phải có để dùng Coroutine

public enum ObstacleType { InstantDeath, Pothole, Ramp }

public class Obstacle : MonoBehaviour
{
    public ObstacleType type;
    public float speedModifier = 15f; // Số này dùng chung: Ổ gà trừ 15, Dốc cộng 15
    public float effectDuration = 2f; // Thời gian tác dụng (2 giây)

    private bool isTriggered = false; // Công tắc chống va chạm đúp

    private void OnTriggerEnter(Collider other)
    {
        // Nếu đã đụng rồi thì bỏ qua, tránh lỗi trừ máu/tốc độ 2 lần
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            isTriggered = true;
            EscapeBikeController bike = other.GetComponent<EscapeBikeController>();

            switch (type)
            {
                case ObstacleType.InstantDeath:
                    // Gọi Singleton cực nhanh, không gây lag
                    PursuitManager.instance.GameOver("TỬ VONG DO VA CHẠM XE NGƯỢC CHIỀU!");
                    break;

                case ObstacleType.Pothole:
                    StartCoroutine(HandleSpeedEffect(bike, -speedModifier));
                    break;

                case ObstacleType.Ramp:
                    StartCoroutine(HandleSpeedEffect(bike, speedModifier));
                    break;
            }
        }
    }

    // Bộ đếm thời gian độc lập để trả lại tốc độ cũ
    IEnumerator HandleSpeedEffect(EscapeBikeController bike, float amount)
    {
        // 1. Tác dụng lên xe (Cộng/Trừ)
        bike.forwardSpeed += amount;

        // 2. Ẩn vật cản đi ngay lập tức (Để nhìn có vẻ như xe đã chèn qua nó)
        if (GetComponent<MeshRenderer>()) GetComponent<MeshRenderer>().enabled = false;
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;

        // 3. Đợi đúng 2 giây
        yield return new WaitForSeconds(effectDuration);

        // 4. Trả lại tốc độ cũ cho xe
        if (bike != null) bike.forwardSpeed -= amount;

        // 5. Xóa vật cản khỏi bộ nhớ cho nhẹ máy
        Destroy(gameObject);
    }
}