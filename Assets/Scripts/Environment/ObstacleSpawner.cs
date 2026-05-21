using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Tọa độ 4 Làn (BẮT BUỘC NHẬP: -10.5, -3.5, 3.5, 10.5)")]
    public float[] laneCenters = new float[4] { -10.5f, -3.5f, 3.5f, 10.5f };

    [Header("Kho Vật Phẩm (Tĩnh) - Làn Phải")]
    public GameObject[] dangerPrefabs; // Ổ gà
    public GameObject[] buffPrefabs;   // Xe tải làm dốc tăng tốc
    public GameObject brickPrefab;     // Gạch
    public GameObject trafficSignGatePrefab;

    [Header("Kho Hung Thần (Động) - Làn Trái")]
    public GameObject[] highSpeedVehicles; // Xe khách, xe cấp cứu...

    [Header("Kho Quái Xế Lạng Lách (Ninja Lead)")]
    public GameObject recklessBikerPrefab;
    public float spawnDistanceBehind = -40f;
    private float bikerTimer;

    [Header("Cài đặt Cổng Biển Báo (V.I.P)")]
    public float gateInterval = 30f;
    public float gapDuration = 3f;
    public float gateLifeTime = 15f;

    [Header("Cài đặt Đẻ Quái Vật")]
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
    private float safeZoneTimer = 0f;

    void Update()
    {
        globalTimer += Time.deltaTime;

        // 1. HỆ THỐNG CỔNG BIỂN BÁO
        gateTimer += Time.deltaTime;
        if (gateActiveTimer > 0) gateActiveTimer -= Time.deltaTime;
        if (safeZoneTimer > 0) safeZoneTimer -= Time.deltaTime;

        bool isGapTime = (gateInterval - gateTimer) <= gapDuration;

        if (gateTimer >= gateInterval)
        {
            SpawnGate();
            gateTimer = 0f;
            gateActiveTimer = gateLifeTime;
        }

        if (isGapTime) return;

        // 2. HỆ THỐNG XE HUNG THẦN (CHỈ ĐẺ Ở LÀN 0 VÀ 1 - TRÁI)
        if (globalTimer >= startSpawnBusAfter && gateActiveTimer <= 0 && safeZoneTimer <= 0)
        {
            vehicleTimer += Time.deltaTime;
            if (vehicleTimer >= busSpawnInterval)
            {
                SpawnHighSpeedVehicle();
                vehicleTimer = 0f;
            }
        }

        // 3. HỆ THỐNG VẬT CẢN TĨNH (CHỈ ĐẺ Ở LÀN 2 VÀ 3 - PHẢI)
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

        // 4. HỆ THỐNG ĐẺ QUÁI XẾ LẠNG LÁCH (Từ giây 15 trở đi)
        if (globalTimer >= 15f)
        {
            bikerTimer += Time.deltaTime;
            if (bikerTimer >= 5f)
            {
                SpawnRecklessBiker();
                bikerTimer = 0f;
            }
        }
    }

    void SpawnHighSpeedVehicle()
    {
        if (highSpeedVehicles == null || highSpeedVehicles.Length == 0) return;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            GameObject prefabToSpawn = highSpeedVehicles[Random.Range(0, highSpeedVehicles.Length)];

            // ĐÃ FIX: Chỉ lấy tâm của Làn 0 hoặc Làn 1 (2 làn trái)
            float randomLaneX = laneCenters[Random.Range(0, 2)];
            Vector3 checkPos = new Vector3(randomLaneX, 1f, player.position.z + spawnDistanceAhead);

            if (IsPathClear(checkPos))
            {
                float spawnY = prefabToSpawn.transform.position.y;
                Vector3 spawnPos = new Vector3(randomLaneX, spawnY, player.position.z + spawnDistanceAhead);
                GameObject obj = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);
                Destroy(obj, 20f);
                break;
            }
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
                // ĐÃ FIX: Không random bừa bãi nữa, ép vào đúng giữa Làn 2 hoặc Làn 3 (2 làn phải)
                float randomX = laneCenters[Random.Range(2, 4)];
                Vector3 checkPos = new Vector3(randomX, 1f, player.position.z + spawnDistanceAhead);

                if (IsPathClear(checkPos))
                {
                    Spawn(prefabToSpawn, randomX);
                    break;
                }
            }
        }
    }

    void SpawnSingleBrick()
    {
        if (brickPrefab == null) return;
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // ĐÃ FIX: Gạch cũng chỉ nằm gọn gàng ở Làn 2 hoặc 3
            float randomX = laneCenters[Random.Range(2, 4)];
            Vector3 checkPos = new Vector3(randomX, 1f, player.position.z + spawnDistanceAhead);

            if (IsPathClear(checkPos))
            {
                Spawn(brickPrefab, randomX);
                break;
            }
        }
    }

    void SpawnRecklessBiker()
    {
        if (recklessBikerPrefab == null) return;

        // Quái xế lạng lách giữa Làn 2 và 3
        float laneLeftX = laneCenters[2];
        float laneRightX = laneCenters[3];

        Vector3 spawnPos = new Vector3((laneLeftX + laneRightX) / 2f, 1f, player.position.z + spawnDistanceBehind);
        GameObject bikerObj = Instantiate(recklessBikerPrefab, spawnPos, Quaternion.identity);

        RecklessBiker bikerScript = bikerObj.GetComponent<RecklessBiker>();
        if (bikerScript != null) bikerScript.SetupLanes(laneLeftX, laneRightX);
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
            Vector3 spawnPos = new Vector3(0f, trafficSignGatePrefab.transform.position.y, player.position.z + spawnDistanceAhead);
            GameObject gateObj = Instantiate(trafficSignGatePrefab, spawnPos, trafficSignGatePrefab.transform.rotation);
            SignGateController gateScript = gateObj.GetComponent<SignGateController>();

            if (gateScript != null)
            {
                gateScript.SetLanePositions(laneCenters);

                // Ép làn: 50% bắt chạy sang trái (Nguy hiểm), 50% cho ở lại phải (An toàn)
                bool forceLeft = (Random.value > 0.5f);
                gateScript.SetupGateFor4Lanes(forceLeft);

                if (forceLeft) safeZoneTimer = 4f; // Khóa mồm xe ngược chiều 4 giây
            }
            Destroy(gateObj, 15f);
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