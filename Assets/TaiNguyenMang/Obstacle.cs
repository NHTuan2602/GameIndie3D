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
                    if (AudioManager.instance != null) AudioManager.instance.PlayHit();
                    PursuitManager.instance.GameOver("TỬ VONG DO VA CHẠM MẠNH!");
                    break;

                // 2. VẤP Ổ GÀ
                case ObstacleType.Pothole:
                    if (AudioManager.instance != null) AudioManager.instance.PlayPothole();
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
                    if (AudioManager.instance != null) AudioManager.instance.PlayJump();
                    bike.forwardSpeed += speedChange; // Cộng tốc độ (Hàm Update bên kia sẽ tự hãm lại dần)
                    // Không Destroy dốc để người chơi thấy mình đang bay trên không
                    break;

                // 4. NHẶT GẠCH / VẬT PHẨM
                case ObstacleType.ItemPickup:
                    // Đã tách biệt hoàn toàn, không bao giờ phát tiếng tông xe ở đây nữa
                    PursuitManager.instance.UseItemImmediately();
                    Destroy(gameObject);
                    break;
            }
        }
    }
}