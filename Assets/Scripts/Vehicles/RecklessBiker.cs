using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecklessBiker : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float forwardSpeed = 90f;
    public float weaveFrequency = 4f;

    [Header("Nhân vật NPC (MỚI)")]
    public GameObject npcModel; // Chỗ để gắn thằng lái xe vào

    [Header("Hệ thống Cảnh báo")]
    public Image warningIcon;
    public AudioSource sirenSource;
    public AudioClip warningBeep;

    [Header("Hiệu ứng Tai nạn")]
    public AudioClip screamCrashSound;
    private bool isCrashed = false;

    // Khai báo 2 hướng xoay độc lập cho xe và người
    private Vector3 crashSpinDirection;
    private Vector3 npcSpinDirection;

    private Transform player;
    private float midPoint;
    private float amplitude;
    private bool isReady = false;

    private bool isWeaving = false;
    private float straightLineX;
    private float weaveStartTime;
    private float phaseOffset;

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

        if (player != null)
        {
            straightLineX = player.position.x;
        }
        else
        {
            straightLineX = midPoint;
        }

        StartCoroutine(WarningRoutine());

        // Tạo góc lộn nhào ngẫu nhiên cho Xe (Lật ngang lộn xộn)
        crashSpinDirection = new Vector3(Random.Range(300, 600), Random.Range(200, 500), Random.Range(100, 400));
        // Tạo góc lộn nhào cho Người (Xoay đầu chúi nhủi về phía trước)
        npcSpinDirection = new Vector3(Random.Range(500, 800), Random.Range(-200, 200), Random.Range(-100, 100));
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
        // NẾU BỊ TÉ, CHẠY THUẬT TOÁN "MỖI NGƯỜI 1 NẺO"
        if (isCrashed)
        {
            // 1. CHIẾC XE MÁY: Bị khựng lại, dội ngược ra sau và văng lật ngang
            transform.Translate(Vector3.up * 5f * Time.deltaTime, Space.World);
            transform.Translate(Vector3.back * 10f * Time.deltaTime, Space.World);
            transform.Rotate(crashSpinDirection * Time.deltaTime);

            // 2. NHÂN VẬT LÁI XE: Bị quán tính tống mạnh lên cao, bay vút về phía trước và dạt sang phải
            if (npcModel != null)
            {
                npcModel.transform.Translate(Vector3.up * 15f * Time.deltaTime, Space.World);
                npcModel.transform.Translate(Vector3.forward * 20f * Time.deltaTime, Space.World);
                npcModel.transform.Translate(Vector3.right * 8f * Time.deltaTime, Space.World);
                npcModel.transform.Rotate(npcSpinDirection * Time.deltaTime);
            }

            return; // Khóa di chuyển bình thường
        }

        if (player == null || !isReady) return;

        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        if (!isWeaving)
        {
            transform.position = new Vector3(straightLineX, transform.position.y, transform.position.z);

            if (transform.position.z > player.position.z + 2f)
            {
                isWeaving = true;
                weaveStartTime = Time.time;
                float ratio = Mathf.Clamp((straightLineX - midPoint) / amplitude, -1f, 1f);
                phaseOffset = Mathf.Asin(ratio);
            }
        }
        else
        {
            float timeSinceWeave = Time.time - weaveStartTime;
            float newX = midPoint + Mathf.Sin(timeSinceWeave * weaveFrequency + phaseOffset) * amplitude;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        if (transform.position.z > player.position.z + 250f)
        {
            if (npcModel != null) Destroy(npcModel); // Dọn rác
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

            if (screamCrashSound != null && Camera.main != null)
            {
                GameObject audioObj = new GameObject("ScreamAudio_2D");
                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = screamCrashSound;
                source.spatialBlend = 0f;
                source.volume = 0.7f;
                source.Play();
                Destroy(audioObj, screamCrashSound.length + 0.1f);
            }

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // BƯỚC ĐỘT PHÁ: "LY HÔN" CHIẾC XE VÀ THẰNG NPC
            if (npcModel != null)
            {
                // Cắt đứt quan hệ cha-con. Từ giờ tao không còn ngồi trên xe nữa!
                npcModel.transform.SetParent(null);

                // Tiêu hủy xác thằng NPC sau 2.5 giây để không thành rác
                Destroy(npcModel, 2.5f);
            }

            Destroy(gameObject, 2.5f);
        }
    }
}