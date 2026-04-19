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
    public GameObject trafficSignGatePrefab;

    [Header("Cài đặt Sinh sản")]
    public Transform player;
    public float spawnDistanceAhead = 80f;
    public float spawnInterval = 2f;
    public float brickInterval = 6f;
    [Range(0f, 1f)] public float buffChance = 0.2f;

    [Header("Cài đặt Radar Chống Trùng Lặp")]
    public int maxSpawnAttempts = 3; // Số lần thử tìm chỗ trống tối đa
    // Hộp radar quét: Rộng 3m, Cao 2m, Dài 40m (20f x 2)
    public Vector3 clearanceBoxSize = new Vector3(1.5f, 1f, 20f);

    private float obstacleTimer;
    private float brickTimer;
    private int spawnCount = 0;

    void Update()
    {
        obstacleTimer += Time.deltaTime;
        if (obstacleTimer >= spawnInterval)
        {
            SpawnObstacleRow();
            obstacleTimer = 0f;
        }

        brickTimer += Time.deltaTime;
        if (brickTimer >= brickInterval)
        {
            SpawnSingleBrick();
            brickTimer = 0f;
        }
    }

    void SpawnObstacleRow()
    {
        spawnCount++;

        // 1. BIỂN BÁO (Đẻ ở giữa đường)
        if (spawnCount % 5 == 0 && trafficSignGatePrefab != null)
        {
            float gateX = laneCenters[1];
            Spawn(trafficSignGatePrefab, gateX);
            return;
        }

        // 2. CHƯỚNG NGẠI VẬT & DỐC (Có dùng Radar dò đường)
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
            // CỐ GẮNG TÌM MỘT CHỖ TRỐNG (Tối đa 3 lần thử)
            for (int i = 0; i < maxSpawnAttempts; i++)
            {
                float randomX = Random.Range(roadMinX, roadMaxX);
                Vector3 checkPos = new Vector3(randomX, 1f, player.position.z + spawnDistanceAhead);

                if (IsPathClear(checkPos))
                {
                    Spawn(prefabToSpawn, randomX);
                    break; // Đã tìm thấy chỗ an toàn và đẻ xong -> Thoát vòng lặp
                }
                // Nếu không Clear, vòng lặp sẽ tự động quay lại bốc số randomX khác!
            }
        }
    }

    void SpawnSingleBrick()
    {
        // Gạch cũng quét Radar để không bị kẹt lấp ló sau đuôi xe địch
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

    // ================= BỘ NÃO RADAR QUÉT KHÔNG GIAN =================
    bool IsPathClear(Vector3 spawnPos)
    {
        // Tung ra một cái hộp tàng hình tại vị trí chuẩn bị đẻ
        Collider[] hits = Physics.OverlapBox(spawnPos, clearanceBoxSize, Quaternion.identity);

        foreach (Collider hit in hits)
        {
            // Nếu phát hiện có vật thể nào mang 3 Tag này đang nằm trong hộp
            if (hit.CompareTag("Obstacle") || hit.CompareTag("Ramp") || hit.CompareTag("Buff") || hit.CompareTag("Brick"))
            {
                return false; // Báo động: Đường này đang bị kẹt!
            }
        }
        return true; // Đường quang mây tạnh, cho đẻ!
    }

    // Vẽ hình cái hộp Radar ra màn hình Scene để bạn dễ căn chỉnh độ dài
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