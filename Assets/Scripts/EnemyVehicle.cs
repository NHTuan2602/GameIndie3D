using UnityEngine;

public class EnemyVehicle : MonoBehaviour
{
    public float driveSpeed = 15f; // Tốc độ chạy ngược chiều

    void Update()
    {
        // Xe chạy ngược lại phía sau (hướng về phía người chơi)
        // Lưu ý: Nếu xe của bạn quay mặt về phía người chơi sẵn rồi thì dùng Vector3.forward
        // Nếu xe đang quay mặt cùng chiều nhưng bạn muốn nó lùi lại thì dùng Vector3.back
        transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime);
    }
}