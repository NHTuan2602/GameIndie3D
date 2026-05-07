using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Cài đặt Địa hình")]
    public GameObject roadPrefab;
    public Transform player;

    public float chunkLength = 50f;
    public int startChunks = 5;
    public float safeZone = 60f; // BÙA HỘ MỆNH: Giữ đường lại 60m sau lưng xe

    private float spawnZ = 0f;
    private List<GameObject> activeChunks = new List<GameObject>();

    void Start()
    {
        // Trải thảm 5 khúc đường lúc mới vào game
        for (int i = 0; i < startChunks; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        // CHỈ XÓA ĐƯỜNG KHI XE ĐÃ CHẠY VƯỢT QUA KHÚC ĐÓ 60 MÉT
        if (player.position.z - safeZone > activeChunks[0].transform.position.z)
        {
            SpawnChunk();      // Mọc đường mới ở đằng xa
            DeleteOldChunk();  // Xóa đường cũ ở sau lưng
        }
    }

    void SpawnChunk()
    {
        GameObject go = Instantiate(roadPrefab, transform.forward * spawnZ, transform.rotation);
        activeChunks.Add(go);
        spawnZ += chunkLength;
    }

    void DeleteOldChunk()
    {
        Destroy(activeChunks[0]);
        activeChunks.RemoveAt(0);
    }
}