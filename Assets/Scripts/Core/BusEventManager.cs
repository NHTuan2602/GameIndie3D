using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BusEventManager : MonoBehaviour
{
    [Header("Nhân vật Thằng Bạn")]
    public Transform friendBody;
    public Transform seatTarget;
    public float walkSpeed = 1.5f;

    // ==========================================
    // ĐÃ THÊM: Kết nối với bộ điều khiển Hoạt ảnh
    // ==========================================
    public Animator friendAnimator;

    [Header("Vật phẩm & Màn hình")]
    public GameObject waterBottle;
    public Image blackScreenFade;
    public string nextSceneName = "CampuchiaScene";

    [Header("Góc nhìn (Đã chốt cứng tọa độ chuẩn)")]
    public Camera playerCamera;

    [Header("--- KỊCH BẢN HỘI THOẠI TRÊN XE ---")]
    public DialogueLine[] introLines;

    private bool hasReachedSeat = false;

    void Start()
    {
        waterBottle.SetActive(false);

        if (blackScreenFade != null)
        {
            blackScreenFade.gameObject.SetActive(true);
            blackScreenFade.color = new Color(0, 0, 0, 0);
        }

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) player.canWalk = false;

        // Bật animation đi bộ ngay khi mới vào Scene
        if (friendAnimator != null) friendAnimator.SetBool("isWalking", true);
    }

    void Update()
    {
        if (!hasReachedSeat)
        {
            // Ép thằng bạn phải nhìn về phía cái ghế khi đang đi
            friendBody.LookAt(new Vector3(seatTarget.position.x, friendBody.position.y, seatTarget.position.z));

            // Di chuyển từ từ đến ghế
            friendBody.position = Vector3.MoveTowards(friendBody.position, seatTarget.position, walkSpeed * Time.deltaTime);

            if (Vector3.Distance(friendBody.position, seatTarget.position) < 0.05f)
            {
                hasReachedSeat = true;

                // ==========================================
                // ĐÃ FIX: ÉP GÓC XOAY ĐỂ NGỒI ĐÚNG HƯỚNG
                // ==========================================
                friendBody.position = seatTarget.position; // Khóa chặt vị trí vào ghế
                friendBody.rotation = seatTarget.rotation; // Bẻ lưng lại cho khớp với hướng ghế

                if (friendAnimator != null)
                {
                    friendAnimator.SetBool("isWalking", false); // Tắt đi bộ
                    friendAnimator.SetTrigger("Sit"); // Kích hoạt hoạt ảnh ngồi
                }

                StartCoroutine(DialogueAndWaterSequence());
            }
        }
    }

    IEnumerator DialogueAndWaterSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(introLines, null);

            while (DialogueManager.instance.dialoguePanel.activeSelf)
            {
                yield return null;
            }
        }

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) player.canWalk = false;

        if (playerCamera != null)
        {
            Rigidbody rb = waterBottle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            waterBottle.transform.SetParent(playerCamera.transform, false);
            waterBottle.transform.localPosition = new Vector3(0.7f, -0.72f, 1.29f);
            waterBottle.transform.localRotation = Quaternion.Euler(-8.7f, -18.57f, 10f);
            waterBottle.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        waterBottle.SetActive(true);
    }

    public void StartBlackout()
    {
        if (blackScreenFade == null) return;

        blackScreenFade.gameObject.SetActive(true);
        StartCoroutine(BlinkingBlackoutRoutine());
    }

    IEnumerator BlinkingBlackoutRoutine()
    {
        int blinkCount = 3;

        for (int i = 0; i < blinkCount; i++)
        {
            yield return StartCoroutine(FadeScreen(0f, 1f, 0.2f));
            yield return new WaitForSeconds(0.1f);

            float openSpeed = 0.5f + (i * 0.4f);
            yield return StartCoroutine(FadeScreen(1f, 0f, openSpeed));
            yield return new WaitForSeconds(0.3f);
        }

        yield return StartCoroutine(FadeScreen(0f, 1f, 2f));
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            if (blackScreenFade != null) blackScreenFade.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        if (blackScreenFade != null) blackScreenFade.color = new Color(0, 0, 0, endAlpha);
    }
}