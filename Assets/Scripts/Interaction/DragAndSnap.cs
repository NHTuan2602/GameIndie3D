using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class DragAndSnap : MonoBehaviour
{
    public static event Action OnItemPlaced;

    [Header("Cài đặt Nút bấm & Định danh")]
    public int dragButton = 0;
    public string itemType = "Coca";

    [Header("Cài đặt Cầm nắm & Bắt dính")]
    public float snapDistance = 1.5f;
    public float holdDistance = 1f; // Khoảng cách đẩy về phía trước mặt

    [Tooltip("Chỉnh tọa độ cầm: X(Phải/Trái), Y(Lên/Xuống), Z(Tiến/Lùi)")]
    public Vector3 holdOffset = new Vector3(0.5f, -0.4f, 0f); // Mặc định lệch phải 0.5, thấp xuống 0.4

    private Rigidbody rb;
    private bool isDragging = false;
    private Camera mainCamera;
    private Vector3 initialPosition;
    private bool isPlaced = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        initialPosition = transform.position;
    }

    void OnMouseDown()
    {
        if (isPlaced) return;

        isDragging = true;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            // BÍ QUYẾT: Thay vì dính ở giữa màn hình, ta cộng thêm offset để đưa sang góc phải
            Vector3 targetPosition = mainCamera.transform.position
                                   + mainCamera.transform.forward * holdDistance
                                   + mainCamera.transform.right * holdOffset.x
                                   + mainCamera.transform.up * holdOffset.y;

            transform.position = targetPosition;

            // Ép chai nước luôn đứng thẳng bảnh bao khi cầm trên tay (không bị lật ngang)
            transform.rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f);
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;
        CheckForSnap();
    }

    private void CheckForSnap()
    {
        bool snapped = false;
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");

        foreach (GameObject target in targets)
        {
            if (target.name.Contains("Target_" + itemType))
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance <= snapDistance)
                {
                    transform.position = target.transform.position;
                    transform.rotation = target.transform.rotation;

                    rb.isKinematic = true;
                    rb.useGravity = false;
                    isPlaced = true;

                    target.tag = "Untagged";

                    Debug.Log("Đặt hàng lên kệ thành công!");
                    OnItemPlaced?.Invoke();

                    snapped = true;
                    break;
                }
            }
        }

        if (!snapped)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 safePosition = initialPosition;
            safePosition.y += 0.2f;
            transform.position = safePosition;

            Debug.Log("Sai vị trí hoặc vị trí đã có hàng. Rơi lại vào thùng.");
        }
    }
}