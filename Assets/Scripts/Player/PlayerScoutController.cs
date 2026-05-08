using UnityEngine;
using TMPro;

public class PlayerScoutController : MonoBehaviour
{
    [Header("--- HỆ THỐNG TIA MẮT (RAYCAST) ---")]
    public Camera playerCamera;
    public float interactDistance = 4f;
    public TextMeshProUGUI hintText;
    public LayerMask itemLayer;

    [Header("--- HỆ THỐNG SỔ TAY (NOTEBOOK) ---")]
    public GameObject notebookPanel;
    public TextMeshProUGUI txtNippers;     // Kềm
    public TextMeshProUGUI txtNotebook;    // ĐÃ SỬA: Thay Bản đồ (Map) thành Sổ tay (Notebook)
    public TextMeshProUGUI txtRope;        // Dây thừng
    public TextMeshProUGUI txtKey;         // Chìa khóa

    [Header("--- SCRIPT ĐIỀU KHIỂN CHUỘT ---")]
    public MonoBehaviour mouseLookScript;

    private bool isNotebookOpen = false;
    private GameObject currentTarget = null;

    void Start()
    {
        if (notebookPanel != null) notebookPanel.SetActive(false);
        if (hintText != null) hintText.gameObject.SetActive(false);
        UpdateNotebookUI();
    }

    void Update()
    {
        HandleRaycastInteraction();
        HandleNotebookToggle();
    }

    private void HandleRaycastInteraction()
    {
        if (isNotebookOpen) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            GameObject obj = hit.collider.gameObject;

            if (GameManager.instance != null)
            {
                // ĐỒNG BỘ ĐÚNG 4 MÓN: KỀM, SỔ TAY, DÂY THỪNG, CHÌA KHÓA
                if (obj.CompareTag("Item_Nippers") && !GameManager.instance.hasNippers)
                {
                    ShowHint("Bấm [E] để lấy Kềm Cắt Xích", obj);
                }
                else if (obj.CompareTag("Item_Notebook") && !GameManager.instance.hasNotebook)
                {
                    ShowHint("Bấm [E] để lấy Sổ Tay Ghi Chép", obj);
                }
                else if (obj.CompareTag("Item_Rope") && !GameManager.instance.hasRope)
                {
                    ShowHint("Bấm [E] để chôm Dây Thừng", obj);
                }
                else if (obj.CompareTag("Item_Key") && !GameManager.instance.hasKey)
                {
                    ShowHint("Bấm [E] để trộm Chìa Khóa", obj);
                }
                else
                {
                    HideHint();
                }
            }
            else
            {
                HideHint();
            }
        }
        else
        {
            HideHint();
        }

        if (!isNotebookOpen && currentTarget != null && Input.GetKeyDown(KeyCode.E))
        {
            CollectItem(currentTarget);
        }
    }

    private void ShowHint(string message, GameObject target)
    {
        currentTarget = target;
        if (hintText != null)
        {
            hintText.text = message;
            hintText.gameObject.SetActive(true);
        }
    }

    private void HideHint()
    {
        currentTarget = null;
        if (hintText != null) hintText.gameObject.SetActive(false);
    }

    private void CollectItem(GameObject item)
    {
        if (GameManager.instance == null) return;

        // ĐỒNG BỘ 4 MÓN ĐỒ LƯU VÀO GAMEMANAGER
        if (item.CompareTag("Item_Nippers")) GameManager.instance.hasNippers = true;
        else if (item.CompareTag("Item_Notebook")) GameManager.instance.hasNotebook = true;
        else if (item.CompareTag("Item_Rope")) GameManager.instance.hasRope = true;
        else if (item.CompareTag("Item_Key")) GameManager.instance.hasKey = true;

        item.SetActive(false);

        HideHint();
        UpdateNotebookUI();

        Debug.Log("<color=green>Đã lấy vật phẩm thành công!</color>");
    }

    private void HandleNotebookToggle()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isNotebookOpen = !isNotebookOpen;
            if (notebookPanel != null) notebookPanel.SetActive(isNotebookOpen);

            if (isNotebookOpen)
            {
                UpdateNotebookUI();
                if (mouseLookScript != null) mouseLookScript.enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (mouseLookScript != null) mouseLookScript.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void UpdateNotebookUI()
    {
        if (GameManager.instance == null) return;

        if (txtNippers != null)
        {
            if (GameManager.instance.hasNippers) txtNippers.text = "<s>1. Kềm cắt xích (Đã lấy)</s>";
            else txtNippers.text = "1. Tìm Kềm để cắt xích rào";
        }

        // Cập nhật giao diện cho Sổ Tay
        if (txtNotebook != null)
        {
            if (GameManager.instance.hasNotebook) txtNotebook.text = "<s>2. Sổ tay ghi chép (Đã lấy)</s>";
            else txtNotebook.text = "2. Tìm Sổ tay để lên kế hoạch";
        }

        if (txtRope != null)
        {
            if (GameManager.instance.hasRope) txtRope.text = "<s>3. Dây thừng đu tường (Đã chôm)</s>";
            else txtRope.text = "3. Tìm dây thừng ở khu nhà kho";
        }

        if (txtKey != null)
        {
            if (GameManager.instance.hasKey) txtKey.text = "<s>4. Chìa khóa cổng chính (Đã chôm)</s>";
            else txtKey.text = "4. Trộm chìa khóa của quản lý";
        }
    }
}