using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Bùa chú Singleton để gọi từ mọi nơi
    public static AudioManager instance;

    [Header("--- Nguồn Phát Âm Thanh ---")]
    [Tooltip("Dành cho nhạc nền (phát liên tục)")]
    public AudioSource bgmSource;
    [Tooltip("Dành cho hiệu ứng âm thanh 2D (UI, hệ thống)")]
    public AudioSource sfxSource;

    [Header("--- Kho Âm Thanh ---")]
    public AudioClip bgmMusic;
    public AudioClip uiClickSound;
    public AudioClip sirenAlarmSound;

    [Tooltip("Bỏ nhiều tiếng bước chân vào đây để phát ngẫu nhiên, nghe sẽ thật hơn")]
    public AudioClip[] stealthFootsteps;

    void Awake()
    {
        // Đảm bảo chỉ có 1 AudioManager tồn tại qua các màn chơi
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Tự động bật nhạc nền khi vào game
        if (bgmMusic != null) PlayBGM(bgmMusic);
    }

    // ==========================================
    // CÁC HÀM PHÁT ÂM THANH SẴN SÀNG ĐỂ GỌI
    // ==========================================

    // 1. Nhạc Nền (BGM)
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource != null && clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // 2. Tiếng UI (Bấm nút)
    public void PlayUI()
    {
        if (sfxSource != null && uiClickSound != null)
        {
            sfxSource.PlayOneShot(uiClickSound);
        }
    }

    // 3. Tiếng Báo Động (Phát ra dạng 3D từ vị trí kẻ địch)
    public void PlayAlarm(Vector3 enemyPosition)
    {
        if (sirenAlarmSound != null)
        {
            // AudioSource.PlayClipAtPoint giúp tạo âm thanh 3D, càng xa nghe càng nhỏ
            AudioSource.PlayClipAtPoint(sirenAlarmSound, enemyPosition, 1f);
        }
    }

    // 4. Tiếng Bước Chân Lén Lút
    public void PlayFootstep(AudioSource playerAudioSource)
    {
        if (stealthFootsteps.Length > 0 && playerAudioSource != null)
        {
            // Chọn ngẫu nhiên 1 tiếng bước chân
            int randIndex = Random.Range(0, stealthFootsteps.Length);

            // Đổi tông âm thanh (Pitch) một chút để các bước chân không bị lặp lại y hệt nhau
            playerAudioSource.pitch = Random.Range(0.8f, 1.1f);
            playerAudioSource.PlayOneShot(stealthFootsteps[randIndex]);
        }
    }
}