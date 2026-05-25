using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Tọa độ 4 Làn (BẮT BUỘC NHẬP: -23.65, -11.99, -0.6, 11.2)")]
    public float[] laneCenters = new float[4];

    [Header("Kho Xe Ngược Chiều")]
    public GameObject[] leftVehicles;
    public float startSpawnLeftAfter = 10f;
    public float leftSpawnInterval = 2f;

    [Header("Kho Xe Cùng Chiều")]
    public GameObject[] rightVehicles;
    public float rightSpawnInterval = 2f;

    [Header("Kho Vật Phẩm Đặc Biệt (CẢ 4 LÀN)")]
    public GameObject[] specialItems;
    public float specialSpawnInterval = 5f;

    [Header("Kho Quái Xế Lạng Lách")]
    public GameObject recklessBikerPrefab;
    public float spawnDistanceBehind = -40f;

    [Header("Cài đặt Cổng Biển Báo (V.I.P)")]
    public GameObject trafficSignGatePrefab;
    public float gateInterval = 30f;
    public float gapDuration = 3f;
    public float gateLifeTime = 15f;

    [Header("Cài đặt Cốt lõi")]
    public Transform player;
    public float spawnDistanceAhead = 150f;
    public int maxSpawnAttempts = 3;
    public Vector3 clearanceBoxSize = new Vector3(1.5f, 1f, 20f);

    // --- Các bộ đếm thời gian (Timers) ---
    private float globalTimer = 0f;
    private float gateTimer;
    private float gateActiveTimer;
    private float safeZoneTimer = 0f;
    private float leftTimer;
    private float rightTimer;
    private float specialTimer;
    private float bikerTimer;

    // --- CƠ CHẾ ĐẢO CHIỀU MỚI ---
    [Header("Trạng thái Đảo Chiều Giao Thông")]
    public bool isReversed = false; // False = Bình thường, True = Đã đảo chiều
    private float transitionPauseTimer = 0f; // Thời gian "Nín đẻ" để dọn đường khi vừa đảo chiều

    void Update()
    {
        globalTimer += Time.deltaTime;

        // Giảm thời gian "nín đẻ" dọn đường
        if (transitionPauseTimer > 0) transitionPauseTimer -= Time.deltaTime;

        gateTimer += Time.deltaTime;
        if (gateActiveTimer > 0) gateActiveTimer -= Time.deltaTime;
        if (safeZoneTimer > 0) safeZoneTimer -= Time.deltaTime;

        bool isGapTime = (gateInterval - gateTimer) <= gapDuration;

        // 1. SINH CỔNG BIỂN BÁO
        if (gateTimer >= gateInterval)
        {
            SpawnGate();
            gateTimer = 0f;
            gateActiveTimer = gateLifeTime;
        }

        // Nếu đang trong thời gian nín đẻ chờ xe cũ đi qua HOẶC sắp có cổng thì không đẻ gì cả
        if (isGapTime || transitionPauseTimer > 0) return;

        // 2 & 3. XÁC ĐỊNH LÀN NÀO LÀ NGƯỢC CHIỀU / CÙNG CHIỀU DỰA VÀO TRẠNG THÁI isReversed
        int oncomingMinLane = isReversed ? 2 : 0;
        int oncomingMaxLane = isReversed ? 4 : 2;

        int samedirMinLane = isReversed ? 0 : 2;
        int samedirMaxLane = isReversed ? 2 : 4;

        // ĐẺ XE NGƯỢC CHIỀU
        if (globalTimer >= startSpawnLeftAfter && gateActiveTimer <= 0 && safeZoneTimer <= 0)
        {
            leftTimer += Time.deltaTime;
            if (leftTimer >= leftSpawnInterval)
            {
                SpawnVehicle(leftVehicles, oncomingMinLane, oncomingMaxLane);
                leftTimer = 0f;
            }
        }

        // ĐẺ XE CÙNG CHIỀU
        rightTimer += Time.deltaTime;
        if (rightTimer >= rightSpawnInterval)
        {
            SpawnVehicle(rightVehicles, samedirMinLane, samedirMaxLane);
            rightTimer = 0f;
        }

        // 4. VẬT PHẨM ĐẶC BIỆT (Luôn đẻ cả 4 làn)
        specialTimer += Time.deltaTime;
        if (specialTimer >= specialSpawnInterval)
        {
            SpawnVehicle(specialItems, 0, 4);
            specialTimer = 0f;
        }

        // 5. QUÁI XẾ LẠNG LÁCH (Luôn đi ở làn Cùng Chiều)
        if (globalTimer >= 15f)
        {
            bikerTimer += Time.deltaTime;
            if (bikerTimer >= 5f)
            {
                SpawnRecklessBiker(samedirMinLane, samedirMaxLane);
                bikerTimer = 0f;
            }
        }
    }

    void SpawnVehicle(GameObject[] prefabArray, int minLane, int maxLane)
    {
        if (prefabArray == null || prefabArray.Length == 0) return;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            GameObject prefabToSpawn = prefabArray[Random.Range(0, prefabArray.Length)];
            float randomLaneX = laneCenters[Random.Range(minLane, maxLane)];
            Vector3 checkPos = new Vector3(randomLaneX, 1f, player.position.z + spawnDistanceAhead);

            if (IsPathClear(checkPos))
            {
                float spawnY = prefabToSpawn.transform.position.y;
                Vector3 spawnPos = new Vector3(randomLaneX, spawnY, player.position.z + spawnDistanceAhead);
                GameObject obj = Instantiate(prefabToSpawn, spawnPos, prefabToSpawn.transform.rotation);
                Destroy(obj, 40f);
                break;
            }
        }
    }

    void SpawnRecklessBiker(int minLane, int maxLane)
    {
        if (recklessBikerPrefab == null) return;

        // Quái xế lạng lách giữa 2 làn CÙNG CHIỀU hiện tại
        float laneLeftX = laneCenters[minLane];
        float laneRightX = laneCenters[maxLane - 1];

        Vector3 spawnPos = new Vector3((laneLeftX + laneRightX) / 2f, 1f, player.position.z + spawnDistanceBehind);
        GameObject bikerObj = Instantiate(recklessBikerPrefab, spawnPos, recklessBikerPrefab.transform.rotation);

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

                // Random xem có ép người chơi sang Làn Ngược Chiều hay không
                bool forceOncoming = (Random.value > 0.5f);

                // Truyền vào Gate dựa theo trạng thái đảo chiều hiện tại
                bool forceLeft = isReversed ? !forceOncoming : forceOncoming;
                gateScript.SetupGateFor4Lanes(forceLeft);

                // Nếu ép sang làn ngược chiều, khóa họng xe ngược chiều 4 giây
                if (forceOncoming) safeZoneTimer = 4f;
            }
            Destroy(gateObj, 30f);

            // BẮT ĐẦU ĐẾM NGƯỢC 5 GIÂY ĐỂ ĐẢO CHIỀU GIAO THÔNG
            StartCoroutine(SwapTrafficRoutine(5f));
        }
    }

    IEnumerator SwapTrafficRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Kích hoạt đảo chiều
        isReversed = !isReversed;
        Debug.Log("<color=red>CẢNH BÁO: ĐÃ ĐẢO CHIỀU GIAO THÔNG!</color>");

        // Tạm "nín đẻ" 2 giây để xe cũ đi khuất, tránh xe mới tông xe cũ
        transitionPauseTimer = 2f;
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
        if (player != null && laneCenters.Length >= 4)
        {
            Vector3 checkPos = new Vector3(laneCenters[2], 1f, player.position.z + spawnDistanceAhead);
            Gizmos.DrawCube(checkPos, clearanceBoxSize * 2);
        }
    }
}