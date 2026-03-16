using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(AudioSource))] // Code tự động ép Unity phải có AudioSource
public class FluorescentFlicker : MonoBehaviour
{
    private Light myLight;
    private AudioSource myAudio;

    [Header("Cường độ sáng & Âm thanh")]
    public float maxIntensity = 4f;
    public float minIntensity = 1f;

    [Tooltip("Âm lượng tối đa khi đèn sáng nhất (0 đến 1)")]
    [Range(0f, 1f)]
    public float maxVolume = 0.6f; // Mức 0.6 để tránh làm điếc tai người chơi

    [Header("Tốc độ chập chờn (Giây)")]
    public float minFlickerSpeed = 0.05f;
    public float maxFlickerSpeed = 0.2f;

    [Header("Tỷ lệ đứt bóng (0.0 đến 1.0)")]
    [Range(0f, 1f)]
    public float dropToZeroChance = 0.15f;

    void Start()
    {
        myLight = GetComponent<Light>();
        myAudio = GetComponent<AudioSource>();

        // Đảm bảo audio luôn chạy ngầm
        myAudio.loop = true;
        if (!myAudio.isPlaying) myAudio.Play();

        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (Random.value < dropToZeroChance)
            {
                // TRƯỜNG HỢP 1: Đứt bóng đen thui
                myLight.intensity = 0f;
                myAudio.volume = 0f; // Cúp điện -> Tắt tiếng ngay lập tức

                yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
            }
            else
            {
                // TRƯỜNG HỢP 2: Ánh sáng giật chập chờn
                float randomIntensity = Random.Range(minIntensity, maxIntensity);
                myLight.intensity = randomIntensity;

                // ==========================================
                // PHÉP THUẬT TOÁN HỌC: ĐỒNG BỘ VOLUME
                // Chia độ sáng hiện tại cho độ sáng max để ra Tỷ lệ %
                // Nhân Tỷ lệ % đó với Âm lượng Max
                // ==========================================
                myAudio.volume = (randomIntensity / maxIntensity) * maxVolume;

                yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
            }
        }
    }
}