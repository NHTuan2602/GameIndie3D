using UnityEngine;

public class NoiseTrap : MonoBehaviour
{
    [Header("Cài đặt Bẫy Tiếng Ồn")]
    public AudioClip trapSound; // Âm thanh xoảng/bộp
    public float noiseRadius = 20f; // Độ vang của tiếng ồn (20 mét)
    public bool isOneTimeUse = true; // Kêu 1 lần rồi tịt (chống spam)

    private bool hasTriggered = false;
    private AudioSource audioSource;

    void Start()
    {
        // Tự động thêm AudioSource nếu bạn quên gắn trên Unity
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1f; // Âm thanh 3D (gần to xa nhỏ)
    }

    private void OnTriggerEnter(Collider other)
    {
        // Nếu đã kích hoạt rồi và là bẫy dùng 1 lần thì bỏ qua
        if (hasTriggered && isOneTimeUse) return;

        // Chỉ kêu khi người chơi (Player) dẫm vào
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            // 1. Phát ra tiếng động
            if (trapSound != null)
            {
                audioSource.PlayOneShot(trapSound);
                Debug.Log("<color=cyan>XOẢNG! Bạn đã dẫm phải bẫy tiếng ồn!</color>");
            }

            // 2. Đánh động con AI (Nếu nó ở trong bán kính nghe được)
            EnemyPatrol enemy = FindObjectOfType<EnemyPatrol>();
            if (enemy != null)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy <= noiseRadius)
                {
                    // Gọi hàm AI ra kiểm tra tiếng ồn
                    enemy.InvestigateNoise(transform.position);
                }
            }
        }
    }
}