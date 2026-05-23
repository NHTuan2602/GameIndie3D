using UnityEngine;
using System.Collections.Generic;

public class SignGateController : MonoBehaviour
{
    private MeshRenderer[] signBoards = new MeshRenderer[4];
    private GameObject[] deathBlockers = new GameObject[4];

    [Header("Vật liệu Biển báo")]
    public Material safeMaterial;
    public Material[] dangerMaterials;

    void Awake()
    {
        AutoFindComponents();
    }

    public void SetLanePositions(float[] centers)
    {
        if (centers.Length < 4) return;

        // Gọi hàm "Máy hút bụi" để tự động nắn lại toàn bộ 4 cột thẳng tắp theo code
        AlignColumn("Column_0", centers[0]);
        AlignColumn("Column_1", centers[1]);
        AlignColumn("Column_2", centers[2]);
        AlignColumn("Column_3", centers[3]);
    }

    // ==========================================
    // HÀM MỚI: TỰ ĐỘNG FIX LỖI "HỒN LÌA KHỎI XÁC"
    // ==========================================
    void AlignColumn(string colName, float targetWorldX)
    {
        Transform col = FindDeepChild(transform, colName);
        if (col == null) return;

        // 1. Ép tất cả các object con (Biển báo, Cột sắt, Blocker) về thẳng hàng ngay giữa Column
        // Bất chấp việc bạn lỡ tay kéo chúng lệch đi đâu trong cửa sổ Unity
        foreach (Transform child in col)
        {
            child.localPosition = new Vector3(0, child.localPosition.y, child.localPosition.z);
        }

        // 2. Bứng nguyên cái Column (đã gọn gàng) đặt vào đúng tọa độ của Làn đường
        col.position = new Vector3(targetWorldX, col.position.y, col.position.z);
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
        // 1. Chọn ngẫu nhiên 2 làn bất kỳ (từ 0 đến 3) để làm đường sống
        int safeLane1 = Random.Range(0, 4);
        int safeLane2 = safeLane1;

        // Đảm bảo làn sống thứ 2 không bị trùng với làn thứ 1
        while (safeLane2 == safeLane1)
        {
            safeLane2 = Random.Range(0, 4);
        }

        // 2. Dán hình và bật/tắt bẫy
        for (int i = 0; i < 4; i++)
        {
            // Kiểm tra xem làn hiện tại có phải là 1 trong 2 làn sống vừa quay ngẫu nhiên không
            bool isSafeLane = (i == safeLane1 || i == safeLane2);

            if (isSafeLane)
            {
                if (signBoards[i] != null) signBoards[i].material = safeMaterial;
                if (deathBlockers[i] != null) deathBlockers[i].SetActive(false);
            }
            else
            {
                if (signBoards[i] != null && dangerMaterials.Length > 0)
                {
                    signBoards[i].material = dangerMaterials[Random.Range(0, dangerMaterials.Length)];
                }
                if (deathBlockers[i] != null) deathBlockers[i].SetActive(true);
            }
        }
    }
}