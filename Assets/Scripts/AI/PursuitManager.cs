using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PursuitManager : MonoBehaviour
{
    public static PursuitManager instance;

    [Header("Chỉ số Sinh tồn & Chiến đấu")]
    public float distanceRan = 0f;
    public float winDistance = 10f;
    public int enemiesRemaining = 10;

    [Header("Kết nối UI & Object")]
    public EscapeBikeController player;
    public GameObject projectilePrefab;
    public TextMeshProUGUI statusUI;
    public Slider healthBar;
    public GameObject killNotificationUI;
    public TextMeshProUGUI killMessageText;
    public GameObject gameOverPanel;
    // public GameObject winPanel; // TẠM ẨN: Không dùng panel Win cũ nữa vì đã chuyển sang Ending Screen

    private bool isGameOver = false;

    void Awake() { instance = this; }

    void Start()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = 10;
            healthBar.value = enemiesRemaining;
        }
    }

    void Update()
    {
        if (isGameOver)
        {
            // Nếu GameOver (Bị bắt/Tông xe), ấn Space để chơi lại màn đua xe
            if (Input.GetKeyDown(KeyCode.Space) && gameOverPanel.activeSelf)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        distanceRan += (player.forwardSpeed * Time.deltaTime) / 1000f;

        if (player.forwardSpeed <= 10f) GameOver("BỊ BẮT DO TỐC ĐỘ QUÁ THẤP!");

        if (statusUI != null) statusUI.text = $"{player.forwardSpeed:F0} km/h";

        if (healthBar != null) healthBar.value = enemiesRemaining;
    }

    // ================== COMBO ÂM THANH NÉM GẠCH ==================
    public void UseItemImmediately()
    {
        if (isGameOver) return;

        if (projectilePrefab != null)
        {
            Instantiate(projectilePrefab, player.transform.position + Vector3.up, Quaternion.identity);
        }

        if (BikeAudioManager.instance != null)
        {
            BikeAudioManager.instance.PlayPickup();
            BikeAudioManager.instance.PlayThrow();
        }

        StartCoroutine(HandleDamageAndNotification());
    }

    IEnumerator HandleDamageAndNotification()
    {
        yield return new WaitForSeconds(0.8f);
        EnemyTakeDamage();
    }

    public void EnemyTakeDamage()
    {
        enemiesRemaining--;

        if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayEnemyHurt();

        string pName = "BẠN";
        if (GameManager.instance != null) pName = GameManager.instance.playerName;

        int killedIndex = 10 - enemiesRemaining;
        ShowKillNotification($"{pName.ToUpper()} ĐÃ HẠ KẺ ĐỊCH THỨ {killedIndex}!");

        // Khi thanh máu về 0 -> Kích hoạt WinGame
        if (enemiesRemaining <= 0) WinGame();
    }
    // =============================================================

    void ShowKillNotification(string message)
    {
        if (killNotificationUI != null)
        {
            killMessageText.text = message;
            killNotificationUI.SetActive(true);
            StopCoroutine("HideNotification");
            StartCoroutine(HideNotification(2.5f));
        }
    }

    IEnumerator HideNotification(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (killNotificationUI != null) killNotificationUI.SetActive(false);
    }

    public void GameOver(string r)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (BikeAudioManager.instance != null && BikeAudioManager.instance.bgmSource != null)
        {
            BikeAudioManager.instance.bgmSource.Stop();
            BikeAudioManager.instance.PlayHit();
        }

        StartCoroutine(ShowGameOverDelayed());
    }

    IEnumerator ShowGameOverDelayed()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(1.5f);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // =============================================================
    // ĐÃ FIX: CHUYỂN CẢNH ENDING KHI THẮNG (MÁU ĐỊCH = 0)
    // =============================================================
    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        // 1. Ép GameManager ghi nhận kết cục Tự Do (TrueEscape)
        if (GameManager.instance != null)
        {
            GameManager.instance.currentEnding = EndingType.TrueEscape;
        }

        // 2. Tắt nhạc đua xe dồn dập đi
        if (BikeAudioManager.instance != null && BikeAudioManager.instance.bgmSource != null)
        {
            BikeAudioManager.instance.bgmSource.Stop();
        }

        // 3. Gọi Coroutine để chờ 1.5 giây rồi mới đá sang màn Ending
        StartCoroutine(TransitionToEndingScene());
    }

    IEnumerator TransitionToEndingScene()
    {
        // Đợi 1.5 giây để người chơi nhìn thấy thông báo Hạ kẻ địch thứ 10
        yield return new WaitForSeconds(1.5f);

        // Lưu ý: Tên "EndingScene" trong ngoặc kép phải CHÍNH XÁC với tên file Scene Ending của bạn ngoài thư mục Scenes.
        SceneManager.LoadScene("EndingScene");
    }
}