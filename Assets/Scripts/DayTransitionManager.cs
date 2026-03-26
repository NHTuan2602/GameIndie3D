using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DayTransitionManager : MonoBehaviour
{
    public static DayTransitionManager instance;

    [Header("Cài đặt UI")]
    public CanvasGroup fadeCanvasGroup;
    public TextMeshProUGUI dayText;

    [Header("Cài đặt Thời gian")]
    public float fadeDuration = 1.5f;
    public float blackScreenDuration = 2f;

    private bool isTransitioning = false;

    void Awake()
    {
        // Singleton để GameManager có thể gọi từ bất cứ đâu
        if (instance == null) instance = this;
    }

    void Start()
    {
        // Khởi đầu phải trong suốt và không chặn chuột
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        if (dayText != null) dayText.text = "";
    }

    // Hàm chủ đạo để gọi từ GameManager
    public void StartTransition(string nextSceneName)
    {
        if (!isTransitioning)
        {
            StopAllCoroutines(); // Tránh việc gọi chồng chéo
            StartCoroutine(TransitionRoutine(nextSceneName));
        }
    }

    IEnumerator TransitionRoutine(string nextSceneName)
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true; // Chặn bấm nút bậy bạ

        // 1. Mờ dần sang đen (0 -> 1)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // 2. Cập nhật chữ Ngày và LOAD SCENE NGẦM
        if (GameManager.instance != null)
        {
            dayText.text = "NGÀY " + GameManager.instance.currentDay;
        }

        // Đợi 1 nhịp để đảm bảo chữ đã hiện lên rồi mới Load màn mới
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextSceneName);

        // 3. Đứng chờ ở màn hình đen cho người chơi nghỉ ngơi
        yield return new WaitForSeconds(blackScreenDuration);

        // Xóa chữ trước khi sáng lại
        dayText.text = "";

        // 4. Màn hình từ từ sáng lại (1 -> 0)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;

        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }
}