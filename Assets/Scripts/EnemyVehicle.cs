using UnityEngine;

public class EnemyVehicle : MonoBehaviour
{
    [Header("Chỉ số di chuyển")]
    public float driveSpeed = 15f;

    [Header("Hệ thống cảnh báo lóa mắt")]
    public Light[] headLights;
    public float flashSpeed = 15f;
    public float baseIntensity = 50f;
    public float maxIntensity = 300f;

    [Header("Hệ thống Âm thanh Xé gió")]
    public AudioSource audioSource;
    public AudioClip passBySound;

    private Transform player;
    private bool hasPassed = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime);

        if (headLights.Length > 0)
        {
            float pingPong = Mathf.PingPong(Time.time * flashSpeed, maxIntensity - baseIntensity);
            foreach (Light l in headLights)
            {
                if (l != null) l.intensity = baseIntensity + pingPong;
            }
        }

        // CẢM BIẾN XÉ GIÓ ĐÃ ĐƯỢC MỞ RỘNG TẦM NHÌN LÊN 25 MÉT
        if (!hasPassed && player != null)
        {
            // Tăng từ 5f lên 25f để kích hoạt âm thanh sớm hơn, bù trừ độ trễ file nhạc
            if (transform.position.z <= player.position.z + 25f)
            {
                hasPassed = true;
                if (audioSource != null && passBySound != null)
                {
                    audioSource.PlayOneShot(passBySound);
                }
            }
        }
    }
}