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

    [Header("Cài đặt Âm Thanh")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;
    public AudioClip unlockSound;

    [Header("Trạng thái (Chỉ xem)")]
    public bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine closeRoutine;

    private bool isPlayerNear = false;
    private Collider playerCollider;

    // MỚI: Biến đếm số người đang đứng ở cửa
    private int occupantsCount = 0;

    void Start()
    {
        if (doorHinge == null) doorHinge = transform;
        closedRotation = doorHinge.localRotation;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isLocked && isOpen) isOpen = false;

        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        doorHinge.localRotation = Quaternion.Slerp(doorHinge.localRotation, targetRotation, Time.deltaTime * smoothSpeed);

        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && playerCollider != null)
        {
            Vector3 directionToDoor = (transform.position - playerCollider.transform.position).normalized;
            float lookAngle = Vector3.Dot(playerCollider.transform.forward, directionToDoor);

            if (lookAngle > 0f) TryToInteract(playerCollider);
            else Debug.Log("<color=yellow>Đang quay lưng với cửa!</color>");
        }
    }

    public void OpenDoor(Vector3 interactorPosition)
    {
        if (isLocked) return;

        Vector3 directionToInteractor = (interactorPosition - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionToInteractor);

        float actualOpenAngle = (dotProduct > 0) ? openAngle : -openAngle;

        Vector3 rotationVector = Vector3.zero;
        if (axisToRotate == RotationAxis.X) rotationVector = new Vector3(actualOpenAngle, 0, 0);
        else if (axisToRotate == RotationAxis.Y) rotationVector = new Vector3(0, actualOpenAngle, 0);
        else if (axisToRotate == RotationAxis.Z) rotationVector = new Vector3(0, 0, actualOpenAngle);

        openRotation = closedRotation * Quaternion.Euler(rotationVector);

        if (!isOpen && audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

        isOpen = true;
        if (closeRoutine != null) StopCoroutine(closeRoutine);
        closeRoutine = StartCoroutine(AutoCloseRoutine());
    }

    public void OpenDoor() { OpenDoor(transform.position + transform.forward); }

    private void TryToInteract(Collider other)
    {
        if (isLocked)
        {
            if (CheckInventoryForKey())
            {
                if (audioSource != null && unlockSound != null) audioSource.PlayOneShot(unlockSound);
                isLocked = false;
                OpenDoor(other.transform.position);
            }
            else
            {
                if (audioSource != null && lockedSound != null && !audioSource.isPlaying)
                    audioSource.PlayOneShot(lockedSound);
                Debug.Log($"<color=red>CỬA KHÓA! Cần vật phẩm: {requiredKeyID}</color>");
            }
        }
        else OpenDoor(other.transform.position);
    }

    private bool CheckInventoryForKey()
    {
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredKeyID)) return true;
        if (GameManager.instance != null && requiredKeyID == "key" && GameManager.instance.hasKey) return true;
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            occupantsCount++; // Có người bước vào vùng cửa
        }

        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerCollider = other;
        }
        else if (other.CompareTag("Enemy"))
        {
            // Quái thì tự động mở cửa (nếu cửa không khóa)
            if (!isLocked) OpenDoor(other.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            occupantsCount--; // Có người bước ra
            if (occupantsCount < 0) occupantsCount = 0;
        }

        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerCollider = null;
        }

        // CHỈ ĐÓNG CỬA KHI KHÔNG CÒN AI ĐỨNG TRONG VÙNG ĐÓ NỮA
        if (occupantsCount == 0 && isOpen && !isLocked)
        {
            if (closeRoutine != null) StopCoroutine(closeRoutine);
            closeRoutine = StartCoroutine(AutoCloseRoutine());
        }
    }

    IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (isOpen && audioSource != null && closeSound != null)
            audioSource.PlayOneShot(closeSound);

        isOpen = false;
    }
}