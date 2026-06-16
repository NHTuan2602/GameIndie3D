using UnityEngine;
using System.Collections;

public class BikeAudioManager : MonoBehaviour
{
    public static BikeAudioManager instance;

    [Header("Nhạc Nền (BGM)")]
    public AudioSource bgmSource;
    public AudioClip mainBGM;

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
        if (mainBGM != null && bgmSource != null)
        {
            bgmSource.clip = mainBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // Các kênh gọi âm thanh 
    public void PlayThrow() { if (throwBrick != null && sfxSource != null) sfxSource.PlayOneShot(throwBrick); }
    public void PlayHit() { if (hitSound != null && sfxSource != null) sfxSource.PlayOneShot(hitSound); }
    public void PlayPothole() { if (potholeHit != null && sfxSource != null) sfxSource.PlayOneShot(potholeHit); }
    public void PlayJump() { if (rampJump != null && sfxSource != null) sfxSource.PlayOneShot(rampJump); }
    public void PlayPickup() { if (pickupItem != null && sfxSource != null) sfxSource.PlayOneShot(pickupItem); }
    public void PlayEnemyHurt() { if (enemyHurt != null && sfxSource != null) sfxSource.PlayOneShot(enemyHurt); }

    // GÓC KHUẤT 4 FIX: Thêm hàm này để PursuitManager có thể ra lệnh "Im Lặng Chơi Lại"
    public void StopAllSounds()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
    }
}