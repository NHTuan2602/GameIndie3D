using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Cấu trúc Nhạc Động")]
    public AudioSource bgmSource;
    public AudioClip introClip;
    public AudioClip loopClip;

    [Header("Hiệu ứng SFX (Loa phát)")]
    public AudioSource sfxSource;

    [Header("Cuộn băng SFX (Kéo file MP3/WAV vào đây)")]
    public AudioClip throwBrick;    // Tiếng Vút ném đi
    public AudioClip hitSound;      // Tiếng RẦM tông xe (Game Over)
    public AudioClip potholeHit;    // Tiếng Xóc ổ gà
    public AudioClip rampJump;      // Tiếng Bay lên dốc

    // 2 CUỘN BĂNG MỚI DÀNH RIÊNG CHO COMBO NÉM GẠCH:
    public AudioClip pickupItem;    // Tiếng Ting/Cắc nhặt đồ
    public AudioClip enemyHurt;     // Tiếng Choảng (kính vỡ) / Á (địch kêu)

    void Awake() { instance = this; }

    void Start()
    {
        if (introClip != null && bgmSource != null)
        {
            StartCoroutine(PlayDynamicMusic());
        }
    }

    IEnumerator PlayDynamicMusic()
    {
        bgmSource.clip = introClip;
        bgmSource.loop = false;
        bgmSource.Play();

        yield return new WaitForSeconds(introClip.length - 0.1f);

        bgmSource.clip = loopClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // Các kênh gọi âm thanh
    public void PlayThrow() { sfxSource.PlayOneShot(throwBrick); }
    public void PlayHit() { sfxSource.PlayOneShot(hitSound); }
    public void PlayPothole() { sfxSource.PlayOneShot(potholeHit); }
    public void PlayJump() { sfxSource.PlayOneShot(rampJump); }

    // KÊNH MỚI:
    public void PlayPickup() { sfxSource.PlayOneShot(pickupItem); }
    public void PlayEnemyHurt() { sfxSource.PlayOneShot(enemyHurt); }
}