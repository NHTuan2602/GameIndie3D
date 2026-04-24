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

    [Header("Cài đặt Sinh sản Vật cản")]
    public Transform player;
    public float spawnDistanceAhead = 150f; // BẮT BUỘC ĐẨY RA XA
    public float spawnInterval = 2f;
    public float brickInterval = 6f;
    [Range(0f, 1f)] public float buffChance = 0.2f;

    [Header("Cài đặt Cổng Biển Báo (THỜI GIAN THỰC)")]
    public float gateInterval = 15f; // Cứ đúng 15 giây đẻ 1 cổng
    public float gapDuration = 3f;   // Dừng đẻ mọi thứ trước 3 giây để tạo quãng nghỉ

    [Header("Cài đặt Radar Chống Trùng Lặp")]
    public int maxSpawnAttempts = 3;
    public Vector3 clearanceBoxSize = new Vector3(1.5f, 1f, 20f);

    private float obstacleTimer;
    private float brickTimer;
    private float gateTimer;

    void Update()
    {
        // 1. ĐỒNG HỒ CỦA CỔNG BIỂN BÁO
        gateTimer += Time.deltaTime;
        bool isGapTime = (gateInterval - gateTimer) <= gapDuration;

        if (gateTimer >= gateInterval)
        {
            SpawnGate();
            gateTimer = 0f;
        }

        // 2. NẾU ĐANG TRONG QUÃNG NGHỈ -> CHẶN ĐỨNG MỌI SINH SẢN KHÁC
        if (isGapTime) return;

        // 3. ĐỒNG HỒ VẬT CẢN
        obstacleTimer += Time.deltaTime;
        if (obstacleTimer >= spawnInterval)
        {
            SpawnRandomObstacle();
            obstacleTimer = 0f;
        }

        // 4. ĐỒNG HỒ GẠCH
        brickTimer += Time.deltaTime;
        if (brickTimer >= brickInterval)
        {
            SpawnSingleBrick();
            brickTimer = 0f;
        }
    }

    void SpawnGate()
    {
        if (trafficSignGatePrefab == null) return;

        // RADAR KIỂM TRA 3 LÀN
        bool allClear = true;
        foreach (float xCenter in laneCenters)
        {
            Vector3 checkPos = new Vector3(xCenter, 1f, player.position.z + spawnDistanceAhead);
            if (!IsPathClear(checkPos))
            {
                allClear = false;
                Debug.Log("<color=yellow>HỦY ĐẺ CỔNG: Phía trước ở làn X=" + xCenter + " đang bị kẹt!</color>");
                break;
            }
        }

        // Nếu 3 làn đều trống -> Tiến hành đẻ cổng
        if (allClear)
        {
            Debug.Log("<color=red>ĐANG ĐẺ CỔNG SINH TỬ!</color>");

            // Đẻ cái vỏ ở X = 0
            Vector3 spawnPos = new Vector3(0f, trafficSignGatePrefab.transform.position.y, player.position.z + spawnDistanceAhead);
            GameObject gateObj = Instantiate(trafficSignGatePrefab, spawnPos, trafficSignGatePrefab.transform.rotation);

            // Bắt cái cổng phải dạt 3 tấm bảng ra tọa độ 0.45, 14, -14
            SignGateController gateScript = gateObj.GetComponent<SignGateController>();
            if (gateScript != null)
            {
                gateScript.SetLanePositions(laneCenters);
            }

            Destroy(gateObj, 15f); // Tự hủy cổng sau 15 giây
        }
    }

    void SpawnRandomObstacle()
    {
        GameObject prefabToSpawn = null;
        if (Random.value < buffChance && buffPrefabs.Length > 0)
        {
            prefabToSpawn = buffPrefabs[Random.Range(0, buffPrefabs.Length)];
        }
        else if (dangerPrefabs.Length > 0)
        {
            prefabToSpawn = dangerPrefabs[Random.Range(0, dangerPrefabs.Length)];
        }

        if (prefabToSpawn != null)
        {
            for (int i = 0; i < maxSpawnAttempts; i++)
            {
                float randomX = Random.Range(roadMinX, roadMaxX);
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
            float randomX = Random.Range(roadMinX, roadMaxX);
            Vector3 checkPos = new Vector3(randomX, 1f, player.position.z + spawnDistanceAhead);

            if (IsPathClear(checkPos))
            {
                Spawn(brickPrefab, randomX);
                break;
            }
        }
    }

    bool IsPathClear(Vector3 spawnPos)
    {
        Collider[] hits = Physics.OverlapBox(spawnPos, clearanceBoxSize, Quaternion.identity);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Obstacle") || hit.CompareTag("Ramp") || hit.CompareTag("Buff") || hit.CompareTag("Brick"))
            {
                return false;
            }
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