using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Tọa độ Tâm 3 Làn (BẮT BUỘC ĐIỀN ĐÚNG)")]
    // Ví dụ: Làn trái X = -3, Giữa X = 0, Phải X = 3
    public float[] laneCenters = new float[3] { -3f, 0f, 3f }; // Nhớ nhập lại số chuẩn trên Inspector!

    [Header("Kho Vật Phẩm (Prefabs)")]
    public GameObject[] dangerPrefabs; // Xe ngược chiều, Ổ gà
    public GameObject[] buffPrefabs;   // Dốc tăng tốc, Thùng đồ

    [Header("Bẫy Biển Báo")]
    public GameObject trafficSignGatePrefab; // Cổng Biển Báo

    [Header("Cài đặt Sinh sản")]
    public Transform player;
    public float spawnDistanceAhead = 80f; // Khoảng cách sinh ra trước mặt
    public float spawnInterval = 2f;       // Cứ 2 giây sinh 1 lần

    [Range(0f, 1f)] public float buffChance = 0.3f; // 30% ra đồ buff

    private float timer;
    private int spawnCount = 0; // Bộ đếm nhịp để xuất hiện Biển báo

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnRow();
            timer = 0f;
        }
    }

    void SpawnRow()
    {
        spawnCount++;

        // 1. SỰ KIỆN ĐẶC BIỆT: Cứ 4 nhịp thì xuất hiện Cổng Biển Báo
        if (spawnCount % 4 == 0 && trafficSignGatePrefab != null)
        {
            float centerOfRoadX = laneCenters[1]; // Lấy làn giữa
            Vector3 gatePos = new Vector3(centerOfRoadX, transform.position.y, player.position.z + spawnDistanceAhead);

            GameObject gate = Instantiate(trafficSignGatePrefab, gatePos, Quaternion.identity);
            Destroy(gate, 15f); // Tự hủy sau 15s

            return; // DỪNG LẠI TẠI ĐÂY! Tránh sinh thêm ổ gà chồng chéo lên cổng
        }

        // 2. SỰ KIỆN BÌNH THƯỜNG: Thả Ổ gà / Xe
        // Trộn làn
        int[] lanes = { 0, 1, 2 };
        for (int i = 0; i < lanes.Length; i++)
        {
            int temp = lanes[i];
            int r = Random.Range(i, lanes.Length);
            lanes[i] = lanes[r];
            lanes[r] = temp;
        }

        // Chỉ sinh ra 1 hoặc 2 vật cản mỗi hàng
        int obstacleCount = Random.Range(1, 3);

        for (int i = 0; i < obstacleCount; i++)
        {
            float spawnX = laneCenters[lanes[i]];

            // BƯỚC A (FIX LỖI): Quyết định xem sẽ đẻ ra vật gì trước
            GameObject prefabToSpawn = null;
            if (Random.value > buffChance && dangerPrefabs.Length > 0)
            {
                prefabToSpawn = dangerPrefabs[Random.Range(0, dangerPrefabs.Length)];
            }
            else if (buffPrefabs.Length > 0)
            {
                prefabToSpawn = buffPrefabs[Random.Range(0, buffPrefabs.Length)];
            }

            // BƯỚC B (FIX LỖI): Nếu đã có vật phẩm, mới lấy chiều cao và thả xuống đường
            if (prefabToSpawn != null)
            {
                // Tự động đọc xem bản gốc Prefab dặn độ cao Y bao nhiêu thì lấy đúng số đó
                float spawnY = prefabToSpawn.transform.position.y;
                Vector3 spawnPos = new Vector3(spawnX, spawnY, player.position.z + spawnDistanceAhead);

                // Thả vật cản
                GameObject newObstacle = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);

                // Hẹn giờ hủy rác sau 10 giây để chống giật lag
                Destroy(newObstacle, 10f);
            }
        }
    }
}