using UnityEngine;
using TMPro;

public class LockedCabinetInteraction : MonoBehaviour
{
    private bool isPlayerNear = false;
    private bool isOpened = false;

    [Header("Cài đặt Điều kiện & Phần thưởng")]
    [Tooltip("Nhập ID của cái Kìm (phải khớp với ID bạn cài trong ItemData của cái Kìm)")]
    public string requiredItemID = "kiem";
    [Tooltip("Kéo file ItemData của CHÌA KHÓA vào đây để nó rớt vào túi")]
    public ItemData rewardKeyData;

    [Header("Giao diện & Hình ảnh")]
    public TextMeshProUGUI interactPromptText;
    public GameObject lockVisual; // Kéo object cái ổ khóa vào đây

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            isPlayerNear = true;
            if (interactPromptText != null)
            {
                interactPromptText.text = "Bấm [E] để cắt khóa tủ";
                interactPromptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            bool hasPliers = false;

            // Kiểm tra xem trong túi có kìm không
            if (InventoryManager.Instance != null)
            {
                hasPliers = InventoryManager.Instance.HasItem(requiredItemID);
            }

            if (hasPliers)
            {
                // THÀNH CÔNG: CẮT KHÓA
                isOpened = true;

                // 1. Làm biến mất cái ổ khóa để báo hiệu đã cắt
                if (lockVisual != null) lockVisual.SetActive(false);

                // 2. Tắt dòng chữ hiển thị
                if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);

                // 3. Trao Chìa Khóa cho người chơi
                if (InventoryManager.Instance != null && rewardKeyData != null)
                {
                    InventoryManager.Instance.AddItem(rewardKeyData);
                    Debug.Log("<color=green>Đã cắt khóa thành công! Bỏ túi Chìa Khóa.</color>");
                }
            }
            else
            {
                // THẤT BẠI: KHÔNG CÓ KÌM
                if (interactPromptText != null)
                {
                    interactPromptText.text = "<color=red>Khóa rất chắc! Cần có Kìm để cắt.</color>";
                }
            }
        }
    }
}