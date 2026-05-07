using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    public ItemData data;

    public void Interact()
    {
        string safeItemID = data.itemID.Trim().ToLower();

        // 1. Kiểm tra quyền nhặt đồ
        if (!GameManager.instance.CanCollectItems() && safeItemID != "notebook")
        {
            Debug.Log("Bạn cần tìm cuốn sổ tay trước khi thu thập vật phẩm này!");
            return;
        }

        // 2. Nhận diện nhặt sổ
        if (safeItemID == "notebook")
        {
            GameManager.instance.hasNotebook = true;
        }

        // 3. Tống vào túi đồ mới và xóa khỏi map
        InventoryManager.Instance.AddItem(data);
        Destroy(gameObject);
    }
}