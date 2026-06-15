using UnityEngine;
using System.Collections;

public class NightEnvironmentAudio : MonoBehaviour
{
    [Header("--- NHẠC NỀN BGM ---")]
    public AudioSource bgmSource;
    public AudioClip nightBGM;

    [Header("--- ÂM THANH MÔI TRƯỜNG (Tắc kè, Dế...) ---")]
    public AudioSource ambientSource;
    public AudioClip[] ambientClips;

    [Header("--- CÀI ĐẶT NHỊP ĐỘ (Giây) ---")]
    [Tooltip("Thời gian im lặng ít nhất sau khi âm thanh trước kêu xong")]
    public float minWaitTime = 5f;

    [Tooltip("Thời gian im lặng lâu nhất sau khi âm thanh trước kêu xong")]
    public float maxWaitTime = 10f;

    void Start()
    {
        // 1. BẬT NHẠC NỀN MA MỊ
        if (bgmSource != null && nightBGM != null)
        {
            if (bgmSource.clip != nightBGM)
            {
                bgmSource.clip = nightBGM;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        // 2. KHỞI ĐỘNG HỆ THỐNG ÂM THANH MÔI TRƯỜNG NGẪU NHIÊN
        if (ambientSource != null && ambientClips.Length > 0)
        {
            // Chốt an toàn: Đảm bảo cái loa môi trường không bao giờ bị dính Loop
            ambientSource.loop = false;
            StartCoroutine(PlayRandomAmbientSounds());
        }
    }

    IEnumerator PlayRandomAmbientSounds()
    {
        // Vừa vào đêm, cho im lặng một chút từ 2-4 giây rồi mới bắt đầu kêu
        yield return new WaitForSeconds(Random.Range(2f, 4f));

        while (true) // Vòng lặp vô tận suốt đêm
        {
            // 1. Bốc đại 1 âm thanh trong danh sách
            int randomIndex = Random.Range(0, ambientClips.Length);
            AudioClip clipToPlay = ambientClips[randomIndex];

            // 2. ẢO THUẬT: Đổi độ trầm bổng (Pitch) và Âm lượng (Volume) một chút
            ambientSource.pitch = Random.Range(0.85f, 1.15f);
            ambientSource.volume = Random.Range(0.4f, 0.8f);

            // 3. Phát âm thanh đó lên
            ambientSource.PlayOneShot(clipToPlay);

            // ==========================================
            // ĐÃ FIX: CHỜ ÂM THANH KÊU XONG + THÊM 5 ĐẾN 10 GIÂY IM LẶNG
            // ==========================================
            float silenceTime = Random.Range(minWaitTime, maxWaitTime);
            float totalWaitTime = clipToPlay.length + silenceTime;

            // 4. Ngủ đông đúng bằng tổng thời gian đó rồi mới lặp lại vòng mới
            yield return new WaitForSeconds(totalWaitTime);
        }
    }
}