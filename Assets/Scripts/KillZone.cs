using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng tông vào tường có phải là người chơi không
        if (other.CompareTag("Player"))
        {
            // Tìm script Xe Đạp và gọi hàm rơi
            EscapeBikeController bike = other.GetComponent<EscapeBikeController>();
            if (bike != null)
            {
                bike.TriggerFallDeath();
            }
        }
    }
}