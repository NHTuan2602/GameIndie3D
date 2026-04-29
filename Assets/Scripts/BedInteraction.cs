using UnityEngine;
using TMPro; // BẮT BUỘC CÓ DÒNG NÀY ĐỂ DÙNG UI TEXT

public class BedInteraction : MonoBehaviour
{
    private bool isPlayerNear = false;

    [Header("Cài đặt Vật phẩm")]
    [Tooltip("Gõ chính xác Item ID của cuốn sổ vào đây")]
    public string notebookItemID = "notebook";

    [Header("Cài đặt Giao Diện (UI)")]
    [Tooltip("Kéo chữ hiển thị trên màn hình vào đây")]
    public TextMeshProUGUI interactPromptText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            // 1. Hiển thị chữ lên màn hình khi lại gần giường
            if (interactPromptText != null)
            {
                interactPromptText.text = "Bấm [E] để ngủ qua đêm";
                interactPromptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;

            // 2. Giấu chữ đi khi bỏ đi xa khỏi giường
            if (interactPromptText != null)
            {
                interactPromptText.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            bool hasNotebookInBag = false;
            if (InventoryManager.Instance != null)
            {
                hasNotebookInBag = InventoryManager.Instance.HasItem(notebookItemID);
            }

            if (hasNotebookInBag)
            {
                // Tắt dòng chữ thông báo đi trước khi chuyển màn hình đen
                if (interactPromptText != null)
                {
                    interactPromptText.gameObject.SetActive(false);
                }

                if (GameManager.instance != null)
                {
                    GameManager.instance.hasNotebook = true;
                    GameManager.instance.SleepThroughNight();
                }
            }
            else
            {
                // 3. Nếu chưa có sổ, đổi chữ trên màn hình thành màu ĐỎ để cảnh báo!
                if (interactPromptText != null)
                {
                    interactPromptText.text = "<color=red>Chưa có sổ! Không thể ngủ!</color>";
                }
            }
        }
    }
}