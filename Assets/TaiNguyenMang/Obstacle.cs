using UnityEngine;

public enum ObstacleType { InstantDeath, Pothole, Ramp, ItemPickup }

public class Obstacle : MonoBehaviour
{
    public ObstacleType type;
    public float speedChange = 10f; // Dùng chung: dốc cộng 10, ổ gà trừ 10

    private void OnTriggerEnter(Collider other)
    {
        // GÓC KHUẤT 1 FIX: Kiểm tra tag trên cả object va chạm trực tiếp VÀ object cha ngoài cùng (root).
        // Đảm bảo không bị miss nếu collider va chạm nằm ở object con (như bánh xe) không được gắn tag.
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            // Lấy component từ Parent/Root thay vì chính object đó.
            EscapeBikeController bike = other.GetComponentInParent<EscapeBikeController>();

            // Nếu vì lý do nào đó không tìm thấy script, thoát luôn để tránh gãy code báo lỗi Null.
            if (bike == null)
            {
                Debug.LogWarning("Va chạm với Player nhưng không tìm thấy EscapeBikeController!");
                return;
            }

            switch (type)
            {
                // 1. TÔNG XE ĐỊCH
                case ObstacleType.InstantDeath:
                    if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayHit();

                    // GÓC KHUẤT 4 FIX: Bọc null check cho Manager để tránh Silent Error
                    if (PursuitManager.instance != null)
                    {
                        PursuitManager.instance.GameOver("TỬ VONG DO VA CHẠM MẠNH!");
                    }
                    else
                    {
                        Debug.LogError("LỖI NGHIÊM TRỌNG: PursuitManager.instance bị NULL!");
                    }
                    break;

                // 2. VẤP Ổ GÀ
                case ObstacleType.Pothole:
                    if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayPothole();
                    bike.forwardSpeed -= speedChange; // Trừ tốc độ

                    // CHỐT CHẶN: Nếu trừ xong mà tốc độ tụt xuống 15 thì bắt luôn
                    if (bike.forwardSpeed <= 15f)
                    {
                        if (PursuitManager.instance != null)
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
                    if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayJump();
                    bike.forwardSpeed += speedChange; // Cộng tốc độ
                    break;

                // 4. NHẶT GẠCH / VẬT PHẨM
                case ObstacleType.ItemPickup:
                    if (PursuitManager.instance != null)
                        PursuitManager.instance.UseItemImmediately();
                    Destroy(gameObject);
                    break;
            }
        }
    }
}