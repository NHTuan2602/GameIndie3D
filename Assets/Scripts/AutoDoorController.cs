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

    [Header("Trạng thái (Chỉ xem)")]
    public bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine closeRoutine;

    void Start()
    {
        if (doorHinge == null) doorHinge = transform;
        closedRotation = doorHinge.localRotation;

        Vector3 rotationVector = Vector3.zero;
        if (axisToRotate == RotationAxis.X) rotationVector = new Vector3(openAngle, 0, 0);
        else if (axisToRotate == RotationAxis.Y) rotationVector = new Vector3(0, openAngle, 0);
        else if (axisToRotate == RotationAxis.Z) rotationVector = new Vector3(0, 0, openAngle);

        openRotation = closedRotation * Quaternion.Euler(rotationVector);
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        doorHinge.localRotation = Quaternion.Slerp(doorHinge.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    // Hàm này dùng để gọi từ phím E hoặc Trigger
    public void OpenDoor()
    {
        isOpen = true;
        // Mỗi khi hàm này được gọi, đồng hồ sẽ bị Reset lại từ đầu
        if (closeRoutine != null) StopCoroutine(closeRoutine);
        closeRoutine = StartCoroutine(AutoCloseRoutine());
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Nếu người chơi đứng trong vùng cửa, liên tục gọi Open để reset đồng hồ
            OpenDoor();
        }
    }

    IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        isOpen = false;
        Debug.Log("<color=yellow>Cửa: Đã hết thời gian chờ, đang đóng...</color>");
    }
}