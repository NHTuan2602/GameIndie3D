using UnityEngine;
using System.Collections;

// ĐÃ SỬA: Đổi tên Class thành BikeAudioManager
public class BikeAudioManager : MonoBehaviour
{
    // ĐÃ SỬA: Đổi tên biến Singleton cho khớp
    public static BikeAudioManager instance;

    [Header("Cấu trúc Nhạc Động")]
    public AudioSource bgmSource;
    public AudioClip introClip;
    public AudioClip loopClip;

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

    public void PlayPickup() { sfxSource.PlayOneShot(pickupItem); }
    public void PlayEnemyHurt() { sfxSource.PlayOneShot(enemyHurt); }
}