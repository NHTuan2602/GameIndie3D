using UnityEngine;

public class NightActivator : MonoBehaviour
{
    public int spawnOnDay = 2;

    void Start()
    {
        // 1. Nếu CHƯA TỚI NGÀY thì tuyệt đối không hiện
        if (GameManager.instance.currentDay < spawnOnDay)
        {
            gameObject.SetActive(false);
            return;
        }

        // 2. Nếu ĐÃ ĐẾN NGÀY (hoặc qua ngày rồi), phải hỏi xem người chơi đã có đồ chưa?
        // Ta sẽ "mượn" luôn thông tin ID từ script InteractableItem gắn trên cùng vật thể
        InteractableItem interactScript = GetComponent<InteractableItem>();

        if (interactScript != null && interactScript.data != null)
        {
            string safeItemID = interactScript.data.itemID.Trim().ToLower();
            bool alreadyHasItem = false;

            // Dò xem GameManager đang báo là có hay chưa
            if (safeItemID == "notebook") alreadyHasItem = GameManager.instance.hasNotebook;
            else if (safeItemID == "nippers") alreadyHasItem = GameManager.instance.hasNippers;
            else if (safeItemID == "rope") alreadyHasItem = GameManager.instance.hasRope;
            else if (safeItemID == "key") alreadyHasItem = GameManager.instance.hasKey;

            // 3. QUYẾT ĐỊNH SINH TỬ:
            if (alreadyHasItem)
            {
                // Đã có trong người (do nhặt hôm trước) -> Tàng hình
                gameObject.SetActive(false);
            }
            else
            {
                // Chưa có (do chưa nhặt HOẶC VỪA BỊ TỊCH THU) -> Hiện lên cho nhặt lại!
                gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("<color=red>Vật thể này có NightActivator nhưng thiếu InteractableItem hoặc thiếu ItemData!</color>");
        }
    }
}