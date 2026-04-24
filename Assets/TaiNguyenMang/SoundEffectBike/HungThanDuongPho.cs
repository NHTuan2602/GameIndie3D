using UnityEngine;
using System.Collections;

public class HungThanDuongPho : MonoBehaviour
{
    [Header("Chỉ số di chuyển")]
    public float driveSpeed = 25f;

    [Header("Hệ thống Còi Ngẫu Nhiên")]
    // Thay vì 1 clip, ta dùng 1 mảng để chứa nhiều loại cèn (Phương Trang, Còi hơi, Còi ngân...)
    public AudioClip[] randomHorns;

    [Header("Hệ thống Xé Gió")]
    public AudioClip passBySound;

    [Header("Cài đặt Âm thanh")]
    public float stopSoundDistance = 50f; // Khoảng cách để bắt đầu tắt nhạc sau khi vượt qua
    public bool isLoopingSiren = false;  // Tích vào nếu là xe cấp cứu cần hú liên tục

    [Header("Hệ thống Đèn Pha")]
    public Light[] headLights;
    public float flashSpeed = 15f;
    public float baseIntensity = 50f;
    public float maxIntensity = 300f;

    private AudioSource hornSource;
    private AudioSource windSource;
    private Transform player;
    private bool hasPassed = false;
    private bool isFadingOut = false;

    void Start()
    {
        // 1. Tạo và cấu hình loa
        hornSource = gameObject.AddComponent<AudioSource>();
        windSource = gameObject.AddComponent<AudioSource>();

        // Cấu hình âm thanh 3D để nghe được hướng xe chạy
        SetupAudioSource(hornSource, 200f);
        SetupAudioSource(windSource, 50f);

        // 2. Tìm người chơi
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // 3. PHÁT KÈN NGẪU NHIÊN KHI XUẤT HIỆN
        if (randomHorns != null && randomHorns.Length > 0)
        {
            AudioClip selectedHorn = randomHorns[Random.Range(0, randomHorns.Length)];
            hornSource.clip = selectedHorn;
            hornSource.loop = isLoopingSiren; // Nếu là còi hú thì cho lặp lại
            hornSource.Play();
        }
    }

    void SetupAudioSource(AudioSource source, float maxDist)
    {
        source.spatialBlend = 1f; // Âm thanh 3D
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 5f;
        source.maxDistance = maxDist;
        source.playOnAwake = false;
    }

    void Update()
    {
        if (player == null) return;

        // 1. Di chuyển xe
        transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime);

        // 2. Hiệu ứng nháy đèn
        HandleLights();

        // 3. Xử lý âm thanh theo khoảng cách
        HandleDistanceBasedAudio();
    }

    void HandleLights()
    {
        if (headLights.Length > 0)
        {
            float pingPong = Mathf.PingPong(Time.time * flashSpeed, maxIntensity - baseIntensity);
            foreach (Light l in headLights)
            {
                if (l != null) l.intensity = baseIntensity + pingPong;
            }
        }
    }

    void HandleDistanceBasedAudio()
    {
        float distanceZ = transform.position.z - player.position.z;

        // A. KÍCH HOẠT TIẾNG XÉ GIÓ (Khi xe đến gần trong khoảng 25m)
        if (!hasPassed && Mathf.Abs(distanceZ) <= 25f)
        {
            hasPassed = true;
            if (passBySound != null) windSource.PlayOneShot(passBySound);
        }

        // B. TẮT NHẠC KHI VƯỢT QUA ĐỦ XA
        // Nếu xe đã vượt qua player (distanceZ dương) và vượt quá khoảng cách định sẵn
        if (hasPassed && distanceZ > stopSoundDistance && !isFadingOut)
        {
            StartCoroutine(FadeOutAudio());
        }
    }

    // Coroutine giúp âm thanh nhỏ dần rồi mới tắt, tránh bị ngắt đột ngột nghe rất thô
    IEnumerator FadeOutAudio()
    {
        isFadingOut = true;
        float startVolume = hornSource.volume;

        while (hornSource.volume > 0)
        {
            hornSource.volume -= startVolume * Time.deltaTime * 2f; // Giảm dần volume
            windSource.volume -= startVolume * Time.deltaTime * 2f;
            yield return null;
        }

        hornSource.Stop();
        windSource.Stop();
        Debug.Log("<color=cyan>Đã tắt âm thanh xe hung thần do đi quá xa.</color>");
    }
}