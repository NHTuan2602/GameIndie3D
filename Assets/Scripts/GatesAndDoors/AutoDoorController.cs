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
    public AudioClip openSound;    // Tiếng két/xịt mở cửa
    public AudioClip closeSound;   // Tiếng cạch đóng cửa
    public AudioClip lockedSound;  // Tiếng tít tít (Từ chối)
    public AudioClip unlockSound;  // Tiếng tít rào (Chấp nhận)

    [Header("Trạng thái (Chỉ xem)")]
    public bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine closeRoutine;

    // Biến kiểm tra người chơi đứng gần
    private bool isPlayerNear = false;
    private Collider playerCollider;

    void Start()
    {
        if (doorHinge == null) doorHinge = transform;
        closedRotation = doorHinge.localRotation;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 1. Xử lý hoạt ảnh xoay cửa
        if (isLocked && isOpen)
        {
            isOpen = false;
        }

        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        doorHinge.localRotation = Quaternion.Slerp(doorHinge.localRotation, targetRotation, Time.deltaTime * smoothSpeed);

        // 2. CHỜ NGƯỜI CHƠI BẤM PHÍM E VÀ PHẢI NHÌN VỀ PHÍA CỬA MỚI TƯƠNG TÁC
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && playerCollider != null)
        {
            // Tính hướng từ người chơi đến cánh cửa
            Vector3 directionToDoor = (transform.position - playerCollider.transform.position).normalized;

            // Kiểm tra xem mặt người chơi (forward) có đang hướng về phía cửa không
            float lookAngle = Vector3.Dot(playerCollider.transform.forward, directionToDoor);

            // Nếu góc nhìn hướng về phía cửa (lookAngle > 0) thì mới cho tương tác
            if (lookAngle > 0f)
            {
                TryToInteract(playerCollider);
            }
            else
            {
                Debug.Log("<color=yellow>Đang quay lưng với cửa, bấm E vào không khí sẽ không có tác dụng!</color>");
            }
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

        // Phát tiếng mở cửa (Chỉ phát nếu cửa đang đóng)
        if (!isOpen && audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

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
        if (isLocked)
        {
            bool hasKey = CheckInventoryForKey();

            if (hasKey)
            {
                if (audioSource != null && unlockSound != null) audioSource.PlayOneShot(unlockSound);
                isLocked = false;
                OpenDoor(other.transform.position);
            }
            else
            {
                // Phát tiếng cảnh báo lỗi
                if (audioSource != null && lockedSound != null && !audioSource.isPlaying)
                    audioSource.PlayOneShot(lockedSound);

                Debug.Log($"<color=red>CỬA KHÓA! Bạn cần tìm vật phẩm có ID: {requiredKeyID}</color>");
            }
        }
        else
        {
            OpenDoor(other.transform.position);
        }
    }

    private bool CheckInventoryForKey()
    {
        // 1. Dành cho người chơi bình thường: Kiểm tra túi đồ thật
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredKeyID))
        {
            return true;
        }

        // 2. Dành cho DEV MODE (Thượng đế): Kiểm tra cờ hack trong GameManager
        if (GameManager.instance != null)
        {
            // Nếu cửa yêu cầu "key" và Thượng đế đã tick chọn có Key
            if (requiredKeyID == "key" && GameManager.instance.hasKey)
            {
                Debug.Log("<color=magenta>Mở cửa bằng quyền lực DEV MODE!</color>");
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Lấy thông tin người chơi khi lại gần để chờ bấm E
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerCollider = other;
        }
        else if (other.CompareTag("Enemy"))
        {
            // Kẻ địch thì vẫn cho mở tự động không cần bấm E
            if (!isLocked) OpenDoor(other.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerCollider = null;
        }

        // Đi ra xa thì tự động đóng cửa lại
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (!isLocked && isOpen)
            {
                if (closeRoutine != null) StopCoroutine(closeRoutine);
                closeRoutine = StartCoroutine(AutoCloseRoutine());
            }
        }
    }

    IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        // Phát tiếng đóng cửa
        if (isOpen && audioSource != null && closeSound != null)
            audioSource.PlayOneShot(closeSound);

        isOpen = false;
    }
}