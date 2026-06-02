using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("Mục tiêu")]
    public Transform target; // Kéo PlayerBike vào đây

    [Header("Cài đặt Góc nhìn thứ nhất")]
    // Độ cao ghế ngồi để nhìn bao quát hơn (Y = 2.5)
    public Vector3 offset = new Vector3(0, 2.5f, 0.2f);

    [Header("Góc ngắm & Bù trừ bẻ cong")]
    public float lookDistance = 20f;

    // BIẾN BÙ TRỪ: Đặt SỐ DƯƠNG để mắt liếc sang PHẢI (Chữ C ngược).
    // Phù hợp khi CurveStrength đang là số dương (ví dụ: 0.0002)
    public float curveLookOffsetX = 1.5f;

    public float followSpeed = 15f;
    public float lookSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. DI CHUYỂN: Bám theo vị trí mục tiêu cực kỳ cẩn thận
        Vector3 targetPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // 2. GÓC NHÌN: Liếc mắt theo hình chữ C ngược
        Vector3 pointAhead = target.position + target.forward * lookDistance;

        // Dịch điểm ngắm sang phải để đuổi theo chân trời đang bị bẻ cong
        pointAhead.x += curveLookOffsetX;

        Quaternion targetRotation = Quaternion.LookRotation(pointAhead - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }
}