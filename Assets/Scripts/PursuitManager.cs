using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PursuitManager : MonoBehaviour
{
    public static PursuitManager instance;

    [Header("Chỉ số Truy đuổi")]
    public float pursuerDistance = 50f;
    public float catchDistance = 5f;
    public float winDistance = 150f;
    public float basePursuerSpeed = 30f;

    [Header("Kết nối Object & UI")]
    public EscapeBikeController player;
    public TextMeshProUGUI distanceUI;
    public TextMeshProUGUI statusUI; // <-- TÔI ĐÃ THÊM LẠI NÓ (Để hiển thị Tốc độ / Thông báo)
    public GameObject gameOverPanel;
    public GameObject winPanel;

    private bool isGameOver = false;

    void Awake() { instance = this; }

    void Update()
    {
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        // --- 1. LOGIC KHOẢNG CÁCH DỰA VÀO TỐC ĐỘ ---
        float speedDelta = player.forwardSpeed - basePursuerSpeed;
        pursuerDistance += speedDelta * Time.deltaTime;

        // --- 2. CẬP NHẬT GIAO DIỆN ---
        if (distanceUI != null)
            distanceUI.text = $"Địch cách: {pursuerDistance:F1}m\n(Mục tiêu: {winDistance}m)";

        // Cập nhật Đồng hồ tốc độ liên tục
        if (statusUI != null)
            statusUI.text = $"TỐC ĐỘ: {player.forwardSpeed} km/h";

        // --- 3. KIỂM TRA THẮNG / THUA ---
        if (pursuerDistance <= catchDistance)
        {
            GameOver("BẠN ĐÃ BỊ TÚM GỌN!\nNhấn SPACE để thử lại.");
        }
        else if (pursuerDistance >= winDistance)
        {
            WinGame("CẮT ĐUÔI THÀNH CÔNG!\nNhấn SPACE để chơi tiếp.");
        }
    }

    public void AddDistanceBonus(float bonusMeters)
    {
        pursuerDistance += bonusMeters;
        Debug.Log($"Né xe thành công! Thưởng {bonusMeters}m");
    }

    public void GameOver(string reason)
    {
        isGameOver = true;
        if (player != null) player.enabled = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // Hiện lý do chết lên màn hình
        if (statusUI != null) statusUI.text = $"<color=red>{reason}</color>";

        Time.timeScale = 0;
    }

    public void WinGame(string reason)
    {
        isGameOver = true;
        if (player != null) player.enabled = false;
        if (winPanel != null) winPanel.SetActive(true);

        // Hiện lý do thắng lên màn hình
        if (statusUI != null) statusUI.text = $"<color=yellow>{reason}</color>";

        Time.timeScale = 0;
    }
}