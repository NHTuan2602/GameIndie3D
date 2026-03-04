using UnityEngine;

public class DragAndSnap : MonoBehaviour
{
    [Header("Cài đặt Nút bấm")]
    [Tooltip("0 = Chuột Trái, 1 = Chuột Phải")]
    public int dragButton = 0; // Đã chốt cứng là Chuột Trái!

    [Header("Định danh món đồ")]
    public string itemType;
    public float snapDistance = 3.0f;
    public float holdDistance = 3.5f;

    private Vector3 startPosition;
    private bool isFinished = false;
    private bool isDragging = false;

    public delegate void ItemPlacedAction();
    public static event ItemPlacedAction OnItemPlaced;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isFinished) return;

        // Tọa độ CHÍNH GIỮA màn hình (Tâm ngắm Crosshair)
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        // 1. Khi VỪA BẤM Chuột Trái xuống
        if (Input.GetMouseButtonDown(dragButton))
        {
            // Bắn laser từ chính giữa màn hình (chuẩn FPS)
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == this.transform)
                {
                    isDragging = true;
                }
            }
        }

        // 2. Khi ĐANG GIỮ Chuột Trái
        if (Input.GetMouseButton(dragButton) && isDragging)
        {
            // Ép đồ vật lơ lửng ngay trước tâm ngắm của camera
            Vector3 targetPos = Camera.main.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, holdDistance));
            transform.position = Vector3.Lerp(transform.position, targetPos, 15f * Time.deltaTime);
        }

        // 3. Khi NHẢ Chuột Trái ra
        if (Input.GetMouseButtonUp(dragButton) && isDragging)
        {
            isDragging = false;
            CheckAndSnap();
        }
    }

    void CheckAndSnap()
    {
        TargetSlot[] allSlots = FindObjectsOfType<TargetSlot>();
        TargetSlot bestSlot = null;
        float closestDist = snapDistance;

        foreach (TargetSlot slot in allSlots)
        {
            if (slot.isOccupied) continue;

            if (slot.acceptedType == this.itemType)
            {
                float dist = Vector3.Distance(transform.position, slot.transform.position);
                if (dist <= closestDist)
                {
                    closestDist = dist;
                    bestSlot = slot;
                }
            }
        }

        if (bestSlot != null)
        {
            transform.position = bestSlot.transform.position;
            transform.rotation = bestSlot.transform.rotation;

            bestSlot.isOccupied = true;
            isFinished = true;

            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null) myCollider.enabled = false;

            if (OnItemPlaced != null) OnItemPlaced.Invoke();
            Debug.Log("Đã xếp " + itemType + " lên kệ!");
        }
        else
        {
            transform.position = startPosition;
            Debug.Log("Sai vị trí, rơi lại vào thùng.");
        }
    }
}