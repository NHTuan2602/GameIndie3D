using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecklessBiker : MonoBehaviour
{
    [Header("Cài đặt Di chuyển")]
    public float forwardSpeed = 90f;
    public float weaveFrequency = 4f;

    [Header("Mô hình 3D (ĐỂ BỐC ĐẦU)")]
    public Transform bikeModel; // BẠN KÉO OBJECT "Xemay" VÀO ĐÂY NHÉ
    public GameObject npcModel; // Gã bảo vệ

    [Header("Cài đặt Bốc Đầu")]
    public float wheelieAngle = -25f; // Góc bốc đầu
    public float wheelieSpeed = 5f;   // Tốc độ nhấc đầu xe lên (Càng lớn càng nhanh)

    [Header("Hệ thống Cảnh báo")]
    public Image warningIcon;
    public AudioSource sirenSource;
    public AudioClip warningBeep;

    [Header("Hiệu ứng Tai nạn")]
    public AudioClip screamCrashSound;
    private bool isCrashed = false;

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

    // Trạng thái tàng hình khi đuổi theo từ xa
    private bool isCatchingUp = true;

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

        if (player != null) straightLineX = player.position.x;
        else straightLineX = midPoint;

        StartCoroutine(WarningRoutine());

        crashSpinDirection = new Vector3(Random.Range(300, 600), Random.Range(200, 500), Random.Range(100, 400));
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
        if (isCrashed)
        {
            transform.Translate(Vector3.up * 5f * Time.deltaTime, Space.World);
            transform.Translate(Vector3.back * 10f * Time.deltaTime, Space.World);
            transform.Rotate(crashSpinDirection * Time.deltaTime);

            if (npcModel != null)
            {
                npcModel.transform.Translate(Vector3.up * 15f * Time.deltaTime, Space.World);
                npcModel.transform.Translate(Vector3.forward * 20f * Time.deltaTime, Space.World);
                npcModel.transform.Translate(Vector3.right * 8f * Time.deltaTime, Space.World);
                npcModel.transform.Rotate(npcSpinDirection * Time.deltaTime);
            }
            return;
        }

        if (player == null || !isReady) return;

        // KIỂM TRA BẤT TỬ: Nếu đã tiến sát lưng người chơi ở khoảng cách 15m, Tắt bất tử!
        if (isCatchingUp && transform.position.z >= player.position.z - 15f)
        {
            isCatchingUp = false;
        }

        // =========================================================
        // CHỈ BỐC ĐẦU KHI ĐÃ VƯỢT QUA MẶT NGƯỜI CHƠI (Tầm 1.5 mét)
        // =========================================================
        float currentTargetAngle = 0f;

        if (transform.position.z > player.position.z + 1.5f)
        {
            currentTargetAngle = wheelieAngle;
        }

        if (bikeModel != null)
        {
            Quaternion targetBikeRot = Quaternion.Euler(currentTargetAngle, 0, 0);
            bikeModel.localRotation = Quaternion.Lerp(bikeModel.localRotation, targetBikeRot, Time.deltaTime * wheelieSpeed);
        }

        if (npcModel != null)
        {
            Quaternion targetNpcRot = Quaternion.Euler(currentTargetAngle, 0, 0);
            npcModel.transform.localRotation = Quaternion.Lerp(npcModel.transform.localRotation, targetNpcRot, Time.deltaTime * wheelieSpeed);
        }
        // =========================================================

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
            if (npcModel != null) Destroy(npcModel);
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
            // NẾU ĐANG BẤT TỬ MÀ ĐỤNG TRÚNG XE TẢI -> XUYÊN QUA LUÔN
            if (isCatchingUp) return;

            isCrashed = true;
            if (sirenSource != null) sirenSource.Stop();

            if (screamCrashSound != null && Camera.main != null)
            {
                GameObject audioObj = new GameObject("ScreamAudio_2D");
                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = screamCrashSound;
                source.spatialBlend = 0f;
                source.volume = 1f;
                source.Play();
                Destroy(audioObj, screamCrashSound.length + 0.1f);
            }

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            if (npcModel != null)
            {
                npcModel.transform.SetParent(null);
                Destroy(npcModel, 2.5f);
            }
            Destroy(gameObject, 2.5f);
        }
    }
}