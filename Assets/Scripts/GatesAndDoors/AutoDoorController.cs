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
            else Debug.Log("<color=yellow>[Cửa] Đang quay lưng với cửa!</color>");
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
                Debug.Log($"<color=red>[Cửa] CỬA KHÓA! Cần vật phẩm: {requiredKeyID}</color>");
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
        // Ghi log ra Console để xem ai đang chạm vào cửa
        Debug.Log($"<color=cyan>[Cửa] Có vật thể chạm vào Trigger: {other.name} (Tag: {other.tag})</color>");

        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            occupantsCount++;
        }

        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerCollider = other;
        }
        else if (other.CompareTag("Enemy"))
        {
            Debug.Log("<color=magenta>[Cửa] Kẻ địch đã chạm cửa! Yêu cầu mở cửa tự động!</color>");
            if (!isLocked) OpenDoor(other.transform.position);
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