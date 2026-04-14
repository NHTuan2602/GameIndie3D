using UnityEngine;
using System.Collections;

public class OncomingCar : MonoBehaviour
{
    public float speed = 50f;
    public Light warningLight; // Kéo Spotlight của xe vào đây
    public float warningDuration = 1.5f;

    void Start()
    {
        StartCoroutine(CarRoutine());
    }

    IEnumerator CarRoutine()
    {
        // Giai đoạn 1: Cảnh báo (Đèn pha nhấp nháy hoặc sáng rực)
        warningLight.enabled = true;
        yield return new WaitForSeconds(warningDuration);

        // Giai đoạn 2: Lao tới
        while (transform.position.z > -100)
        { // Chạy cho đến khi ra sau lưng người chơi
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Đâm vào xe ngược chiều = Game Over ngay lập tức
            Debug.Log("ĐÂM TRỰC DIỆN! VƯỢT NGỤC THẤT BẠI.");
        }
    }
}