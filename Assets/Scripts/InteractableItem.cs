using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    public ItemData data;
    public bool isInspectOnly = false; // Nếu tích vào đây thì chỉ ấn F để xem, không nhặt

    public void Interact(bool isCollecting) // true = E (Nhặt), false = F (Xem)
    {
        if (isInspectOnly)
        {
            if (!isCollecting) // Người chơi ấn F
            {
                Debug.Log("Ghi nhớ manh mối: " + data.description);
                // ChecklistManager xài chữ I hoa
                ChecklistManager.Instance.AddItem(data.itemName + " (Manh mối)");
            }
        }
        else
        {
            if (isCollecting) // Người chơi ấn E
            {
                // ĐÃ SỬA LỖI: Dùng GameManager.instance (chữ i thường)
                if (!GameManager.instance.CanCollectItems() && data.itemID != "notebook")
                {
                    Debug.Log("Bạn cần tìm cuốn sổ tay trước khi thu thập vật phẩm này!");
                    return;
                }

                // Nếu nhặt cuốn sổ thì báo cho GameManager biết
                if (data.itemID == "notebook")
                {
                    GameManager.instance.hasNotebook = true;
                }

                ChecklistManager.Instance.AddItem(data.itemName);
                Destroy(gameObject);
            }
        }
    }
}