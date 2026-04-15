using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Tọa độ Tâm 3 Làn (BẮT BUỘC ĐIỀN ĐÚNG)")]
    // -10: Trái | 20: Giữa | 45: Phải
    public float[] laneCenters = new float[3] { -10f, 20f, 45f };

    [Header("Kho Vật Phẩm (Prefabs)")]
    public GameObject[] dangerPrefabs; // Xe ngược chiều, Ổ gà
    public GameObject[] buffPrefabs;   // Dốc tăng tốc, Thùng đồ

    [Header("Bẫy Biển Báo")]
    public GameObject trafficSignGatePrefab; // Kéo Prefab Cổng Biển Báo vào đây!

    [Header("Cài đặt Sinh sản")]
    public Transform player;
    public float spawnDistanceAhead = 80f;
    public float spawnInterval = 2f;

    [Range(0f, 1f)] public float buffChance = 0.3f;

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

        // ---------------------------------------------------------
        // SỰ KIỆN ĐẶC BIỆT: Cứ 4 nhịp thì xuất hiện Cổng Biển Báo
        // ---------------------------------------------------------
        if (spawnCount % 4 == 0 && trafficSignGatePrefab != null)
        {
            // Lấy tọa độ X của làn giữa (laneCenters[1]) để cổng nằm chính giữa đường
            float centerOfRoadX = laneCenters[1];
            Vector3 gatePos = new Vector3(centerOfRoadX, transform.position.y, player.position.z + spawnDistanceAhead);

            GameObject gate = Instantiate(trafficSignGatePrefab, gatePos, Quaternion.identity);
            Destroy(gate, 15f); // Dọn rác sau 15 giây

            return; // DỪNG LẠI TẠI ĐÂY! Tránh sinh thêm ổ gà chồng chéo lên cổng
        }

        // ---------------------------------------------------------
        // SỰ KIỆN BÌNH THƯỜNG: Thả Ổ gà / Xe ngược chiều / Dốc
        // ---------------------------------------------------------

        // 1. Chỉ sinh ra 1 hoặc 2 vật cản
        int obstacleCount = Random.Range(1, 3);

        // 2. Trộn ngẫu nhiên 3 làn
        int[] lanes = { 0, 1, 2 };
        for (int i = 0; i < lanes.Length; i++)
        {
            int temp = lanes[i];
            int randomIndex = Random.Range(i, lanes.Length);
            lanes[i] = lanes[randomIndex];
            lanes[randomIndex] = temp;
        }

        // 3. Thả vật phẩm
        for (int i = 0; i < obstacleCount; i++)
        {
            int selectedLaneIndex = lanes[i];
            float spawnX = laneCenters[selectedLaneIndex];

            Vector3 spawnPos = new Vector3(spawnX, transform.position.y, player.position.z + spawnDistanceAhead);
            GameObject prefabToSpawn = null;

            if (Random.value > buffChance && dangerPrefabs.Length > 0)
            {
                prefabToSpawn = dangerPrefabs[Random.Range(0, dangerPrefabs.Length)];
            }
            else if (buffPrefabs.Length > 0)
            {
                prefabToSpawn = buffPrefabs[Random.Range(0, buffPrefabs.Length)];
            }

            if (prefabToSpawn != null)
            {
                GameObject newObstacle = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);
                Destroy(newObstacle, 10f); // Dọn rác sau 10 giây
            }
        }
    }
}