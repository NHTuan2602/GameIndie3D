using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Cài đặt Cửa")]
    public float openAngle = 90f; // Góc mở (để -90f nếu muốn mở hướng ngược lại)
    public float openSpeed = 3f;  // Tốc độ mở

    private bool isOpen = false;
    private Quaternion defaultRotation;
    private Quaternion targetRotation;
    private Coroutine movingCoroutine;

    void Start()
    {
        // Ghi nhớ góc xoay mặc định lúc cửa đóng
        defaultRotation = transform.localRotation;
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // Tính toán góc đích đến
        if (isOpen)
            targetRotation = defaultRotation * Quaternion.Euler(0, openAngle, 0);
        else
            targetRotation = defaultRotation;

        // Nếu đang xoay dở mà bị bấm tiếp thì dừng tiến trình cũ, chạy tiến trình mới
        if (movingCoroutine != null) StopCoroutine(movingCoroutine);
        movingCoroutine = StartCoroutine(MoveDoor());
    }

    IEnumerator MoveDoor()
    {
        // Xoay mượt mà cho đến khi gần đến đích
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        // Chốt góc chính xác
        transform.localRotation = targetRotation;
    }
}