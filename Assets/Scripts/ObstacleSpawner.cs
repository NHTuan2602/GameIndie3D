using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Tọa độ Mặt Đường")]
    public float roadMinX = -4.5f;
    public float roadMaxX = 4.5f;

    [Header("Tọa độ 3 Làn (CHỈ DÙNG CHO BIỂN BÁO)")]
    public float[] laneCenters = new float[3] { -3f, 0f, 3f };

    [Header("Kho Vật Phẩm (Prefabs)")]
    public GameObject[] dangerPrefabs;
    public GameObject[] buffPrefabs;
    public GameObject brickPrefab;
    public GameObject trafficSignGatePrefab; // NHỚ KÉO PREFAB CỔNG VÀO ĐÂY Ở INSPECTOR!

    [Header("Cài đặt Sinh sản Vật cản")]
    public Transform player;
    public float spawnDistanceAhead = 150f;
    public float spawnInterval = 2f;
    public float brickInterval = 6f;
    [Range(0f, 1f)] public float buffChance = 0.2f;

    [Header("Cài đặt Cổng Biển Báo (THỜI GIAN THỰC)")]
    public float gateInterval = 15f; // Cứ đúng 15 giây đẻ 1 cổng
    public float gapDuration = 3f;   // Dừng đẻ mọi thứ trước 3 giây để tạo quãng nghỉ

    [Header("Cài đặt Radar Chống Trùng Lặp")]
    public int maxSpawnAttempts = 3;
    public Vector3 clearanceBoxSize = new Vector3(1.5f, 1f, 20f);

    // 3 Đồng hồ bấm giờ độc lập
    private float obstacleTimer;
    private float brickTimer;
    private float gateTimer;

    void Update()
    {
        // 1. ĐỒNG HỒ CỦA CỔNG BIỂN BÁO (Chạy liên tục không ngừng)
        gateTimer += Time.deltaTime;

        // Tính toán xem có đang rơi vào "Quãng nghỉ" (3 giây trước khi đẻ cổng) không?
        bool isGapTime = (gateInterval - gateTimer) <= gapDuration;

        // Nếu đủ 15 giây -> Đẻ Cổng
        if (gateTimer >= gateInterval)
        {
            SpawnGate();
            gateTimer = 0f; // Reset đồng hồ cổng về 0
        }

        // 2. NẾU ĐANG TRONG QUÃNG NGHỈ -> KHÔNG LÀM GÌ CẢ (Chặn đứng mọi sinh sản khác)
        if (isGapTime)
        {
            return; // Thoát hàm Update luôn, không cho đồng hồ vật cản chạy tiếp
        }

        // 3. ĐỒNG HỒ VẬT CẢN (Chỉ chạy khi KHÔNG có quãng nghỉ)
        obstacleTimer += Time.deltaTime;
        if (obstacleTimer >= spawnInterval)
        {
            SpawnRandomObstacle();
            obstacleTimer = 0f;
        }

        // 4. ĐỒNG HỒ GẠCH (Chỉ chạy khi KHÔNG có quãng nghỉ)
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

        Debug.Log("<color=red>ĐANG ĐẺ CỔNG SINH TỬ!</color>");

        // 1. Cố định đẻ Cổng ở tâm bản đồ (X = 0) để làm mốc neo chuẩn
        Vector3 spawnPos = new Vector3(0f, trafficSignGatePrefab.transform.position.y, player.position.z + spawnDistanceAhead);
        GameObject gateObj = Instantiate(trafficSignGatePrefab, spawnPos, trafficSignGatePrefab.transform.rotation);

        // 2. Ném 3 tọa độ của bạn (0.45, 14, -14) sang cho Cổng tự bung ra
        SignGateController gateScript = gateObj.GetComponent<SignGateController>();
        if (gateScript != null)
        {
            gateScript.SetLanePositions(laneCenters);
        }

        Destroy(gateObj, 15f);
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

        Destroy(obj, 15f); // Tự hủy sau 15s để chống lag
    }
}