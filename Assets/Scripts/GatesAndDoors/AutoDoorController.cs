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
    [Tooltip("Gõ chữ 'key' để dùng chìa khóa, gõ 'nippers' để dùng kềm cắt")]
    public string requiredKeyID = "key";

    [Header("Cửa Tẩu Thoát (Chỉ mở Đêm 5)")]
    public bool isEscapeDoor = false;

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

        // Hỗ trợ dự phòng nếu cửa dùng Trigger Collider
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && playerCollider != null)
        {
            Vector3 directionToDoor = (transform.position - playerCollider.transform.position).normalized;
            float lookAngle = Vector3.Dot(playerCollider.transform.forward, directionToDoor);

            if (lookAngle > 0f) ProcessInteraction(playerCollider.transform.position);
        }
    }

    // =================================================================
    // ĐÃ FIX: HÀM INTERACT LÀM CẦU NỐI CHO HỆ THỐNG TIA NGẮM (RAYCAST)
    // Khi bạn chỉa tâm vào cửa và ấn E, hàm này sẽ được gọi!
    // =================================================================
    public void Interact()
    {
        // Lấy tọa độ của Camera người chơi để tính toán hướng mở cửa (đẩy vào hay kéo ra)
        Vector3 playerPos = Camera.main != null ? Camera.main.transform.position : transform.position + transform.forward;
        ProcessInteraction(playerPos);
    }

    private void ProcessInteraction(Vector3 interactorPosition)
    {
        // 1. KIỂM TRA CHỐT CHẶN ĐÊM 5
        if (isEscapeDoor && GameManager.instance != null && GameManager.instance.currentDay < 5)
        {
            if (audioSource != null && lockedSound != null && !audioSource.isPlaying)
                audioSource.PlayOneShot(lockedSound);

            Debug.Log("<color=red>[Cửa] CỬA KHÓA! Chưa đến đêm tẩu thoát (Đêm 5)!</color>");
            return; // Đuổi về, không cho mở
        }

        // 2. KIỂM TRA CHÌA KHÓA
        if (isLocked)
        {
            if (CheckInventoryForKey())
            {
                if (audioSource != null && unlockSound != null) audioSource.PlayOneShot(unlockSound);
                isLocked = false;
                OpenDoor(interactorPosition);
            }
            else
            {
                if (audioSource != null && lockedSound != null && !audioSource.isPlaying)
                    audioSource.PlayOneShot(lockedSound);
                Debug.Log($"<color=red>[Cửa] CỬA KHÓA! Cần vật phẩm: {requiredKeyID}</color>");
            }
        }
        // 3. MỞ BÌNH THƯỜNG (Nếu không khóa)
        else
        {
            OpenDoor(interactorPosition);
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

    private bool CheckInventoryForKey()
    {
        string keyStr = requiredKeyID.ToLower().Trim();
        if (GameManager.instance != null && keyStr == "key" && GameManager.instance.hasKey) return true;
        if (GameManager.instance != null && keyStr == "nippers" && GameManager.instance.hasNippers) return true;
        return false;
    }

    // --- Auto Close nếu AI Kẻ Địch đi ngang qua ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy")) occupantsCount++;

        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerCollider = other;
        }
        else if (other.CompareTag("Enemy"))
        {
            if (!isLocked && (!isEscapeDoor || (GameManager.instance != null && GameManager.instance.currentDay >= 5)))
            {
                OpenDoor(other.transform.position);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            occupantsCount--;
            if (occupantsCount < 0) occupantsCount = 0;
        }

        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerCollider = null;
        }

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