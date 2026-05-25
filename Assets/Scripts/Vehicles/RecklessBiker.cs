using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecklessBiker : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float forwardSpeed = 90f;
    public float weaveFrequency = 4f;

    [Header("Hệ thống Cảnh báo (Tự động tìm nếu để trống)")]
    public Image warningIcon;
    public AudioSource sirenSource;
    public AudioClip warningBeep;

    private Transform player;
    private float midPoint;
    private float amplitude;
    private bool isReady = false;

    // HÀM NÀY ĐƯỢC OBSTACLE SPAWNER GỌI KHI VỪA ĐẺ RA
    public void SetupLanes(float leftLaneX, float rightLaneX)
    {
        midPoint = (leftLaneX + rightLaneX) / 2f;
        amplitude = Mathf.Abs(rightLaneX - leftLaneX) / 2f;
        isReady = true;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // THUẬT TOÁN RADAR: Tự động lùng sục tìm cái ảnh có Tag "WarningIcon" trên Canvas
        if (warningIcon == null)
        {
            GameObject iconObj = GameObject.FindGameObjectWithTag("WarningIcon");
            if (iconObj != null)
            {
                warningIcon = iconObj.GetComponent<Image>();
            }
            else
            {
                Debug.LogWarning("Sparring Partner nhắc nhở: Bạn chưa gắn Tag 'WarningIcon' cho cái ảnh trên Canvas kìa!");
            }
        }

        StartCoroutine(WarningRoutine());
    }

    IEnumerator WarningRoutine()
    {
        float timer = 0;
        bool isIconVisible = false;

        // Nhấp nháy liên tục trong 1.5 giây
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

            yield return new WaitForSeconds(0.15f); // Tốc độ chớp tắt (càng nhỏ càng nhanh)
            timer += 0.15f;
        }

        // Tắt hẳn khi xe đã trờ tới
        if (warningIcon != null) warningIcon.enabled = false;
    }

    void Update()
    {
        if (player == null || !isReady) return;

        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        float newX = midPoint + Mathf.Sin(Time.time * weaveFrequency) * amplitude;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        if (transform.position.z > player.position.z + 80f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayHit();

            // Ép tắt cái icon lỡ như người chơi chết khi nó đang nhấp nháy
            if (warningIcon != null) warningIcon.enabled = false;

            PursuitManager.instance.GameOver("BỊ QUÁI XẾ LẠNG LÁCH TÔNG TRÚNG TỪ PHÍA SAU!");
        }
    }
}