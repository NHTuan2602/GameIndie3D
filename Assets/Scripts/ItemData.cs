using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "StealthGame/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;           // CẦN THIẾT: Mã định danh (VD: "notebook", "day_thung")
    public string itemName;         // Tên hiển thị (VD: "Sổ tay lính canh")
    [TextArea] public string description; // Mô tả manh mối
}