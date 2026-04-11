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
        // Nếu túi đồ đang mở thì không cho tương tác nhặt đồ
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
            }
        }
    }
}