using UnityEngine;
using System.Collections;

public class BikeAudioManager : MonoBehaviour
{
    public static BikeAudioManager instance;

    [Header("Nhạc Nền (BGM)")]
    public AudioSource bgmSource;
    public AudioClip mainBGM; // ĐÃ SỬA: Chỉ dùng 1 bài duy nhất

    [Header("Hiệu ứng SFX (Loa phát)")]
    public AudioSource sfxSource;

    [Header("Cuộn băng SFX (Kéo file MP3/WAV vào đây)")]
    public AudioClip throwBrick;
    public AudioClip hitSound;
    public AudioClip potholeHit;
    public AudioClip rampJump;
    public AudioClip pickupItem;
    public AudioClip enemyHurt;

    void Awake() { instance = this; }

    void Start()
    {
        // Phát đúng 1 bài nhạc và lặp lại liên tục
        if (mainBGM != null && bgmSource != null)
        {
            bgmSource.clip = mainBGM;
            bgmSource.loop = true; // Bật chế độ lặp
            bgmSource.Play();
        }
    }

    // Các kênh gọi âm thanh (Đã thêm check an toàn chống lỗi đỏ)
    public void PlayThrow() { if (throwBrick != null && sfxSource != null) sfxSource.PlayOneShot(throwBrick); }
    public void PlayHit() { if (hitSound != null && sfxSource != null) sfxSource.PlayOneShot(hitSound); }
    public void PlayPothole() { if (potholeHit != null && sfxSource != null) sfxSource.PlayOneShot(potholeHit); }
    public void PlayJump() { if (rampJump != null && sfxSource != null) sfxSource.PlayOneShot(rampJump); }
    public void PlayPickup() { if (pickupItem != null && sfxSource != null) sfxSource.PlayOneShot(pickupItem); }
    public void PlayEnemyHurt() { if (enemyHurt != null && sfxSource != null) sfxSource.PlayOneShot(enemyHurt); }
}