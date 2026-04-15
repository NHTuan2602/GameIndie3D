using UnityEngine;

public enum SignType
{
    ChiDuocDiLanTrai, // Làn 1 Sống, 2-3 Chết
    ChiDuocDiLanGiua, // Làn 3 Sống, 1-2 Chết
    ChiDuocDiLanPhai  // Làn 2 Sống, 1-3 Chết
}

public class TrafficSignGate : MonoBehaviour
{
    [Header("Giao diện Biển Báo")]
    // Ném 3 cái hình biển báo (Material hoặc Texture) vào đây
    public Material matGoLeft;
    public Material matGoMid;
    public float lane1X = -10f; // Tâm làn trái
    public float lane2X = 45f;  // Tâm làn phải
    public float lane3X = 20f;  // Tâm làn giữa
    public Material matGoRight;
    public MeshRenderer signBoardRenderer; // Tấm bảng hiển thị

    [Header("Bẫy Tử Thần (Death Triggers)")]
    // Kéo 3 cục bẫy InstantDeath tương ứng ở 3 làn vào đây
    public GameObject deathTrapLane1; // Trái
    public GameObject deathTrapLane3; // Giữa
    public GameObject deathTrapLane2; // Phải

    private SignType currentRule;

    void Start()
    {
        GenerateRandomSign();
    }

    void GenerateRandomSign()
    {
        // 1. Random luật chơi
        int randomRule = Random.Range(0, 3);
        currentRule = (SignType)randomRule;

        // 2. Bật tất cả bẫy lên (Mặc định cả 3 đường đều chết)
        deathTrapLane1.SetActive(true);
        deathTrapLane3.SetActive(true);
        deathTrapLane2.SetActive(true);

        // 3. Tắt bẫy ở làn ĐÚNG, và đổi hình biển báo
        switch (currentRule)
        {
            case SignType.ChiDuocDiLanTrai:
                deathTrapLane1.SetActive(false); // Làn 1 an toàn
                signBoardRenderer.material = matGoLeft;
                break;

            case SignType.ChiDuocDiLanGiua:
                deathTrapLane3.SetActive(false); // Làn giữa an toàn
                signBoardRenderer.material = matGoMid;
                break;

            case SignType.ChiDuocDiLanPhai:
                deathTrapLane2.SetActive(false); // Làn phải an toàn
                signBoardRenderer.material = matGoRight;
                break;
        }
    }
}