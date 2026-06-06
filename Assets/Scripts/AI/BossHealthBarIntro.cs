using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarIntro : MonoBehaviour
{
    [Header("Giao diện UI")]
    [Tooltip("Kéo thanh máu (Slider) màu đỏ trên màn hình vào đây")]
    public Slider healthSlider;

    [Header("Cài đặt Hiệu ứng")]
    [Tooltip("Thời gian gồng đầy máu (Giây)")]
    public float fillDuration = 2.5f;
    [Tooltip("Mức máu tối đa")]
    public float maxHealth = 100f;

    [Header("Âm thanh (Tùy chọn)")]
    public AudioSource audioSource;
    public AudioClip chargingSound; // Tiếng dồn dập lúc máu đang nạp
    public AudioClip fullyChargedSound; // Tiếng "Keng" lúc máu nạp xong

    void Start()
    {
        if (healthSlider != null)
        {
            // 1. Ép thanh UI về con số 0 ngay lập tức khi vừa mở Scene
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = 0f;

            // 2. Kích hoạt hiệu ứng nạp năng lượng
            StartCoroutine(AnimateHealthBar());
        }
        else
        {
            Debug.LogError("Bạn chưa kéo thanh Slider vào script BossHealthBarIntro!");
        }
    }

    IEnumerator AnimateHealthBar()
    {
        float elapsedTime = 0f;

        // Bật tiếng nạp năng lượng dồn dập
        if (audioSource != null && chargingSound != null)
        {
            audioSource.clip = chargingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Vòng lặp tăng dần thanh máu
        while (elapsedTime < fillDuration)
        {
            elapsedTime += Time.deltaTime;

            // Dùng Mathf.Lerp để thanh máu chạy mượt từ 0 đến maxHealth
            healthSlider.value = Mathf.Lerp(0f, maxHealth, elapsedTime / fillDuration);

            yield return null; // Đợi frame tiếp theo
        }

        // Đảm bảo máu chốt sổ ở mức 100% tuyệt đối, không bị số lẻ
        healthSlider.value = maxHealth;

        // Xử lý âm thanh chốt sổ
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            if (fullyChargedSound != null)
            {
                audioSource.PlayOneShot(fullyChargedSound);
            }
        }

        Debug.Log("<color=green>Nạp máu hoàn tất! Sẵn sàng đua xe!</color>");

        // MẸO: Bạn có thể gọi GameManager.instance.StartRace() hoặc mở khóa điều khiển ở đây
        // nếu muốn người chơi phải đứng nhìn thanh máu đầy rồi mới được chạy.
    }
}