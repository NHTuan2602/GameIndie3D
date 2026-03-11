using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    // Bùa chú Singleton: Cho phép gọi lệnh chuyển cảnh từ bất kỳ file code nào
    public static SceneTransitionManager instance;

    [Header("Cài đặt UI")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.5f;

    void Awake()
    {
        // Kiểm tra xem đã có quản lý chuyển cảnh nào tồn tại chưa
        if (instance == null)
        {
            instance = this;
            // Dòng lệnh tối thượng: KHÔNG TIÊU DIỆT vật thể này khi chuyển Scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Nếu lỡ có 2 cái thì tiêu diệt cái mới sinh ra
            Destroy(gameObject);
        }
    }

    // Hàm public này sẽ được gọi khi bạn bấm nút "Đồng ý" đi Campuchia
    public void LoadNextScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // 1. CHUẨN BỊ: Chặn người chơi click bậy bạ lúc đang chuyển màn
        fadeCanvasGroup.blocksRaycasts = true;

        // 2. FADE OUT (MÀN HÌNH TỐI DẦN)
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // 3. TẢI SCENE NGẦM (KHÔNG ĐỒNG BỘ) - Chống đứng máy
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Đợi cho đến khi Scene mới tải xong 100% trong nền
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // --- LÚC NÀY SCENE XE KHÁCH ĐÃ LOAD XONG, NHƯNG MÀN HÌNH VẪN ĐANG ĐEN THUI ---

        // 4. FADE IN (MÀN HÌNH SÁNG LÊN TỪ TỪ Ở SCENE MỚI)
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;

        // 5. KẾT THÚC: Mở khóa cho người chơi tương tác
        fadeCanvasGroup.blocksRaycasts = false;
    }
}