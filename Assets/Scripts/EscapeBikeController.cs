using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EscapeBikeController : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float forwardSpeed = 40f; // Tốc độ tiến thẳng
    public float steerSpeed = 30f;   // Tốc độ lách qua lại
    public float gravity = -15f;

    [Header("Radar Định vị Làn đường (Chuẩn Mới)")]
    // KÉO 2 BIẾN NÀY ĐỂ VẠCH ĐỎ KHỚP VỚI VẠCH VÀNG TRÊN SCENE
    public float leftLaneBoundary = -5f;
    public float rightLaneBoundary = 5f;

    public int currentDetectedLane = 2; // 1: Trái, 2: Giữa, 3: Phải
    private int lastLane = -1;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        ApplyFreeMovement();
        DetectCurrentLane();
    }

    void ApplyFreeMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float moveX = horizontalInput * steerSpeed;
        float fallSpeed = controller.isGrounded ? -2f : gravity;

        Vector3 moveVector = new Vector3(moveX, fallSpeed, forwardSpeed);
        controller.Move(moveVector * Time.deltaTime);
    }

    void DetectCurrentLane()
    {
        float x = transform.position.x;

        // Tự động phân làn dựa vào 2 ranh giới bạn set trên Inspector
        if (x < leftLaneBoundary) currentDetectedLane = 1;
        else if (x > rightLaneBoundary) currentDetectedLane = 3;
        else currentDetectedLane = 2;

        if (currentDetectedLane != lastLane)
        {
            string laneName = "";
            switch (currentDetectedLane)
            {
                case 1: laneName = "<color=cyan>LÀN 1 (TRÁI)</color>"; break;
                case 2: laneName = "<color=white>LÀN 2 (GIỮA)</color>"; break;
                case 3: laneName = "<color=yellow>LÀN 3 (PHẢI)</color>"; break;
            }

            Debug.Log($"[Radar] Tọa độ X: {x:F2} | Trạng thái: {laneName}");
            lastLane = currentDetectedLane;
        }
    }

    // Vẽ vạch đỏ ra màn hình Scene để dễ căn chỉnh bằng mắt
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        Gizmos.DrawLine(new Vector3(leftLaneBoundary, pos.y, pos.z - 10), new Vector3(leftLaneBoundary, pos.y, pos.z + 50));
        Gizmos.DrawLine(new Vector3(rightLaneBoundary, pos.y, pos.z - 10), new Vector3(rightLaneBoundary, pos.y, pos.z + 50));
    }
}