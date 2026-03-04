using System.Collections;
using UnityEngine;
using TMPro; // Bắt buộc phải có dòng này để điều khiển font chữ TextMeshPro

public class DayTransitionManager : MonoBehaviour
{
    [Header("Cài đặt UI")]
    public CanvasGroup fadeCanvasGroup;
    public TextMeshProUGUI dayText;

    [Header("Cài đặt Thời gian")]
    public float fadeDuration = 1.5f;       // Thời gian từ từ tối đen (giây)
    public float blackScreenDuration = 2f;  // Thời gian màn hình đen thui để đọc chữ (giây)

    private int currentDay = 1;             // Ngày hiện tại
    private bool isTransitioning = false;   // Biến cờ kiểm tra xem có đang chuyển ngày không

    void Start()
    {
        // Khi mới vào game, đảm bảo màn hình không bị che đen
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            // Tắt chức năng cản chuột bấm của màn đen (để bạn còn bấm nút UI khác được)
            fadeCanvasGroup.blocksRaycasts = false;
        }

        if (dayText != null)
        {
            dayText.text = "Day " + currentDay;
        }
    }

    void Update()
    {
        // [TEST] Nhấn phím T để giả lập việc hoàn thành 1 ngày và chuyển sang ngày hôm sau
        if (Input.GetKeyDown(KeyCode.T) && !isTransitioning)
        {
            StartCoroutine(TransitionToNextDay());
        }
    }

    // Coroutine: Chạy đa luồng theo thời gian thực để tạo hiệu ứng mượt mà
    public IEnumerator TransitionToNextDay()
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true; // Chặn người chơi thao tác khi màn hình đang đen

        // 1. Màn hình từ từ tối đen (Tăng Alpha từ 0 -> 1)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null; // Chờ 1 khung hình rồi tiếp tục vòng lặp
        }
        fadeCanvasGroup.alpha = 1f;

        // 2. Tăng số ngày lên và cập nhật chữ trên màn hình
        currentDay++;
        dayText.text = "Day " + currentDay;

        // --- TẠI ĐÂY BẠN CÓ THỂ GỌI CODE ĐỂ RESET VỊ TRÍ LÍNH GÁC, HOẶC PLAYER ---
        // Ví dụ: playerTarget.position = viTriXuatPhat;

        // 3. Đứng chờ màn hình đen một khoảng thời gian cho người chơi đọc chữ
        yield return new WaitForSeconds(blackScreenDuration);

        // 4. Màn hình từ từ sáng lại (Giảm Alpha từ 1 -> 0)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;

        fadeCanvasGroup.blocksRaycasts = false; // Trả lại quyền thao tác cho người chơi
        isTransitioning = false;
    }
}