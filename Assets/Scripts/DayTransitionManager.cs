using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Chế Độ Test (Dành cho Dev)")]
    [Tooltip("Tích vào đây để test hiệu ứng màn hình đen MÀ KHÔNG CHUYỂN SCENE")]
    public bool testModeNoLoad = false;

    void Awake()
    {
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
        Canvas myCanvas = GetComponentInChildren<Canvas>();
        if (myCanvas != null) myCanvas.sortingOrder = 999;

        if (fadeCanvasGroup != null)
        {
            // Ép buộc cái Panel phải có màu đen tuyệt đối
            Image bgImage = fadeCanvasGroup.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = new Color(0f, 0f, 0f, 1f);
            }

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
        Debug.Log("<color=magenta>DAY TRANSITION: Bắt đầu kéo rèm đen!</color>");
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

        // BƯỚC 3: LOAD SCENE (HOẶC BỎ QUA NẾU ĐANG BẬT TEST MODE)
        if (testModeNoLoad)
        {
            Debug.Log("<color=yellow>ĐANG BẬT TEST MODE: Bỏ qua bước Load Scene để test UI!</color>");
        }
        else
        {
            Debug.Log("<color=magenta>DAY TRANSITION: Bắt đầu Load Scene ngầm...</color>");
            AsyncOperation asyncLoad = null;

            // ĐÃ FIX: Chỉ dùng try-catch để lấy lệnh load, lôi yield return ra ngoài!
            try
            {
                asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("LỖI CHÍ MẠNG: Chưa Add Scene vào Build Settings!\n" + e.Message);
            }

            // Nằm ngoài khối try-catch, Unity sẽ không báo lỗi CS1626 nữa
            if (asyncLoad != null)
            {
                while (!asyncLoad.isDone) yield return null;
            }
        }

        // BƯỚC 4: Đứng chờ tĩnh lặng ở màn hình đen
        yield return new WaitForSecondsRealtime(blackScreenDuration);
        if (dayText != null) dayText.text = "";

        // BƯỚC 5: Sáng dần lên
        Debug.Log("<color=magenta>DAY TRANSITION: Mở rèm, trả lại game!</color>");
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