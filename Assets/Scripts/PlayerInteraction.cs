using UnityEngine;
using TMPro; // Dùng để can thiệp vào TextMeshPro

public class PlayerInteraction : MonoBehaviour
{
    [Header("Cài đặt Tương tác")]
    public float interactRange = 3f; // Tầm với cánh tay (3 mét)
    public Transform playerCamera;   // Kéo Main Camera vào đây
    public TextMeshProUGUI interactUI; // Kéo cái InteractPromptText vào đây

    void Update()
    {
        // BƯỚC 1: Mặc định tắt dòng chữ báo hiệu
        interactUI.gameObject.SetActive(false);

        // BƯỚC 2: Bắn một tia laser tàng hình từ giữa màn hình ra phía trước
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        // Nếu tia đụng trúng một vật gì đó trong tầm tay
        if (Physics.Raycast(ray, out hit, interactRange))
        {
            // Kiểm tra xem vật đó có script InteractableItem (có thể tương tác) không
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();

            if (item != null)
            {
                // Bật UI lên
                interactUI.gameObject.SetActive(true);

                // Nếu là đồ chỉ được XEM (như tờ giấy, vết máu)
                if (item.isInspectOnly)
                {
                    interactUI.text = "[F] Ghi nhớ " + item.data.itemName;

                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        item.Interact(false); // false = Ấn F
                        // Tùy chọn: Tắt luôn UI sau khi ghi nhớ
                        interactUI.gameObject.SetActive(false);
                    }
                }
                // Nếu là đồ có thể NHẶT BỎ TÚI (như Sổ tay, Kìm)
                else
                {
                    interactUI.text = "[E] Nhặt " + item.data.itemName;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        item.Interact(true); // true = Ấn E
                    }
                }
            }
        }
    }
}