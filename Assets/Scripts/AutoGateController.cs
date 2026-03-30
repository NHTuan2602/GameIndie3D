using UnityEngine;

public class AutoGateController : MonoBehaviour
{
    [Header("--- CÀI ĐẶT CỬA ---")]
    public Transform leftGateHinge;
    public Transform rightGateHinge;

    [Tooltip("Góc mở cho cửa trái (Thường là số âm, ví dụ: -90)")]
    public float leftOpenAngle = -90f;
    [Tooltip("Góc mở cho cửa phải (Thường là số dương, ví dụ: 90)")]
    public float rightOpenAngle = 90f;

    public float smoothSpeed = 2f;

    [Header("--- TRẠNG THÁI ---")]
    private bool isPlayerNear = false;

    void Update()
    {
        // Góc mục tiêu khi mở và khi đóng (đóng luôn là 0)
        float targetLeft = isPlayerNear ? leftOpenAngle : 0f;
        float targetRight = isPlayerNear ? rightOpenAngle : 0f;

        // Xoay cửa trái
        Quaternion targetRotLeft = Quaternion.Euler(0, targetLeft, 0);
        leftGateHinge.localRotation = Quaternion.Slerp(leftGateHinge.localRotation, targetRotLeft, Time.deltaTime * smoothSpeed);

        // Xoay cửa phải
        Quaternion targetRotRight = Quaternion.Euler(0, targetRight, 0);
        rightGateHinge.localRotation = Quaternion.Slerp(rightGateHinge.localRotation, targetRotRight, Time.deltaTime * smoothSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bus") || other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bus") || other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}