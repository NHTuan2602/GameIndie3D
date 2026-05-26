using UnityEngine;

public class EnemyVehicle : MonoBehaviour
{
    [Header("Chỉ số di chuyển & Trí tuệ AI")]
    public float defaultSpeed = 30f; // Tốc độ chạy bình thường (thay thế cho driveSpeed cũ)
    private float currentSpeed;

    [Header("Cảm biến Radar (Raycast)")]
    public float sensorLength = 25f; // Tầm quét của laser
    public float safeDistance = 8f;  // Khoảng cách phanh gấp
    public Vector3 sensorOffset = new Vector3(0, 1f, 0); // Nâng radar lên 1m khỏi mặt đường

    [Header("Hệ thống cảnh báo lóa mắt")]
    public Light[] headLights;
    public float flashSpeed = 15f;
    public float baseIntensity = 50f;
    public float maxIntensity = 300f;

    [Header("Hệ thống Âm thanh Xé gió")]
    public AudioSource audioSource;
    public AudioClip passBySound;

    private Transform player;
    private bool hasPassed = false;

    void Start()
    {
        currentSpeed = defaultSpeed; // Khởi tạo tốc độ ban đầu
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        // 1. CHẠY RADAR AI ĐỂ QUYẾT ĐỊNH TỐC ĐỘ (currentSpeed)
        HandleTrafficAI();

        // 2. DI CHUYỂN DỰA TRÊN TỐC ĐỘ ĐÃ ĐƯỢC AI TÍNH TOÁN
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self);

        // 3. HIỆU ỨNG NHÁY ĐÈN (Giữ nguyên của bạn)
        if (headLights.Length > 0)
        {
            float pingPong = Mathf.PingPong(Time.time * flashSpeed, maxIntensity - baseIntensity);
            foreach (Light l in headLights)
            {
                if (l != null) l.intensity = baseIntensity + pingPong;
            }
        }

        // 4. HIỆU ỨNG ÂM THANH XÉ GIÓ (Giữ nguyên của bạn)
        if (!hasPassed && player != null)
        {
            if (transform.position.z <= player.position.z + 25f)
            {
                hasPassed = true;
                if (audioSource != null && passBySound != null)
                {
                    audioSource.PlayOneShot(passBySound);
                }
            }
        }
    }

    // THUẬT TOÁN RADAR XỬ LÝ PHANH TRÁNH ĐÂM XUYÊN
    void HandleTrafficAI()
    {
        Vector3 origin = transform.position + sensorOffset;
        Vector3 direction = transform.forward;

        // Vẽ tia đỏ trong màn hình Scene để dễ fix bug
        Debug.DrawRay(origin, direction * sensorLength, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, sensorLength))
        {
            // NẾU TIA LASER CHẠM VÀO VẬT CÓ TAG LÀ "Obstacle" (Ví dụ: Xe tải)
            if (hit.collider.CompareTag("Obstacle"))
            {
                if (hit.distance <= safeDistance)
                {
                    // Quá gần -> Phanh gấp (giảm xuống 18km/h)
                    currentSpeed = Mathf.Lerp(currentSpeed, 18f, Time.deltaTime * 5f);
                }
                else
                {
                    // Từ xa -> Rà phanh từ từ (giảm xuống 20km/h cho bằng xe tải)
                    currentSpeed = Mathf.Lerp(currentSpeed, 20f, Time.deltaTime * 2f);
                }
                return; // Thoát hàm ngay để không tăng tốc
            }
        }

        // Đường thoáng -> Đạp ga về lại defaultSpeed (30km/h)
        currentSpeed = Mathf.Lerp(currentSpeed, defaultSpeed, Time.deltaTime * 2f);
    }
}