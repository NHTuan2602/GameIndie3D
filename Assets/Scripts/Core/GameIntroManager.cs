using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameIntroManager : MonoBehaviour
{
    [Header("Cài đặt Màn hình Intro")]
    [Tooltip("Kéo các Canvas Group (Cảnh báo, Logo...) vào đây theo thứ tự muốn hiện")]
    public CanvasGroup[] introElements;

    [Header("Thời gian (Giây)")]
    public float fadeDuration = 1.5f;    // Thời gian từ mờ sang rõ
    public float displayDuration = 2.5f; // Thời gian đứng im trên màn hình

    [Header("Chuyển Scene")]
    public string nextSceneName = "MainMenu"; // Điền tên Scene Main Menu của bạn vào đây

    private bool isSkipping = false;

    void Start()
    {
        // Giấu toàn bộ UI ngay khi bắt đầu
        foreach (var element in introElements)
        {
            element.alpha = 0f;
            element.gameObject.SetActive(false);
        }

        // Khóa chuột cho giống phim điện ảnh
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(PlayIntroSequence());
    }

    void Update()
    {
        // Bấm phím hoặc chuột bất kỳ để SKIP thẳng vào Game
        if (Input.anyKeyDown && !isSkipping)
        {
            isSkipping = true;
            StopAllCoroutines(); // Dừng ngay lập tức mọi hiệu ứng mờ ảo
            LoadNextScene();
        }
    }

    IEnumerator PlayIntroSequence()
    {
        // Chờ 0.5s cho game load ổn định rồi mới diễn
        yield return new WaitForSeconds(0.5f);

        foreach (var element in introElements)
        {
            element.gameObject.SetActive(true);

            // 1. Hiệu ứng Mờ -> Rõ (Fade In)
            yield return StartCoroutine(FadeCanvasGroup(element, 0f, 1f));

            // 2. Đứng im cho người chơi đọc
            yield return new WaitForSeconds(displayDuration);

            // 3. Hiệu ứng Rõ -> Mờ (Fade Out)
            yield return StartCoroutine(FadeCanvasGroup(element, 1f, 0f));

            element.gameObject.SetActive(false);
        }

        // Chạy xong hết thì sang Menu
        if (!isSkipping) LoadNextScene();
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }

    void LoadNextScene()
    {
        // Mở lại chuột để người chơi bấm Menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(nextSceneName);
    }
}