using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecklessBiker : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float forwardSpeed = 90f;
    public float weaveFrequency = 4f;

    [Header("Hệ thống Cảnh báo")]
    public Image warningIcon;
    public AudioSource sirenSource;
    public AudioClip warningBeep;

    [Header("Hiệu ứng Tai nạn (MỚI)")]
    public AudioClip screamCrashSound;
    private bool isCrashed = false;
    private Vector3 crashSpinDirection;

    private Transform player;
    private float midPoint;
    private float amplitude;
    private bool isReady = false;

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

        if (warningIcon == null)
        {
            GameObject iconObj = GameObject.FindGameObjectWithTag("WarningIcon");
            if (iconObj != null) warningIcon = iconObj.GetComponent<Image>();
        }

        StartCoroutine(WarningRoutine());
        crashSpinDirection = new Vector3(Random.Range(300, 600), Random.Range(200, 500), Random.Range(100, 400));
    }

    IEnumerator WarningRoutine()
    {
        if (sirenSource != null && warningBeep != null && !isCrashed)
        {
            sirenSource.clip = warningBeep;
            sirenSource.Play();
        }

        float timer = 0;
        bool isIconVisible = false;

        while (timer < 1.5f)
        {
            // FIX LỖI 1: Nếu té giữa chừng, PHẢI TẮT CHẤM THAN trước khi ngắt vòng lặp!
            if (isCrashed)
            {
                if (warningIcon != null) warningIcon.enabled = false;
                yield break;
            }

            if (warningIcon != null)
            {
                isIconVisible = !isIconVisible;
                warningIcon.enabled = isIconVisible;
            }
            yield return new WaitForSeconds(0.15f);
            timer += 0.15f;
        }

        if (warningIcon != null) warningIcon.enabled = false;
    }

    void Update()
    {
        if (isCrashed)
        {
            transform.Translate(Vector3.up * 8f * Time.deltaTime, Space.World);
            transform.Translate(Vector3.back * 15f * Time.deltaTime, Space.World);
            transform.Rotate(crashSpinDirection * Time.deltaTime);
            return;
        }

        if (player == null || !isReady) return;

        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        float newX = midPoint + Mathf.Sin(Time.time * weaveFrequency) * amplitude;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        if (transform.position.z > player.position.z + 250f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCrashed) return;

        if (other.CompareTag("Player"))
        {
            if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayHit();
            if (warningIcon != null) warningIcon.enabled = false;
            PursuitManager.instance.GameOver("BỊ QUÁI XẾ LẠNG LÁCH TÔNG TRÚNG TỪ PHÍA SAU!");
        }

        if (other.CompareTag("Obstacle"))
        {
            isCrashed = true;

            if (sirenSource != null) sirenSource.Stop();

            // FIX LỖI 2: Phát âm thanh tại vị trí Camera thay vì vị trí chiếc xe bị rớt lại
            // Cài đặt âm lượng 2f (Gấp đôi) để nghe tiếng hét chói tai cực rõ!
            if (screamCrashSound != null && Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(screamCrashSound, Camera.main.transform.position, 2f);
            }

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Destroy(gameObject, 2.5f);
        }
    }
}