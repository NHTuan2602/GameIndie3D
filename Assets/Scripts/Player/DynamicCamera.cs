using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("Mục tiêu")]
    public Transform target; // Kéo PlayerBike vào đây

    [Header("Cài đặt Góc nhìn thứ nhất")]
    // Vị trí mắt: X = 0 (giữa), Y = 1.5 (ngang đầu), Z = 0.2 (hơi nhích lên trên ghi-đông)
    public Vector3 offset = new Vector3(0, 1.5f, 0.2f);

    // BIẾN MỚI: Điểm nhìn cách mũi xe bao xa (càng xa nhìn càng thẳng)
    public float lookDistance = 20f;

    public float followSpeed = 15f; // Rút ngắn thời gian delay để không bị chóng mặt
    public float lookSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. DI CHUYỂN: Bám chặt vào vị trí mắt người lái (có độ mượt nhẹ để cảm nhận gia tốc)
        Vector3 targetPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // 2. GÓC NHÌN: Quét 1 điểm tít đằng xa (cách 20m) và nhìn vào đó
        // Cộng thêm Vector3.up * 1.5f để mắt nhìn thẳng song song với mặt đường
        Vector3 pointAhead = target.position + target.forward * lookDistance + Vector3.up * 1.5f;

        Quaternion targetRotation = Quaternion.LookRotation(pointAhead - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
    }
}