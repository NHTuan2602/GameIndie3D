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

    void Awake()
    {
        // 1. CƠ CHẾ SINH TỒN: Tự động tiêu diệt bản sao nếu bị trùng
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 2. ÉP BUỘC HIỂN THỊ: Đẩy Canvas này lên lớp cao nhất (999) để che mọi thứ!
        Canvas myCanvas = GetComponentInChildren<Canvas>();
        if (myCanvas != null)
        {
            myCanvas.sortingOrder = 999;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        if (dayText != null) dayText.text = "";
    }

    public void StartTransition(string nextSceneName)
    {
        StopAllCoroutines();
        StartCoroutine(TransitionRoutine(nextSceneName));
    }

    IEnumerator TransitionRoutine(string nextSceneName)
    {
        if (fadeCanvasGroup != null) fadeCanvasGroup.blocksRaycasts = true;

        // BƯỚC 1: Mờ dần sang đen
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 1f;

        // BƯỚC 2: Hiện chữ Ngày
        if (GameManager.instance != null && dayText != null)
        {
            dayText.text = "NGÀY THỨ " + GameManager.instance.currentDay;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // BƯỚC 3: Load Màn Mới (Dùng try-catch để chặn lỗi nếu quên add Scene)
        AsyncOperation asyncLoad = null;
        try
        {
            asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("<color=red>LỖI CHÍ MẠNG: Bạn chưa Add Scene '" + nextSceneName + "' vào Build Settings!</color>\n" + e.Message);
        }

        if (asyncLoad != null)
        {
            while (!asyncLoad.isDone) yield return null;
        }

        // BƯỚC 4: Đứng chờ tĩnh lặng ở màn hình đen
        yield return new WaitForSecondsRealtime(blackScreenDuration);

        if (dayText != null) dayText.text = "";

        // BƯỚC 5: Sáng dần lên trả lại game
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}