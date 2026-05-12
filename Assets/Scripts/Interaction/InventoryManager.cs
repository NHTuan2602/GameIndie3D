using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI Túi Đồ")]
    public GameObject inventoryPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemCounterText;

    [Header("Dữ liệu gốc (Kéo file ItemData vào đây)")]
    public ItemData notebookData;
    public ItemData nippersData;
    public ItemData ropeData;
    public ItemData keyData;

    private List<ItemData> collectedItems = new List<ItemData>();
    private int currentIndex = 0;

    [HideInInspector]
    public bool isInventoryOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        inventoryPanel.SetActive(false);

        // PHỤC HỒI TÚI ĐỒ TỪ GAME MANAGER KHI VỪA VÀO MÀN MỚI
        if (GameManager.instance != null)
        {
            if (GameManager.instance.hasNotebook && notebookData != null) AddItem(notebookData);
            if (GameManager.instance.hasNippers && nippersData != null) AddItem(nippersData);
            if (GameManager.instance.hasRope && ropeData != null) AddItem(ropeData);
            if (GameManager.instance.hasKey && keyData != null) AddItem(keyData);
        }
    }

    void Update()
    {
        // Bấm Tab để Bật/Tắt túi đồ
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        // Dùng phím A và D để chuyển đồ khi túi đang mở
        if (isInventoryOpen)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) Navigate(-1);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) Navigate(1);
        }
    }

    // Đã gộp và xóa hàm trùng lặp
    public void AddItem(ItemData newItem)
    {
        // Kiểm tra xem trong túi đã có món này chưa để tránh bị đúp 2 món giống nhau
        if (!collectedItems.Contains(newItem))
        {
            collectedItems.Add(newItem);
            Debug.Log("<color=green>Đã bỏ vào túi: " + newItem.itemName + "</color>");
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            Time.timeScale = 0f; // ĐÓNG BĂNG THỜI GIAN
            Cursor.lockState = CursorLockMode.None; // Mở khóa chuột
            Cursor.visible = true;
            UpdateUI();
        }
        else
        {
            Time.timeScale = 1f; // TIẾP TỤC TRÒ CHƠI
            Cursor.lockState = CursorLockMode.Locked; // Khóa chuột lại
            Cursor.visible = false;
        }
    }

    public void Navigate(int direction)
    {
        if (collectedItems.Count == 0) return;

        currentIndex += direction;

        // Vòng lặp: Qua trái món đầu thì nhảy xuống món cuối
        if (currentIndex < 0) currentIndex = collectedItems.Count - 1;
        if (currentIndex >= collectedItems.Count) currentIndex = 0;

        UpdateUI();
    }

    void UpdateUI()
    {
        if (collectedItems.Count == 0)
        {
            itemNameText.text = "Túi Đồ Trống";
            itemDescriptionText.text = "Bạn chưa nhặt bất kỳ vật phẩm nào. Hãy tìm kiếm xung quanh.";
            itemCounterText.text = "0 / 0";
        }
        else
        {
            ItemData currentItem = collectedItems[currentIndex];
            itemNameText.text = currentItem.itemName;
            itemDescriptionText.text = currentItem.description;
            itemCounterText.text = (currentIndex + 1) + " / " + collectedItems.Count;
        }
    }

    public bool HasItem(string searchItemID)
    {
        foreach (ItemData item in collectedItems)
        {
            if (item.itemID == searchItemID) return true;
        }
        return false;
    }
}