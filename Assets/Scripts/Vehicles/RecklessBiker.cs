using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecklessBiker : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float forwardSpeed = 90f; // Phải nhanh hơn tốc độ player (Player max 65) để nó đuổi kịp từ phía sau
    public float weaveFrequency = 4f; // Độ gắt của cú lạng lách (Số càng to lắc càng nhanh)

    [Header("Hệ thống Cảnh báo (Kéo thả UI vào đây)")]
    public Image warningIcon; // Cái chấm than đỏ nhấp nháy
    public AudioSource sirenSource; // Loa phát tiếng còi
    public AudioClip warningBeep; // Tiếng tít tít tít

    private Transform player;
    private float midPoint;
    private float amplitude;
    private bool isReady = false;

    // HÀM NÀY ĐƯỢC OBSTACLE SPAWNER GỌI KHI VỪA ĐẺ RA
    public void SetupLanes(float leftLaneX, float rightLaneX)
    {
        // 1. Tìm tâm điểm: Nằm chính giữa 2 làn
        midPoint = (leftLaneX + rightLaneX) / 2f;

        // 2. Biên độ lắc: Lắc từ tâm ra tới mép làn (bằng một nửa khoảng cách 2 làn)
        amplitude = Mathf.Abs(rightLaneX - leftLaneX) / 2f;

        isReady = true;
    }

    void Start()
    {
        // Tìm người chơi
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Tự động tìm chấm than đỏ trên màn hình (Nếu bạn quên kéo thả)
        if (warningIcon == null)
        {
            GameObject iconObj = GameObject.FindGameObjectWithTag("WarningIcon");
            if (iconObj != null) warningIcon = iconObj.GetComponent<Image>();
        }

        // Vừa xuất hiện là nhá đèn kêu còi cảnh báo ngay
        StartCoroutine(WarningRoutine());
    }

    IEnumerator WarningRoutine()
    {
        float timer = 0;
        bool isIconVisible = false;

        // Cảnh báo trong 1.5 giây đầu tiên
        while (timer < 1.5f)
        {
            if (warningIcon != null)
            {
                isIconVisible = !isIconVisible;
                warningIcon.enabled = isIconVisible;
            }
            if (sirenSource != null && warningBeep != null)
            {
                sirenSource.PlayOneShot(warningBeep);
            }

            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }

        // Tắt chấm than khi xe đã đuổi sát tới nơi
        if (warningIcon != null) warningIcon.enabled = false;
    }

    void Update()
    {
        if (player == null || !isReady) return;

        // 1. Phóng thẳng lên phía trước với tốc độ bàn thờ
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // 2. THUẬT TOÁN LẠNG LÁCH (SÓNG SIN)
        // Sóng Sin sẽ chạy liên tục từ -1 đến 1. 
        // Khi nhân với amplitude, xe sẽ lượn từ mép Làn Trái sang mép Làn Phải cực kỳ mượt mà.
        float newX = midPoint + Mathf.Sin(Time.time * weaveFrequency) * amplitude;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        // 3. Tự hủy khi đã bỏ xa người chơi ở phía trước (khoảng 80 mét)
        if (transform.position.z > player.position.z + 80f)
        {
            Destroy(gameObject);
        }
    }

    // Xử lý tông trúng người chơi
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayHit();

            // Gọi màn hình GameOver
            PursuitManager.instance.GameOver("BỊ QUÁI XẾ LẠNG LÁCH TÔNG TRÚNG TỪ PHÍA SAU!");
        }
    }
}