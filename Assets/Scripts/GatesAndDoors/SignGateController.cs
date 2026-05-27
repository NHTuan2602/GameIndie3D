using UnityEngine;
using System.Collections.Generic;

public class SignGateController : MonoBehaviour
{
    private MeshRenderer[] signBoards = new MeshRenderer[4];
    private GameObject[] deathBlockers = new GameObject[4];

    // MẢNG MỚI: Quản lý 4 tấm ảnh in dưới mặt đường
    private MeshRenderer[] roadDecals = new MeshRenderer[4];

    [Header("Vật liệu Biển báo (Trên cao)")]
    public Material safeMaterial;
    public Material[] dangerMaterials;

    [Header("Vật liệu Mặt đường (Dưới đất)")]
    public Material safeRoadMaterial;   // Kéo Material Mũi tên xanh vào đây
    public Material dangerRoadMaterial; // Kéo Material Dấu X đỏ vào đây

    void Awake()
    {
        AutoFindComponents();
    }

    public void SetLanePositions(float[] centers)
    {
        if (centers.Length < 4) return;

        AlignColumn("Column_0", centers[0]);
        AlignColumn("Column_1", centers[1]);
        AlignColumn("Column_2", centers[2]);
        AlignColumn("Column_3", centers[3]);
    }

    void AlignColumn(string colName, float targetWorldX)
    {
        Transform col = FindDeepChild(transform, colName);
        if (col == null) return;

        foreach (Transform child in col)
        {
            child.localPosition = new Vector3(0, child.localPosition.y, child.localPosition.z);
        }

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
        // Quét tìm Làn 0
        Transform b0 = FindDeepChild(transform, "Board_0");
        if (b0) signBoards[0] = b0.GetComponent<MeshRenderer>();
        Transform blk0 = FindDeepChild(transform, "Blocker_0");
        if (blk0) deathBlockers[0] = blk0.gameObject;
        Transform decal0 = FindDeepChild(transform, "RoadDecal_0");
        if (decal0) roadDecals[0] = decal0.GetComponent<MeshRenderer>();

        // Quét tìm Làn 1
        Transform b1 = FindDeepChild(transform, "Board_1");
        if (b1) signBoards[1] = b1.GetComponent<MeshRenderer>();
        Transform blk1 = FindDeepChild(transform, "Blocker_1");
        if (blk1) deathBlockers[1] = blk1.gameObject;
        Transform decal1 = FindDeepChild(transform, "RoadDecal_1");
        if (decal1) roadDecals[1] = decal1.GetComponent<MeshRenderer>();

        // Quét tìm Làn 2
        Transform b2 = FindDeepChild(transform, "Board_2");
        if (b2) signBoards[2] = b2.GetComponent<MeshRenderer>();
        Transform blk2 = FindDeepChild(transform, "Blocker_2");
        if (blk2) deathBlockers[2] = blk2.gameObject;
        Transform decal2 = FindDeepChild(transform, "RoadDecal_2");
        if (decal2) roadDecals[2] = decal2.GetComponent<MeshRenderer>();

        // Quét tìm Làn 3
        Transform b3 = FindDeepChild(transform, "Board_3");
        if (b3) signBoards[3] = b3.GetComponent<MeshRenderer>();
        Transform blk3 = FindDeepChild(transform, "Blocker_3");
        if (blk3) deathBlockers[3] = blk3.gameObject;
        Transform decal3 = FindDeepChild(transform, "RoadDecal_3");
        if (decal3) roadDecals[3] = decal3.GetComponent<MeshRenderer>();
    }

    public void SetupGateFor4Lanes(bool forceSafeOnLeft)
    {
        int safeLane1 = Random.Range(0, 4);
        int safeLane2 = safeLane1;

        while (safeLane2 == safeLane1)
        {
            safeLane2 = Random.Range(0, 4);
        }

        for (int i = 0; i < 4; i++)
        {
            bool isSafeLane = (i == safeLane1 || i == safeLane2);

            if (isSafeLane)
            {
                // Xử lý Biển trên cao
                if (signBoards[i] != null) signBoards[i].material = safeMaterial;
                if (deathBlockers[i] != null) deathBlockers[i].SetActive(false);

                // Xử lý Ảnh dưới mặt đường
                if (roadDecals[i] != null && safeRoadMaterial != null)
                    roadDecals[i].material = safeRoadMaterial;
            }
            else
            {
                // Xử lý Biển trên cao
                if (signBoards[i] != null && dangerMaterials.Length > 0)
                {
                    signBoards[i].material = dangerMaterials[Random.Range(0, dangerMaterials.Length)];
                }
                if (deathBlockers[i] != null) deathBlockers[i].SetActive(true);

                // Xử lý Ảnh dưới mặt đường
                if (roadDecals[i] != null && dangerRoadMaterial != null)
                    roadDecals[i].material = dangerRoadMaterial;
            }
        }
    }
}