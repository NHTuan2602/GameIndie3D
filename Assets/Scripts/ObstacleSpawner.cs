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
    public float gateInterval = 30f;
    public float gapDuration = 3f;
    public float gateLifeTime = 15f;

    [Header("Cài đặt Xe Khách (DỒN DẬP TỪ GIÂY 10)")]
    public float startSpawnBusAfter = 10f;
    public float busSpawnInterval = 2f;
    private float globalTimer = 0f;

    [Header("Cài đặt Vật cản tĩnh & Gạch")]
    public Transform player;
    public float spawnDistanceAhead = 150f;
    public float spawnInterval = 2f;
    public float brickInterval = 6f;
    [Range(0f, 1f)] public float buffChance = 0.2f;

    [Header("Radar Chống Trùng Lặp")]
    public int maxSpawnAttempts = 3;
    public Vector3 clearanceBoxSize = new Vector3(1.5f, 1f, 20f);

    private float obstacleTimer;
    private float brickTimer;
    private float gateTimer;
    private float vehicleTimer;
    private float gateActiveTimer;

    void Update()
    {
        globalTimer += Time.deltaTime;

        // 1. HỆ THỐNG CỔNG BIỂN BÁO
        gateTimer += Time.deltaTime;
        if (gateActiveTimer > 0) gateActiveTimer -= Time.deltaTime;

        bool isGapTime = (gateInterval - gateTimer) <= gapDuration;

        if (gateTimer >= gateInterval)
        {
            SpawnGate();
            gateTimer = 0f;
            gateActiveTimer = gateLifeTime;
        }

        if (isGapTime) return;

        // 2. HỆ THỐNG XE BUS 
        if (globalTimer >= startSpawnBusAfter && gateActiveTimer <= 0)
        {
            vehicleTimer += Time.deltaTime;
            if (vehicleTimer >= busSpawnInterval)
            {
                SpawnHighSpeedVehicle();
                vehicleTimer = 0f;
            }
        }

        // 3. HỆ THỐNG VẬT CẢN TĨNH & GẠCH
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

    // ĐÃ FIX: THÊM VÒNG LẶP ĐỂ XE BUS CỐ GẮNG TÌM LÀN TRỐNG
    void SpawnHighSpeedVehicle()
    {
        // Báo lỗi ra Console nếu quên kéo xe Bus vào Inspector
        if (highSpeedVehicles == null || highSpeedVehicles.Length == 0)
        {
            Debug.LogWarning("<color=yellow>LỖI: Bạn chưa kéo xe Bus vào mảng High Speed Vehicles trong GameManager!</color>");
            return;
        }

        bool spawned = false;

        // Thử tìm làn trống tối đa 3 lần
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            GameObject prefabToSpawn = highSpeedVehicles[Random.Range(0, highSpeedVehicles.Length)];
            float randomLaneX = laneCenters[Random.Range(0, laneCenters.Length)];
            Vector3 checkPos = new Vector3(randomLaneX, 1f, player.position.z + spawnDistanceAhead);

            if (IsPathClear(checkPos))
            {
                float spawnY = prefabToSpawn.transform.position.y;
                Vector3 spawnPos = new Vector3(randomLaneX, spawnY, player.position.z + spawnDistanceAhead);
                GameObject obj = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);
                Destroy(obj, 20f);
                spawned = true;
                break; // Đẻ thành công thì thoát vòng lặp
            }
        }

        // Báo ra Console nếu đường quá đông không đẻ được xe
        if (!spawned)
        {
            Debug.Log("<color=orange>Đường kẹt quá, xe Bus không có chỗ trống để xuất hiện!</color>");
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