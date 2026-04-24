using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Tọa độ Mặt Đường")]
    public float roadMinX = -4.5f;
    public float roadMaxX = 4.5f;

    [Header("Tọa độ 3 Làn (BẮT BUỘC NHẬP: 0.45, 14, -14)")]
    public float[] laneCenters = new float[3] { 0.45f, 14f, -14f };

    [Header("Kho Vật Phẩm (Prefabs)")]
    public GameObject[] dangerPrefabs;
    public GameObject[] buffPrefabs;
    public GameObject brickPrefab;
    public GameObject trafficSignGatePrefab;

    [Header("Kho Hung Thần (Xe Khách, Cấp Cứu)")]
    public GameObject[] highSpeedVehicles;

    [Header("Cài đặt Cổng Biển Báo (V.I.P)")]
    public float gateInterval = 30f; // 30s đẻ 1 cổng
    public float gapDuration = 3f;   // Quãng nghỉ 3s trước khi cổng ra
    public float gateLifeTime = 15f; // Thời gian cổng cản đường

    [Header("Cài đặt Xe Khách (RANDOM)")]
    public float minVehicleInterval = 12f; // Xe bus ra ngẫu nhiên từ 12s...
    public float maxVehicleInterval = 25f; // ...đến 25s
    private float targetVehicleInterval;   // Con số random sẽ được lưu vào đây

    [Header("Cài đặt Vật cản tĩnh & Gạch")]
    public Transform player;
    public float spawnDistanceAhead = 150f;
    public float spawnInterval = 2f;
    public float brickInterval = 6f;
    [Range(0f, 1f)] public float buffChance = 0.2f;

    [Header("Radar Chống Trùng Lặp")]
    public int maxSpawnAttempts = 3;
    public Vector3 clearanceBoxSize = new Vector3(1.5f, 1f, 20f);

    // Đồng hồ bấm giờ
    private float obstacleTimer;
    private float brickTimer;
    private float gateTimer;
    private float vehicleTimer;
    private float gateActiveTimer; // Khóa không cho xe bus đẻ khi cổng đang tồn tại

    void Start()
    {
        // Khởi tạo thời gian random cho chiếc xe bus đầu tiên
        targetVehicleInterval = Random.Range(minVehicleInterval, maxVehicleInterval);
    }

    void Update()
    {
        // ==========================================
        // 1. HỆ THỐNG CỔNG BIỂN BÁO (QUYỀN LỰC TỐI CAO)
        // ==========================================
        gateTimer += Time.deltaTime;

        // Trừ dần thời gian tồn tại của cổng (nếu đang có cổng)
        if (gateActiveTimer > 0) gateActiveTimer -= Time.deltaTime;

        bool isGapTime = (gateInterval - gateTimer) <= gapDuration;

        if (gateTimer >= gateInterval)
        {
            SpawnGate();
            gateTimer = 0f;
            gateActiveTimer = gateLifeTime; // KHÓA XE BUS TRONG 15 GIÂY TIẾP THEO!
        }

        // NẾU SẮP ĐẺ CỔNG -> CHẶN ĐỨNG TẤT CẢ MỌI THỨ CÒN LẠI
        if (isGapTime) return;

        // ==========================================
        // 2. HỆ THỐNG XE BUS (CHỈ CHẠY KHI KHÔNG CÓ CỔNG)
        // ==========================================
        if (gateActiveTimer <= 0)
        {
            vehicleTimer += Time.deltaTime;
            if (vehicleTimer >= targetVehicleInterval)
            {
                SpawnHighSpeedVehicle();
                vehicleTimer = 0f;
                // Bốc thăm lại thời gian ngẫu nhiên cho chuyến xe bus tiếp theo
                targetVehicleInterval = Random.Range(minVehicleInterval, maxVehicleInterval);
            }
        }

        // ==========================================
        // 3. HỆ THỐNG VẬT CẢN TĨNH & GẠCH
        // ==========================================
        obstacleTimer += Time.deltaTime;
        if (obstacleTimer >= spawnInterval)
        {
            SpawnRandomObstacle();
            obstacleTimer = 0f;
        }

        brickTimer += Time.deltaTime;
        if (brickTimer >= brickInterval)
        {
            SpawnSingleBrick();
            brickTimer = 0f;
        }
    }

    // CÁC HÀM SPAWN (GIỮ NGUYÊN NHƯ CŨ)
    void SpawnHighSpeedVehicle()
    {
        if (highSpeedVehicles == null || highSpeedVehicles.Length == 0) return;
        Debug.Log("<color=orange>CẢNH BÁO: HUNG THẦN ĐƯỜNG PHỐ XUẤT HIỆN!</color>");
        GameObject prefabToSpawn = highSpeedVehicles[Random.Range(0, highSpeedVehicles.Length)];
        float randomLaneX = laneCenters[Random.Range(0, laneCenters.Length)];
        Vector3 checkPos = new Vector3(randomLaneX, 1f, player.position.z + spawnDistanceAhead);

        if (IsPathClear(checkPos))
        {
            float spawnY = prefabToSpawn.transform.position.y;
            Vector3 spawnPos = new Vector3(randomLaneX, spawnY, player.position.z + spawnDistanceAhead);
            GameObject obj = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);
            Destroy(obj, 20f);
        }
    }

    void SpawnGate()
    {
        if (trafficSignGatePrefab == null) return;
        bool allClear = true;
        foreach (float xCenter in laneCenters)
        {
            Vector3 checkPos = new Vector3(xCenter, 1f, player.position.z + spawnDistanceAhead);
            if (!IsPathClear(checkPos)) { allClear = false; break; }
        }

        if (allClear)
        {
            Debug.Log("<color=red>ĐANG ĐẺ CỔNG SINH TỬ!</color>");
            Vector3 spawnPos = new Vector3(0f, trafficSignGatePrefab.transform.position.y, player.position.z + spawnDistanceAhead);
            GameObject gateObj = Instantiate(trafficSignGatePrefab, spawnPos, trafficSignGatePrefab.transform.rotation);
            SignGateController gateScript = gateObj.GetComponent<SignGateController>();
            if (gateScript != null) gateScript.SetLanePositions(laneCenters);
            Destroy(gateObj, 15f);
        }
    }

    void SpawnRandomObstacle()
    {
        GameObject prefabToSpawn = null;
        if (Random.value < buffChance && buffPrefabs.Length > 0) prefabToSpawn = buffPrefabs[Random.Range(0, buffPrefabs.Length)];
        else if (dangerPrefabs.Length > 0) prefabToSpawn = dangerPrefabs[Random.Range(0, dangerPrefabs.Length)];

        if (prefabToSpawn != null)
        {
            for (int i = 0; i < maxSpawnAttempts; i++)
            {
                float randomX = Random.Range(roadMinX, roadMaxX);
                Vector3 checkPos = new Vector3(randomX, 1f, player.position.z + spawnDistanceAhead);
                if (IsPathClear(checkPos)) { Spawn(prefabToSpawn, randomX); break; }
            }
        }
    }

    void SpawnSingleBrick()
    {
        if (brickPrefab == null) return;
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float randomX = Random.Range(roadMinX, roadMaxX);
            Vector3 checkPos = new Vector3(randomX, 1f, player.position.z + spawnDistanceAhead);
            if (IsPathClear(checkPos)) { Spawn(brickPrefab, randomX); break; }
        }
    }

    bool IsPathClear(Vector3 spawnPos)
    {
        Collider[] hits = Physics.OverlapBox(spawnPos, clearanceBoxSize, Quaternion.identity);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Obstacle") || hit.CompareTag("Ramp") || hit.CompareTag("Buff") || hit.CompareTag("Brick")) return false;
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        if (player != null)
        {
            Vector3 checkPos = new Vector3(0, 1f, player.position.z + spawnDistanceAhead);
            Gizmos.DrawCube(checkPos, clearanceBoxSize * 2);
        }
    }

    void Spawn(GameObject prefab, float x)
    {
        if (prefab == null) return;
        float spawnY = prefab.transform.position.y;
        Vector3 spawnPos = new Vector3(x, spawnY, player.position.z + spawnDistanceAhead);
        GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);
        Destroy(obj, 15f);
    }
}