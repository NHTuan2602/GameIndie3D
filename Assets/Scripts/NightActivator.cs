using UnityEngine;

public class NightActivator : MonoBehaviour
{
    public int spawnOnDay = 2; // Đổi tên biến cho chuẩn: Món đồ này hiện ở NGÀY (ĐÊM) mấy?

    void Start()
    {
        // ĐÃ SỬA: Dùng chữ "i" thường và gọi đúng biến "currentDay" của bạn
        if (GameManager.instance.currentDay != spawnOnDay)
        {
            gameObject.SetActive(false);
        }
    }
}