using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

// ==========================================
// ĐÂY LÀ KHU VỰC CHẾ TẠO "KHUÔN ĐÚC" KỊCH BẢN
// ==========================================
[System.Serializable] // Lệnh này ép Unity phải hiển thị cái khuôn này ra Inspector
public class StoryLine
{
    [TextArea(2, 4)] // Tạo ô nhập chữ to để dễ gõ xuống dòng
    public string sentence;

    [Tooltip("Số giây câu thoại này nằm trên màn hình")]
    public float displayTime = 2f;
}

public class CinematicDirector : MonoBehaviour
{
    [Header("Chuyển hướng")]
    public string nextSceneName = "SupermarketScene";

    [Header("Giao diện (UI)")]
    public CanvasGroup blackScreenGroup;
    public TextMeshProUGUI subtitleText;

    // ==========================================
    // KHU VỰC NHẬP KỊCH BẢN TỪ INSPECTOR (Không cần sửa code nữa)
    // ==========================================
    [Header("📝 KỊCH BẢN: PHẦN 1 (BẠO HÀNH)")]
    public AudioClip abuseAudio;
    public StoryLine[] abuseStoryLines; // Danh sách các câu thoại lúc cãi nhau

    [Header("📝 KỊCH BẢN: PHẦN 2 (BỆNH VIỆN)")]
    public AudioClip heartbeatThump;
    public AudioClip ecgBeepAudio;
    public StoryLine[] hospitalStoryLines; // Danh sách các câu thoại lúc ở bệnh viện

    [Header("📝 KỊCH BẢN: PHẦN 3 (CHẾT)")]
    public AudioClip flatlineAudio;
    public float flatlineDuration = 3f; // Thời gian màn hình đen kéo dài tiếng tít

    private AudioSource audioSource;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        audioSource = gameObject.AddComponent<AudioSource>();
        subtitleText.text = "";
        blackScreenGroup.alpha = 1;

        StartCoroutine(PlayCinematicSequence());
    }

    IEnumerator PlayCinematicSequence()
    {
        // ==========================================
        // PHÂN ĐOẠN 1: KÝ ỨC BẠO HÀNH
        // ==========================================
        yield return new WaitForSeconds(1f);
        if (abuseAudio != null) audioSource.PlayOneShot(abuseAudio);

        // Vòng lặp tự động đọc tất cả các câu thoại bạn nhập trong Inspector
        foreach (StoryLine line in abuseStoryLines)
        {
            yield return StartCoroutine(FadeText(line.sentence, 0.5f)); // Hiện chữ
            yield return new WaitForSeconds(line.displayTime); // Chờ theo thời gian bạn setup
        }

        yield return StartCoroutine(FadeText("", 0.5f)); // Tắt phụ đề

        // ==========================================
        // PHÂN ĐOẠN 2: BỆNH VIỆN
        // ==========================================
        yield return new WaitForSeconds(0.5f);
        if (heartbeatThump != null) audioSource.PlayOneShot(heartbeatThump);

        if (ecgBeepAudio != null)
        {
            audioSource.clip = ecgBeepAudio;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Tự động đọc kịch bản bệnh viện
        foreach (StoryLine line in hospitalStoryLines)
        {
            yield return StartCoroutine(FadeText(line.sentence, 1f));
            yield return new WaitForSeconds(line.displayTime);
        }

        // ==========================================
        // PHÂN ĐOẠN 3: FLATLINE (TỬ VONG)
        // ==========================================
        audioSource.Stop();
        if (flatlineAudio != null) audioSource.PlayOneShot(flatlineAudio);

        yield return StartCoroutine(FadeText("", 0.1f));

        yield return new WaitForSeconds(flatlineDuration);

        // ==========================================
        // PHÂN ĐOẠN 4: CHUYỂN CẢNH
        // ==========================================
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeText(string newText, float fadeDuration)
    {
        float timer = 0;
        Color c = subtitleText.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, timer / fadeDuration);
            subtitleText.color = c;
            yield return null;
        }

        subtitleText.text = newText;
        timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, timer / fadeDuration);
            subtitleText.color = c;
            yield return null;
        }
    }
}