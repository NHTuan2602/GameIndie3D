using UnityEngine;
using System.Collections.Generic;

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance;

    // Danh sách các ID vật phẩm đã thu thập
    public List<string> collectedItems = new List<string>();

    void Awake() { Instance = this; }

    public void AddItem(string itemId)
    {
        if (!collectedItems.Contains(itemId))
        {
            collectedItems.Add(itemId);
            // Sau khi thêm, gọi UI cập nhật lại
            FindObjectOfType<ChecklistUI>().UpdateUI();
        }
    }
}