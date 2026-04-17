using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    public Transform target;        // Kéo xe đạp vào đây
    public Vector3 offset = new Vector3(0, 5, -10); // Khoảng cách xe và cam
    public float smoothSpeed = 0.125f; // Độ mượt của vị trí
    public float rotationSmooth = 5f;  // Độ mượt của góc xoay

    void LateUpdate() // Dùng LateUpdate để cam không bị giật (jitter)
    {
        if (target == null) return;

        // 1. Xử lý Vị trí (Theo sát xe khi lên cao)
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // 2. Xử lý Góc xoay (Xoay cam theo độ nghiêng của xe)
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmooth * Time.deltaTime);
    }
}