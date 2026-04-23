using UnityEngine;
using System.Collections; // Bắt buộc phải có dòng này để dùng Coroutine

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

    // BIẾN MỚI: Cờ đánh dấu xe đang rơi
    private bool isFalling = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        forwardSpeed = baseSpeed;
    }

    void Update()
    {
        // QUAN TRỌNG: Nếu đang rớt xuống hố thì cấm không cho đạp xe hay bẻ lái nữa!
        if (isFalling) return;

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

    // ==========================================
    // LOGIC MỚI: XỬ LÝ RƠI XUỐNG VỰC (GAME OVER)
    // ==========================================
    public void TriggerFallDeath()
    {
        if (isFalling) return; // Nếu đang rơi rồi thì không gọi lại nữa
        StartCoroutine(FallAndDieRoutine());
    }

    IEnumerator FallAndDieRoutine()
    {
        isFalling = true;
        Debug.Log("<color=red>SAI ĐƯỜNG RỒI! XE ĐANG VĂNG XUỐNG VỰC...</color>");

        // 1. Tắt CharacterController để chúng ta có thể tự do vứt cái xe rơi tự do
        if (controller != null) controller.enabled = false;

        float timer = 0f;
        Vector3 fallVelocity = Vector3.zero;

        // 2. Diễn hoạt rơi và lộn nhào trong 3 giây
        while (timer < 3f)
        {
            timer += Time.deltaTime;

            // Rơi nhanh dần đều
            fallVelocity.y += -30f * Time.deltaTime;

            // Giữ lại một chút đà đi tới để xe "văng" tới trước xuống hố chứ không rớt thẳng đứng
            fallVelocity.z = baseSpeed * 0.5f;

            transform.position += fallVelocity * Time.deltaTime;

            // Xoay lộn nhào chiếc xe đạp cho cảm giác tai nạn mạnh
            transform.Rotate(new Vector3(150, 50, 0) * Time.deltaTime);

            yield return null;
        }

        // 3. Sau 3 giây, Load lại màn chơi này từ đầu
        Debug.Log("GAME OVER");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        Gizmos.DrawLine(new Vector3(leftLaneBoundary, pos.y, pos.z - 10), new Vector3(leftLaneBoundary, pos.y, pos.z + 50));
        Gizmos.DrawLine(new Vector3(rightLaneBoundary, pos.y, pos.z - 10), new Vector3(rightLaneBoundary, pos.y, pos.z + 50));
    }
}