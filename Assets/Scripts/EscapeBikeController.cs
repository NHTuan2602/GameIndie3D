using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class EscapeBikeController : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float baseSpeed = 60f;     // TỐC ĐỘ GỐC ĐÃ NÂNG LÊN 60
    public float maxRampSpeed = 65f;  // Tốc độ vọt lên tối đa khi bay dốc
    public float deceleration = 2f;   // Độ gắt của phanh khi giảm từ 65 về 60
    public float steerSpeed = 30f;
    public float gravity = -15f;

    [Header("Radar Định vị Làn đường")]
    public float leftLaneBoundary = -5f;
    public float rightLaneBoundary = 5f;

    [Header("Giao diện UI (KÉO PANEL GAME OVER VÀO ĐÂY)")]
    public GameObject gameOverPanel;

    public int currentDetectedLane = 2;
    private int lastLane = -1;

    private CharacterController controller;
    private bool isFalling = false;

    // Biến lưu tốc độ thực tế của xe
    public float forwardSpeed = 60f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Khởi đầu game với tốc độ 60
        forwardSpeed = baseSpeed;
    }

    void Update()
    {
        if (isFalling) return;

        // 1. CHỐT CHẶN: Không bao giờ vọt quá 65km/h dù có bay bao nhiêu cái dốc
        forwardSpeed = Mathf.Min(forwardSpeed, maxRampSpeed);

        // 2. DÂY THUN: Nếu đang chạy > 60 (do vừa bay dốc 65), trừ dần về lại 60
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
    // CẢM BIẾN NHẬN DIỆN BAY DỐC
    // ==========================================
    void OnTriggerEnter(Collider other)
    {
        // Khi xe đạp đâm trúng vật thể có gắn Tag là "Ramp" (Cái dốc)
        if (other.CompareTag("Ramp"))
        {
            Debug.Log("<color=green>BAY DỐC! Tốc độ vọt lên 65km/h</color>");
            forwardSpeed = maxRampSpeed; // Bơm thẳng tốc độ lên 65
        }
    }

    public void TriggerFallDeath()
    {
        if (isFalling) return;
        StartCoroutine(FallAndDieRoutine());
    }

    IEnumerator FallAndDieRoutine()
    {
        isFalling = true;
        Debug.Log("<color=red>SAI ĐƯỜNG RỒI! XE ĐANG VĂNG XUỐNG VỰC...</color>");

        if (controller != null) controller.enabled = false;

        float timer = 0f;
        Vector3 fallVelocity = Vector3.zero;

        // Cho xe lộn nhào rơi tự do trong 2.5 giây
        while (timer < 2.5f)
        {
            timer += Time.deltaTime;
            fallVelocity.y += -30f * Time.deltaTime;
            fallVelocity.z = baseSpeed * 0.5f;
            transform.position += fallVelocity * Time.deltaTime;
            transform.Rotate(new Vector3(150, 50, 0) * Time.deltaTime);
            yield return null;
        }

        Debug.Log("HIỆN MÀN HÌNH GAME OVER");

        // BẬT PANEL GAME OVER THAY VÌ LOAD LẠI SCENE
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Bạn quên kéo Panel Game Over vào script EscapeBikeController rồi!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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