using UnityEngine;
using UnityEngine.UI;
using TMPro; // Bắt buộc dùng để xài TextMeshPro

public class ChecklistUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject notebookPanel;
    public GameObject itemTemplate;
    public Transform contentParent;

    void Start()
    {
        // Ẩn sổ tay khi mới vào game
        if (notebookPanel != null) notebookPanel.SetActive(false);
    }

    void Update()
    {
        // Nhấn phím Tab để bật/tắt sổ tay
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleNotebook();
        }
    }

    public void ToggleNotebook()
    {
        bool isActive = !notebookPanel.activeSelf;
        notebookPanel.SetActive(isActive);

        if (isActive)
        {
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        // 1. Xóa toàn bộ các dòng cũ đang hiển thị (ngoại trừ cái khuôn gốc)
        foreach (Transform child in contentParent)
        {
            if (child.gameObject != itemTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        // 2. Vẽ lại toàn bộ danh sách từ Manager
        foreach (string itemName in ChecklistManager.Instance.collectedItems)
        {
            // Nhân bản cái khuôn
            GameObject newLine = Instantiate(itemTemplate, contentParent);
            newLine.SetActive(true); // Bật nó lên

            // Tìm TextMeshPro bên trong và gạch đầu dòng tên vật phẩm
            TextMeshProUGUI textComp = newLine.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = "- " + itemName;
            }
        }
    }
}