using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // GÓC KHUẤT 3: Tìm script EscapeBikeController ở object đụng trúng 
        // HOẶC tìm ngược lên tất cả các cục cha của nó.
        EscapeBikeController bike = other.GetComponentInParent<EscapeBikeController>();

        if (bike != null)
        {
            Debug.Log("<color=magenta>HỆ THỐNG BÁO: XE ĐÃ TÔNG TRÚNG BẪY TÀNG HÌNH!</color>");
            bike.TriggerFallDeath();
        }
    }
}   