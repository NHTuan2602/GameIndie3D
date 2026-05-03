using UnityEngine;

public enum ObstacleType { InstantDeath, Pothole, Ramp, ItemPickup }

public class Obstacle : MonoBehaviour
{
    public ObstacleType type;
    public float speedChange = 10f; // Dùng chung: dốc cộng 10, ổ gà trừ 10

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ kích hoạt khi người chơi đụng vào
        if (other.CompareTag("Player"))
        {
            EscapeBikeController bike = other.GetComponent<EscapeBikeController>();

            switch (type)
            {
                // 1. TÔNG XE ĐỊCH
                case ObstacleType.InstantDeath:
                    // ĐÃ FIX: Chuyển sang gọi BikeAudioManager
                    if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayHit();
                    PursuitManager.instance.GameOver("TỬ VONG DO VA CHẠM MẠNH!");
                    break;

                // 2. VẤP Ổ GÀ
                case ObstacleType.Pothole:
                    // ĐÃ FIX: Chuyển sang gọi BikeAudioManager
                    if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayPothole();
                    bike.forwardSpeed -= speedChange; // Trừ tốc độ

                    // CHỐT CHẶN: Nếu trừ xong mà tốc độ tụt xuống 15 thì bắt luôn
                    if (bike.forwardSpeed <= 15f)
                    {
                        PursuitManager.instance.GameOver("BỊ BẮT DO CHẠY QUÁ CHẬM!");
                    }
                    else
                    {
                        Debug.Log("Vấp ổ gà! Tốc độ còn: " + bike.forwardSpeed);
                    }
                    Destroy(gameObject);
                    break;

                // 3. BAY LÊN DỐC
                case ObstacleType.Ramp:
                    // ĐÃ FIX: Chuyển sang gọi BikeAudioManager
                    if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayJump();
                    bike.forwardSpeed += speedChange; // Cộng tốc độ
                    break;

                // 4. NHẶT GẠCH / VẬT PHẨM
                case ObstacleType.ItemPickup:
                    // Đã tách biệt hoàn toàn trong PursuitManager
                    PursuitManager.instance.UseItemImmediately();
                    Destroy(gameObject);
                    break;
            }
        }
    }
}