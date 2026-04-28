using UnityEngine;
using System.Collections;

public class AutoDoorController : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("Cài đặt Xoay")]
    public Transform doorHinge;
    public RotationAxis axisToRotate = RotationAxis.Z;
    public float openAngle = 90f;
    public float smoothSpeed = 5f;
    public float autoCloseDelay = 3f;

    [Header("Cài đặt Khóa Cửa")]
    public bool isLocked = false;
    public string requiredKeyID = "key";

    [Header("Trạng thái (Chỉ xem)")]
    public bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine closeRoutine;

    void Start()
    {
        if (doorHinge == null) doorHinge = transform;
        closedRotation = doorHinge.localRotation;
    }

    void Update()
    {
        if (isLocked && isOpen)
        {
            isOpen = false;
        }

        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        doorHinge.localRotation = Quaternion.Slerp(doorHinge.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    // ==========================================
    // MỞ CỬA THÔNG MINH (CÓ GẮN MÁY QUÉT DEBUG)
    // ==========================================
    public void OpenDoor(Vector3 interactorPosition)
    {
        if (isLocked) return;

        // Tính toán hướng
        Vector3 directionToInteractor = (interactorPosition - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionToInteractor);

        // IN RA CONSOLE ĐỂ BẮT LỖI
        if (dotProduct > 0)
        {
            Debug.Log($"<color=cyan>HỆ THỐNG BÁO: Bạn đang đứng TRƯỚC mặt cửa (Dot: {dotProduct}). Cửa sẽ mở góc Dương!</color>");
        }
        else
        {
            Debug.Log($"<color=orange>HỆ THỐNG BÁO: Bạn đang đứng SAU lưng cửa (Dot: {dotProduct}). Cửa sẽ mở góc Âm!</color>");
        }

        float actualOpenAngle = (dotProduct > 0) ? openAngle : -openAngle;

        Vector3 rotationVector = Vector3.zero;
        if (axisToRotate == RotationAxis.X) rotationVector = new Vector3(actualOpenAngle, 0, 0);
        else if (axisToRotate == RotationAxis.Y) rotationVector = new Vector3(0, actualOpenAngle, 0);
        else if (axisToRotate == RotationAxis.Z) rotationVector = new Vector3(0, 0, actualOpenAngle);

        openRotation = closedRotation * Quaternion.Euler(rotationVector);

        isOpen = true;
        if (closeRoutine != null) StopCoroutine(closeRoutine);
        closeRoutine = StartCoroutine(AutoCloseRoutine());
    }

    public void OpenDoor()
    {
        OpenDoor(transform.position + transform.forward);
    }

    private void TryToInteract(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isLocked)
            {
                bool hasKey = CheckInventoryForKey();

                if (hasKey)
                {
                    Debug.Log("<color=green>ĐÃ DÙNG CHÌA KHÓA MỞ CỬA! Lối tắt đã được thông.</color>");
                    isLocked = false;
                    OpenDoor(other.transform.position);
                }
                else
                {
                    Debug.Log($"<color=red>CỬA KHÓA! Bạn cần tìm vật phẩm có ID: {requiredKeyID}</color>");
                }
            }
            else
            {
                OpenDoor(other.transform.position);
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            if (!isLocked)
            {
                OpenDoor(other.transform.position);
            }
        }
    }

    private bool CheckInventoryForKey()
    {
        if (InventoryManager.Instance != null)
        {
            return InventoryManager.Instance.HasItem(requiredKeyID);
        }

        Debug.LogError("LỖI: Không tìm thấy InventoryManager trong Scene!");
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryToInteract(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryToInteract(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (!isLocked)
            {
                if (closeRoutine != null) StopCoroutine(closeRoutine);
                closeRoutine = StartCoroutine(AutoCloseRoutine());
            }
        }
    }

    IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        isOpen = false;
    }
}