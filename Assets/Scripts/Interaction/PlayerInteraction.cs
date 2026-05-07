using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Cài đặt Tương tác")]
    public float interactRange = 3f;
    public Transform playerCamera;
    public TextMeshProUGUI interactUI;

    void Update()
    {
        // Nếu túi đồ đang mở thì không cho tương tác
        if (InventoryManager.Instance != null && InventoryManager.Instance.isInventoryOpen)
        {
            interactUI.gameObject.SetActive(false);
            return;
        }

        interactUI.gameObject.SetActive(false);
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            // 1. KIỂM TRA XEM CÓ PHẢI VẬT PHẨM ĐỂ NHẶT KHÔNG?
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();
            if (item != null)
            {
                interactUI.gameObject.SetActive(true);
                interactUI.text = "[E] Nhặt " + item.data.itemName;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    item.Interact();
                    interactUI.gameObject.SetActive(false);
                }
                return; // Xử lý nhặt đồ xong thì thoát, khỏi kiểm tra tiếp
            }

            // 2. KIỂM TRA XEM CÓ PHẢI LÀ CÁNH CỬA CẦN MỞ KHÔNG?
            // (Chỉ áp dụng cho AutoDoor, không áp dụng cho cửa ngủ CasinoDoor)
            AutoDoorController autoDoor = hit.collider.GetComponentInParent<AutoDoorController>();
            if (autoDoor != null && !autoDoor.isOpen)
            {
                interactUI.gameObject.SetActive(true);
                interactUI.text = "[E] Mở cửa"; // Hoặc [F] tùy bạn đổi chữ

                if (Input.GetKeyDown(KeyCode.E)) // Nếu muốn phím F thì đổi thành KeyCode.F
                {
                    autoDoor.OpenDoor();
                    interactUI.gameObject.SetActive(false);
                }
            }
        }
    }
}