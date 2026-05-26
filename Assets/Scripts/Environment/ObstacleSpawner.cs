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
    [Range(0f, 100f)] public float leftSpawnChance = 70f;

    [Header("Kho Xe Cùng Chiều")]
    public GameObject[] rightVehicles;
    public float rightSpawnInterval = 2f;
    [Range(0f, 100f)] public float rightSpawnChance = 60f;

    [Header("Kho Vật Phẩm Đặc Biệt")]
    public GameObject[] specialItems;
    public float specialSpawnInterval = 5f;

    [Header("Kho Quái Xế Lạng Lách")]
    public GameObject recklessBikerPrefab;
    public float spawnDistanceBehind = -40f;
    public float bikerSpawnInterval = 8f; // Cứ đúng 8 giây sẽ xuất hiện hỗ trợ tạo kịch tính

    [Header("Cài đặt Cổng Biển Báo (V.I.P)")]
    public GameObject trafficSignGatePrefab;
    public float gateInterval = 30f;
    public float gapDuration = 3f;
    public float gateLifeTime = 15f;

    [Header("Cài đặt Cốt lõi")]
    public Transform player;
    public float spawnDistanceAhead = 300f;
    public int maxSpawnAttempts = 3;
    public Vector3 clearanceBoxSize = new Vector3(1.5f, 1f, 20f);

    private float globalTimer = 0f;
    private float gateTimer;
    private float gateActiveTimer;
    private float safeZoneTimer = 0f;
    private float leftTimer;
    private float rightTimer;
    private float specialTimer;
    private float bikerTimer;

    public bool isReversed = false;
    private float transitionPauseTimer = 0f;

    void Update()
    {
        // ----------------------------------------------------------------------
        // TẤT CẢ BỘ ĐẾM THỜI GIAN PHẢI ĐẶT Ở ĐÂY ĐỂ KHÔNG BỊ BLOCK BỞI LỆNH RETURN EARLY
        // ----------------------------------------------------------------------
        globalTimer += Time.deltaTime;
        gateTimer += Time.deltaTime;

        if (transitionPauseTimer > 0) transitionPauseTimer -= Time.deltaTime;
        if (gateActiveTimer > 0) gateActiveTimer -= Time.deltaTime;
        if (safeZoneTimer > 0) safeZoneTimer -= Time.deltaTime;

        // Cộng dồn thời gian đẻ xe máy liên tục không ngừng nghỉ
        if (globalTimer >= 15f)
        {
            bikerTimer += Time.deltaTime;
        }
        // ----------------------------------------------------------------------

        // Kiểm tra xem có đang trong thời gian chuẩn bị đẻ Cổng không
        bool isGapTime = (gateInterval - gateTimer) <= gapDuration;

        if (gateTimer >= gateInterval)
        {
            SpawnGate();
            gateTimer = 0f;
            gateActiveTimer = gateLifeTime;
        }

        // HỆ THỐNG GỌI QUÁI XẾ LẠNG LÁCH (Được đưa lên TRƯỚC lệnh return chặn đường)
        if (bikerTimer >= bikerSpawnInterval)
        {
            int currentSamedirMinLane = isReversed ? 0 : 2;
            int currentSamedirMaxLane = isReversed ? 2 : 4;

            SpawnRecklessBiker(currentSamedirMinLane, currentSamedirMaxLane);
            bikerTimer = 0f; // Khởi động lại đồng hồ đếm ngược ngay lập tức
        }

        // Các lệnh return chặn đường bây giờ chỉ có tác dụng với xe lưu thông thông thường
        if (isGapTime || transitionPauseTimer > 0) return;

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
                if (Random.Range(0f, 100f) <= leftSpawnChance)
                {
                    SpawnVehicle(leftVehicles, oncomingMinLane, oncomingMaxLane);
                }
                leftTimer = 0f;
            }
        }

        // ĐẺ XE CÙNG CHIỀU
        rightTimer += Time.deltaTime;
        if (rightTimer >= rightSpawnInterval)
        {
            if (Random.Range(0f, 100f) <= rightSpawnChance)
            {
                SpawnVehicle(rightVehicles, samedirMinLane, samedirMaxLane);
            }
            rightTimer = 0f;
        }

        // VẬT PHẨM ĐẶC BIỆT
        specialTimer += Time.deltaTime;
        if (specialTimer >= specialSpawnInterval)
        {
            SpawnVehicle(specialItems, 0, 4);
            specialTimer = 0f;
        }
    }

    void SpawnRecklessBiker(int minLane, int maxLane)
    {
        if (recklessBikerPrefab == null || player == null) return;

        float laneLeftX = laneCenters[minLane];
        float laneRightX = laneCenters[maxLane - 1];
        Vector3 spawnPos = new Vector3((laneLeftX + laneRightX) / 2f, 1f, player.position.z + spawnDistanceBehind);

        // KIỂM TRA CHỐNG CHẾT YỂU: Nếu tại điểm xuất phát đang bị kẹt xe tải, dịch lùi xe máy ra sau thêm 15 mét nữa
        if (!IsPathClearForBiker(spawnPos))
        {
            spawnPos.z -= 15f;
        }

        GameObject bikerObj = Instantiate(recklessBikerPrefab, spawnPos, recklessBikerPrefab.transform.rotation);

        RecklessBiker bikerScript = bikerObj.GetComponent<RecklessBiker>();
        if (bikerScript != null)
        {
            bikerScript.SetupLanes(laneLeftX, laneRightX);
        }

        Destroy(bikerObj, 40f);
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
                Destroy(obj, 60f);
                break;
            }
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
            Vector3 spawnPos = new Vector3(0f, trafficSignGatePrefab.transform.position.y, player.position.z + spawnDistanceAhead);
            GameObject gateObj = Instantiate(trafficSignGatePrefab, spawnPos, trafficSignGatePrefab.transform.rotation);
            SignGateController gateScript = gateObj.GetComponent<SignGateController>();

            if (gateScript != null)
            {
                gateScript.SetLanePositions(laneCenters);
                bool forceOncoming = (Random.value > 0.5f);
                bool forceLeft = isReversed ? !forceOncoming : forceOncoming;
                gateScript.SetupGateFor4Lanes(forceLeft);
                if (forceOncoming) safeZoneTimer = 4f;
            }
            Destroy(gateObj, 40f);
            StartCoroutine(SwapTrafficRoutine(5f));
        }
    }

    IEnumerator SwapTrafficRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        isReversed = !isReversed;
        Debug.Log("<color=red>CẢNH BÁO: ĐÃ ĐẢO CHIỀU GIAO THÔNG!</color>");

        ClearOldTraffic();
        transitionPauseTimer = 4f;
    }

    void ClearOldTraffic()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obs in obstacles)
        {
            if (obs.transform.position.z > player.position.z + 100f)
            {
                Destroy(obs);
            }
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

    // Hàm kiểm tra riêng biệt cho xe máy ở phía sau lưng
    bool IsPathClearForBiker(Vector3 spawnPos)
    {
        Collider[] hits = Physics.OverlapBox(spawnPos, new Vector3(3f, 1f, 10f), Quaternion.identity);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Obstacle")) return false;
        }
        return true;
    }
}