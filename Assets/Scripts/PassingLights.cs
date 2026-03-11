using UnityEngine;

public class PassingLights : MonoBehaviour
{
    [Header("Cài đặt Tốc độ")]
    [Tooltip("LƯU Ý: Mọi cây cối, nhà cửa phải có cùng 1 tốc độ để không đụng nhau!")]
    public float speed = 10f;

    [Header("Quỹ đạo chạy (Đã đảo chiều)")]
    public float startZ = -14f; // Vị trí xuất hiện ở đầu xe (số âm)
    public float endZ = 15f;    // Vị trí biến mất ở đuôi xe (số dương)

    [Header("Chống trùng lặp (Giãn cách ngẫu nhiên)")]
    [Tooltip("Khoảng cách ngắn nhất cộng thêm khi reset")]
    public float minExtraGap = 5f;
    [Tooltip("Khoảng cách xa nhất cộng thêm khi reset")]
    public float maxExtraGap = 20f;

    private Vector3 initialLocalPos;

    void Start()
    {
        // Ghi nhớ tọa độ gốc (đặc biệt là trục X và Y để nó không bị lệch khỏi mặt đường)
        initialLocalPos = transform.localPosition;
    }

    void Update()
    {
        // Ép vật thể chạy lùi về sau (hướng Z dương)
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);

        // Kiểm tra nếu vật thể lướt qua khỏi đuôi xe
        if (transform.localPosition.z > endZ)
        {
            // BÍ QUYẾT Ở ĐÂY: Sinh ra một khoảng cách ngẫu nhiên
            float randomGap = Random.Range(minExtraGap, maxExtraGap);

            // Lùi nó về vạch xuất phát (startZ), TRỪ ĐI khoảng cách ngẫu nhiên để nó đứng chờ ở xa hơn
            float newZ = startZ - randomGap;

            // Dịch chuyển vật thể về vị trí mới
            transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, newZ);
        }
    }
}