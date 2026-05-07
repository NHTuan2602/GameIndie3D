using UnityEngine;
using TMPro;

public class BedInteraction : MonoBehaviour
{
    private bool isPlayerNear = false;
    private bool isSleeping = false; // Chặn spam phím E

    [Header("Cài đặt")]
    public string notebookItemID = "notebook";
    public TextMeshProUGUI interactPromptText;

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có đúng là Player chạm vào không
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
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
            if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !isSleeping)
        {
            bool hasNotebook = false;
            if (InventoryManager.Instance != null)
                hasNotebook = InventoryManager.Instance.HasItem(notebookItemID);

            if (hasNotebook)
            {
                isSleeping = true; // Khóa phím
                if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);

                if (GameManager.instance != null)
                {
                    GameManager.instance.hasNotebook = true;
                    GameManager.instance.SleepThroughNight();
                }
            }
            else
            {
                if (interactPromptText != null)
                    interactPromptText.text = "<color=red>Cần có Sổ tay để ghi chép trước khi ngủ!</color>";
            }
        }
    }
}