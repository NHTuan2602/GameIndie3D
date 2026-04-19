using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EscapeBikeController : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float baseSpeed = 40f;
    public float forwardSpeed = 40f;
    public float maxSpeed = 50f;
    public float deceleration = 2f;
    public float steerSpeed = 30f;
    public float gravity = -15f;

    [Header("Radar Định vị Làn đường")]
    public float leftLaneBoundary = -5f;
    public float rightLaneBoundary = 5f;

    public int currentDetectedLane = 2;
    private int lastLane = -1;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        forwardSpeed = baseSpeed;
    }

    void Update()
    {
        // 1. LƯỚI LỌC TỐC ĐỘ: Không bao giờ vượt quá maxSpeed
        forwardSpeed = Mathf.Min(forwardSpeed, maxSpeed);

        // 2. MA SÁT KÉO LẠI: Nếu đang chạy nhanh hơn tốc độ gốc, thì từ từ giảm lại
        if (forwardSpeed > baseSpeed)
        {
            forwardSpeed -= deceleration * Time.deltaTime;
        }

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

        if (x < leftLaneBoundary) currentDetectedLane = 1;
        else if (x > rightLaneBoundary) currentDetectedLane = 3;
        else currentDetectedLane = 2;

        if (currentDetectedLane != lastLane)
        {
            lastLane = currentDetectedLane;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        Gizmos.DrawLine(new Vector3(leftLaneBoundary, pos.y, pos.z - 10), new Vector3(leftLaneBoundary, pos.y, pos.z + 50));
        Gizmos.DrawLine(new Vector3(rightLaneBoundary, pos.y, pos.z - 10), new Vector3(rightLaneBoundary, pos.y, pos.z + 50));
    }
}