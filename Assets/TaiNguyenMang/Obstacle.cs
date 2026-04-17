using UnityEngine;

public enum ObstacleType { InstantDeath, Pothole, Ramp, ItemPickup }

public class Obstacle : MonoBehaviour
{
    public ObstacleType type;
    public float speedChange = 10f; // Số lượng tốc độ thay đổi

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EscapeBikeController bike = other.GetComponent<EscapeBikeController>();

            switch (type)
            {
                case ObstacleType.InstantDeath:
                    PursuitManager.instance.GameOver("TỬ VONG DO VA CHẠM MẠNH!");
                    break;

                case ObstacleType.Pothole:
                    // TRỪ TỐC ĐỘ VĨNH VIỄN
                    bike.forwardSpeed -= speedChange;
                    Debug.Log("Vấp ổ gà! Tốc độ còn: " + bike.forwardSpeed);
                    Destroy(gameObject); // Biến mất sau khi đụng
                    break;

                case ObstacleType.Ramp:
                    // TĂNG TỐC ĐỘ
                    bike.forwardSpeed += speedChange;
                    /*Destroy(gameObject);*/
                    break;

                case ObstacleType.ItemPickup:
                    // NHẶT ĐỒ VÀ NÉM NGAY LẬP TỨC (Đã sửa lỗi ở đây!)
                    PursuitManager.instance.UseItemImmediately();
                    Destroy(gameObject);
                    break;
            }
        }
    }
}