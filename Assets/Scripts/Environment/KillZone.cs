using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EscapeBikeController bike = other.GetComponentInParent<EscapeBikeController>();

        if (bike != null)
        {
            Debug.Log("<color=magenta>HỆ THỐNG BÁO: XE ĐÃ TÔNG TRÚNG BẪY TÀNG HÌNH!</color>");

            // Tắt điều khiển và cho vật lý rơi tự do
            bike.TriggerFallDeath();

            // GÓC KHUẤT FIX: Gọi hàm FallGameOver riêng biệt, không dùng chung hàm đâm xe nữa
            if (PursuitManager.instance != null)
            {
                PursuitManager.instance.FallGameOver("BẠN ĐÃ RỚT KHỎI BẢN ĐỒ!");
            }
        }
    }
}