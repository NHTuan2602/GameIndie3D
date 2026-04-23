using UnityEngine;
using System.Collections.Generic;

public class SignGateController : MonoBehaviour
{
    private MeshRenderer[] signBoards = new MeshRenderer[3];
    private GameObject[] deathBlockers = new GameObject[3];

    [Header("Vật liệu Biển báo (CHỈ CẦN KÉO 2 CÁI NÀY)")]
    public Material safeMaterial;
    public Material[] dangerMaterials;

    void Start()
    {
        AutoFindComponents();
        SetupGate();
    }

    // ==============================================
    // 1. HÀM FIX LỖI CS1061: Xếp vị trí theo ObstacleSpawner
    // ==============================================
    public void SetLanePositions(float[] centers)
    {
        if (centers.Length < 3) return;

        // Dùng vị trí thế giới (World Position) để bất chấp việc bạn giấu bảng trong bao nhiêu thư mục con
        Transform leftBoard = FindDeepChild(transform, "Board_Left");
        Transform leftBlocker = FindDeepChild(transform, "Blocker_Left");
        if (leftBoard) leftBoard.position = new Vector3(transform.position.x + centers[0], leftBoard.position.y, leftBoard.position.z);
        if (leftBlocker) leftBlocker.position = new Vector3(transform.position.x + centers[0], leftBlocker.position.y, leftBlocker.position.z);

        Transform centerBoard = FindDeepChild(transform, "Board_Center");
        Transform centerBlocker = FindDeepChild(transform, "Blocker_Center");
        if (centerBoard) centerBoard.position = new Vector3(transform.position.x + centers[1], centerBoard.position.y, centerBoard.position.z);
        if (centerBlocker) centerBlocker.position = new Vector3(transform.position.x + centers[1], centerBlocker.position.y, centerBlocker.position.z);

        Transform rightBoard = FindDeepChild(transform, "Board_Right");
        Transform rightBlocker = FindDeepChild(transform, "Blocker_Right");
        if (rightBoard) rightBoard.position = new Vector3(transform.position.x + centers[2], rightBoard.position.y, rightBoard.position.z);
        if (rightBlocker) rightBlocker.position = new Vector3(transform.position.x + centers[2], rightBlocker.position.y, rightBlocker.position.z);
    }

    // ==============================================
    // 2. RADAR XUYÊN TƯỜNG (Tìm kiếm mọi ngóc ngách)
    // ==============================================
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
        Transform leftBoard = FindDeepChild(transform, "Board_Left");
        if (leftBoard) signBoards[0] = leftBoard.GetComponent<MeshRenderer>();
        Transform leftBlocker = FindDeepChild(transform, "Blocker_Left");
        if (leftBlocker) deathBlockers[0] = leftBlocker.gameObject;

        Transform centerBoard = FindDeepChild(transform, "Board_Center");
        if (centerBoard) signBoards[1] = centerBoard.GetComponent<MeshRenderer>();
        Transform centerBlocker = FindDeepChild(transform, "Blocker_Center");
        if (centerBlocker) deathBlockers[1] = centerBlocker.gameObject;

        Transform rightBoard = FindDeepChild(transform, "Board_Right");
        if (rightBoard) signBoards[2] = rightBoard.GetComponent<MeshRenderer>();
        Transform rightBlocker = FindDeepChild(transform, "Blocker_Right");
        if (rightBlocker) deathBlockers[2] = rightBlocker.gameObject;
    }

    void SetupGate()
    {
        int safeLaneIndex = Random.Range(0, 3);

        for (int i = 0; i < 3; i++)
        {
            if (i == safeLaneIndex)
            {
                // LÀN AN TOÀN
                if (signBoards[i] != null) signBoards[i].material = safeMaterial;
                if (deathBlockers[i] != null) deathBlockers[i].SetActive(false);
            }
            else
            {
                // LÀN NGUY HIỂM
                if (signBoards[i] != null && dangerMaterials.Length > 0)
                {
                    signBoards[i].material = dangerMaterials[Random.Range(0, dangerMaterials.Length)];
                }
                if (deathBlockers[i] != null) deathBlockers[i].SetActive(true);
            }
        }
    }
}