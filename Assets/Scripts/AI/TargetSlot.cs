using UnityEngine;

public class TargetSlot : MonoBehaviour
{
    [Header("Loại hàng hóa được phép đặt vào đây")]
    [Tooltip("Gõ chính xác tên loại hàng. VD: Coca hoặc Snack")]
    public string acceptedType;

    [Header("Trạng thái")]
    public bool isOccupied = false; // Đã có món đồ nào đặt vào chưa?
}