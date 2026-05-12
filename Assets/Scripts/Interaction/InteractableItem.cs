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

        // 2. KIỂM TRA ID VÀ LƯU VÀO GAME MANAGER + Ổ CỨNG (PLAYERPREFS)
        if (safeItemID == "notebook")
        {
            GameManager.instance.hasNotebook = true;
            PlayerPrefs.SetInt("HasNotebook", 1);
        }
        else if (safeItemID == "nippers") // Chú ý: Đảm bảo itemID của cái kềm là "nippers"
        {
            GameManager.instance.hasNippers = true;
            PlayerPrefs.SetInt("HasNippers", 1);
        }
        else if (safeItemID == "rope") // Dây thừng
        {
            GameManager.instance.hasRope = true;
            PlayerPrefs.SetInt("HasRope", 1);
        }
        else if (safeItemID == "key") // Chìa khóa
        {
            GameManager.instance.hasKey = true;
            PlayerPrefs.SetInt("HasKey", 1);
        }

        // Chốt sổ lưu vào máy tính!
        PlayerPrefs.Save();

        // 3. Tống vào túi đồ UI và xóa khỏi map
        InventoryManager.Instance.AddItem(data);
        Destroy(gameObject);
    }
}