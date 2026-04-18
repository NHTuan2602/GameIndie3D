using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Tọa độ Tâm 3 Làn")]
    public float[] laneCenters = new float[3] { -3f, 0f, 3f };
    public float laneWidth = 2.5f; // Độ rộng của một làn đường để tính toán độ lệch

    [Header("Kho Vật Phẩm (Prefabs)")]
    public GameObject[] dangerPrefabs;
    public GameObject[] buffPrefabs;   // Xe tải dốc nằm ở đây
    public GameObject brickPrefab;

    [Header("Cài đặt Sinh sản")]
    public Transform player;
    public float spawnDistanceAhead = 80f;
    public float spawnInterval = 2f;    // Nhịp sinh vật cản (xe/dốc)
    public float brickInterval = 6f;    // Cứ 6 giây sinh 1 cục gạch

    [Range(0f, 1f)] public float buffChance = 0.2f; // Thêm lại: 20% tỉ lệ ra xe tải dốc thay vì xe địch

    private float obstacleTimer;
    private float brickTimer;

    void Update()
    {
        // 1. Quản lý nhịp sinh vật cản (Xe địch, ổ gà, HOẶC dốc)
        obstacleTimer += Time.deltaTime;
        if (obstacleTimer >= spawnInterval)
        {
            SpawnObstacleRow();
            obstacleTimer = 0f;
        }

        // 2. Quản lý nhịp sinh gạch riêng biệt
        brickTimer += Time.deltaTime;
        if (brickTimer >= brickInterval)
        {
            SpawnSingleBrick();
            brickTimer = 0f;
        }
    }

    void SpawnObstacleRow()
    {
        int laneIndex = Random.Range(0, 3);
        float randomOffset = Random.Range(-laneWidth * 0.2f, laneWidth * 0.2f);
        float spawnX = laneCenters[laneIndex] + randomOffset;

        GameObject prefabToSpawn = null;

        // BỐC THĂM: Xem nhịp này đẻ ra Xe Tải Dốc hay Xe VinFast?
        if (Random.value < buffChance && buffPrefabs.Length > 0)
        {
            // Trúng số 20% -> Thả xe tải dốc
            prefabToSpawn = buffPrefabs[Random.Range(0, buffPrefabs.Length)];
        }
        else if (dangerPrefabs.Length > 0)
        {
            // Trượt (80%) -> Thả xe địch hoặc ổ gà
            prefabToSpawn = dangerPrefabs[Random.Range(0, dangerPrefabs.Length)];
        }

        Spawn(prefabToSpawn, spawnX);
    }

    void SpawnSingleBrick()
    {
        int laneIndex = Random.Range(0, 3);
        float spawnX = laneCenters[laneIndex]; // Gạch vẫn nằm giữa làn
        Spawn(brickPrefab, spawnX);
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