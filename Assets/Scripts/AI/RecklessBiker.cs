using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecklessBiker : MonoBehaviour
{
    [Header("Cài đặt Lạng lách")]
    public float driveSpeed = 80f; // Tốc độ chạy nhanh hơn player
    public float zigzagFrequency = 3f; // Tốc độ lắc qua lắc lại

    [Header("Hệ thống Cảnh báo")]
    public Image warningIcon;
    public AudioSource sirenSource;
    public AudioClip warningBeep;

    private Transform player;
    private float midPoint;
    private float amplitude;
    private bool isReady = false;

    // HÀM NÀY SẼ ĐƯỢC GỌI TỪ OBSTACLE SPAWNER
    public void SetupLanes(float leftLaneX, float rightLaneX)
    {
        // Tìm tâm điểm giữa 2 làn
        midPoint = (leftLaneX + rightLaneX) / 2f;
        // Tính biên độ lắc (từ tâm ra tới đúng giữa làn)
        amplitude = Mathf.Abs(rightLaneX - leftLaneX) / 2f;
        isReady = true;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (warningIcon == null)
        {
            GameObject iconObj = GameObject.FindGameObjectWithTag("WarningIcon");
            if (iconObj != null) warningIcon = iconObj.GetComponent<Image>();
        }

        StartCoroutine(WarningRoutine());
    }

    IEnumerator WarningRoutine()
    {
        float timer = 0;
        bool isIconVisible = false;
        while (timer < 1.5f)
        {
            if (warningIcon != null) { isIconVisible = !isIconVisible; warningIcon.enabled = isIconVisible; }
            if (sirenSource != null && warningBeep != null) sirenSource.PlayOneShot(warningBeep);
            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }
        if (warningIcon != null) warningIcon.enabled = false;
    }

    void Update()
    {
        if (player == null || !isReady) return;

        // Tiến về phía trước
        transform.Translate(Vector3.forward * driveSpeed * Time.deltaTime);

        // Lắc hình chữ S chính xác từ làn Left đến làn Right
        float newX = midPoint + Mathf.Sin(Time.time * zigzagFrequency) * amplitude;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        if (transform.position.z > player.position.z + 60f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayHit();
            PursuitManager.instance.GameOver("BỊ QUÁI XẾ LẠNG LÁCH TÔNG TRÚNG!");
        }
    }
}