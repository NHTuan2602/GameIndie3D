using System.Collections;
using UnityEngine;

public class WakeUpManager : MonoBehaviour
{
    // Da xoa tieng Viet co dau de chong loi Unity
    public GameObject topEyelid;
    public GameObject bottomEyelid;

    [Header("Âm thanh Thức giấc (Tùy chọn)")]
    public AudioSource audioSource;
    public AudioClip gaspSound; // Tiếng hít sâu giật mình tỉnh dậy

    void Start()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.canWalk = false;
            player.canLook = false;
        }

        StartCoroutine(RealisticWakeUpRoutine());
    }

    IEnumerator RealisticWakeUpRoutine()
    {
        // Chờ 1 giây tĩnh lặng trong bóng tối trước khi tỉnh
        yield return new WaitForSeconds(1.0f);

        if (audioSource != null && gaspSound != null)
        {
            audioSource.PlayOneShot(gaspSound, 0.8f);
        }

        // PHA 1: Hé mắt nhẹ vì chói (Kéo ScaleY từ 1 xuống 0.7)
        yield return StartCoroutine(BlinkEyelids(1f, 0.7f, 0.3f));
        yield return new WaitForSeconds(0.1f);
        // Nhắm tịt lại (Kéo ScaleY từ 0.7 lên 1)
        yield return StartCoroutine(BlinkEyelids(0.7f, 1f, 0.15f));
        yield return new WaitForSeconds(0.3f);

        // PHA 2: Cố gắng mở to hơn (Kéo ScaleY xuống 0.4)
        yield return StartCoroutine(BlinkEyelids(1f, 0.4f, 0.3f));
        yield return new WaitForSeconds(0.1f);
        // Lại nhắm lại vì mỏi
        yield return StartCoroutine(BlinkEyelids(0.4f, 1f, 0.15f));
        yield return new WaitForSeconds(0.2f);

        // PHA 3: Mở mắt hoàn toàn một cách từ từ (ScaleY về 0)
        yield return StartCoroutine(BlinkEyelids(1f, 0f, 2.0f));

        // Trả lại quyền điều khiển cho người chơi
        PlayerMovement player2 = FindFirstObjectByType<PlayerMovement>();
        if (player2 != null)
        {
            player2.canWalk = true;
            player2.canLook = true;
        }

        Debug.Log("Đã tỉnh dậy hoàn toàn!");
    }

    // Hàm hỗ trợ nội suy mượt mà (SmoothStep) cho mí mắt
    IEnumerator BlinkEyelids(float startY, float endY, float duration)
    {
        float timer = 0f;
        Vector3 startScale = new Vector3(1, startY, 1);
        Vector3 endScale = new Vector3(1, endY, 1);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Dùng SmoothStep để tạo quán tính: mí mắt bắt đầu chậm, nhanh ở giữa, chậm lại khi kết thúc
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            if (topEyelid != null) topEyelid.transform.localScale = Vector3.Lerp(startScale, endScale, smoothProgress);
            if (bottomEyelid != null) bottomEyelid.transform.localScale = Vector3.Lerp(startScale, endScale, smoothProgress);

            yield return null;
        }

        // Chốt giá trị cuối để không bị lệch do sai số dấu phẩy động
        if (topEyelid != null) topEyelid.transform.localScale = endScale;
        if (bottomEyelid != null) bottomEyelid.transform.localScale = endScale;
    }
}