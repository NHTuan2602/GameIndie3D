using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EscapeBikeController : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float forwardSpeed = 30f;
    public float steerSpeed = 15f;
    public float gravity = -15f;

    [Header("Radar Định vị Làn đường")]
    public int currentDetectedLane = 3; // Mặc định ở giữa
    private int lastLane = -1; // Biến phụ để theo dõi sự thay đổi

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

        // 1. Logic phân làn theo tọa độ Tuấn cung cấp
        if (x <= 4.894493f)
        {
            currentDetectedLane = 1;
        }
        else if (x >= 35.10812f)
        {
            currentDetectedLane = 2;
        }
        else
        {
            currentDetectedLane = 3;
        }

        // 2. CHỈ DEBUG KHI CÓ SỰ THAY ĐỔI (Tránh spam Console)
        if (currentDetectedLane != lastLane)
        {
            string laneName = "";
            switch (currentDetectedLane)
            {
                case 1: laneName = "<color=cyan>LÀN 1 (TRÁI)</color>"; break;
                case 2: laneName = "<color=yellow>LÀN 2 (PHẢI)</color>"; break;
                case 3: laneName = "<color=white>LÀN 3 (GIỮA)</color>"; break;
            }

            Debug.Log($"[Radar] Tọa độ X: {x:F2} | Trạng thái: {laneName}");
            lastLane = currentDetectedLane;
        }
    }

    // 3. VẼ VẠCH NGƯỠNG TRONG CỬA SỔ SCENE ĐỂ DỄ QUAN SÁT
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        // Vẽ vạch ngăn làn 1 và làn 3
        Gizmos.DrawLine(new Vector3(4.894493f, pos.y, pos.z - 5), new Vector3(4.894493f, pos.y, pos.z + 10));
        // Vẽ vạch ngăn làn 3 và làn 2
        Gizmos.DrawLine(new Vector3(35.10812f, pos.y, pos.z - 5), new Vector3(35.10812f, pos.y, pos.z + 10));
    }
}