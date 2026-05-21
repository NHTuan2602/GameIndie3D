using UnityEngine;
using System.Collections.Generic;

public class SignGateController : MonoBehaviour
{
    private MeshRenderer[] signBoards = new MeshRenderer[4];
    private GameObject[] deathBlockers = new GameObject[4];

    [Header("Vật liệu Biển báo")]
    public Material safeMaterial;
    public Material[] dangerMaterials;

    void Start()
    {
        AutoFindComponents();
    }

    // ĐÃ FIX LỖI "HỒN LÌA KHỎI XÁC": Dời nguyên cái Cột (Column) đi, các phần con sẽ đi theo!
    public void SetLanePositions(float[] centers)
    {
        if (centers.Length < 4) return;

        // Làn 1 (Trái ngoài cùng)
        Transform col0 = FindDeepChild(transform, "Column_0");
        if (col0) col0.position = new Vector3(transform.position.x + centers[0], col0.position.y, col0.position.z);

        // Làn 2 (Trái trong)
        Transform col1 = FindDeepChild(transform, "Column_1");
        if (col1) col1.position = new Vector3(transform.position.x + centers[1], col1.position.y, col1.position.z);

        // Làn 3 (Phải trong)
        Transform col2 = FindDeepChild(transform, "Column_2");
        if (col2) col2.position = new Vector3(transform.position.x + centers[2], col2.position.y, col2.position.z);

        // Làn 4 (Phải ngoài cùng)
        Transform col3 = FindDeepChild(transform, "Column_3");
        if (col3) col3.position = new Vector3(transform.position.x + centers[3], col3.position.y, col3.position.z);
    }

    Transform FindDeepChild(Transform parent, string targetName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(parent);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.name == targetName) return current;
            foreach (Transform child in current) queue.Enqueue(child);
        }
        return null;
    }

    void AutoFindComponents()
    {
        // Radar thò vào tận bên trong để gắp MeshRenderer dán hình và Blocker làm bẫy
        Transform b0 = FindDeepChild(transform, "Board_0");
        if (b0) signBoards[0] = b0.GetComponent<MeshRenderer>();
        Transform blk0 = FindDeepChild(transform, "Blocker_0");
        if (blk0) deathBlockers[0] = blk0.gameObject;

        Transform b1 = FindDeepChild(transform, "Board_1");
        if (b1) signBoards[1] = b1.GetComponent<MeshRenderer>();
        Transform blk1 = FindDeepChild(transform, "Blocker_1");
        if (blk1) deathBlockers[1] = blk1.gameObject;

        Transform b2 = FindDeepChild(transform, "Board_2");
        if (b2) signBoards[2] = b2.GetComponent<MeshRenderer>();
        Transform blk2 = FindDeepChild(transform, "Blocker_2");
        if (blk2) deathBlockers[2] = blk2.gameObject;

        Transform b3 = FindDeepChild(transform, "Board_3");
        if (b3) signBoards[3] = b3.GetComponent<MeshRenderer>();
        Transform blk3 = FindDeepChild(transform, "Blocker_3");
        if (blk3) deathBlockers[3] = blk3.gameObject;
    }

    public void SetupGateFor4Lanes(bool forceSafeOnLeft)
    {
        int safeLaneIndex = 0;

        if (forceSafeOnLeft)
        {
            // Ép người chơi phải lao sang 2 làn trái (ngược chiều)
            safeLaneIndex = Random.Range(0, 2);
        }
        else
        {
            // Cho phép người chơi ở 2 làn phải (cùng chiều)
            safeLaneIndex = Random.Range(2, 4);
        }

        // Đóng/Mở đường và Dán hình
        for (int i = 0; i < 4; i++)
        {
            if (i == safeLaneIndex)
            {
                if (signBoards[i] != null) signBoards[i].material = safeMaterial;
                if (deathBlockers[i] != null) deathBlockers[i].SetActive(false); // Đường sống (Tắt tường tàng hình)
            }
            else
            {
                if (signBoards[i] != null && dangerMaterials.Length > 0)
                    signBoards[i].material = dangerMaterials[Random.Range(0, dangerMaterials.Length)];
                if (deathBlockers[i] != null) deathBlockers[i].SetActive(true); // Đường chết (Bật tường tàng hình lên)
            }
        }
    }
}