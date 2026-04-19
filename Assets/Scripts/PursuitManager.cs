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
    public GameObject winPanel;

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
            if (Input.GetKeyDown(KeyCode.Space))
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

        // COMBO BƯỚC 1 & 2: Vừa phát tiếng TING (Nhặt đồ) vừa phát tiếng VÚT (Ném luôn)
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPickup();
            AudioManager.instance.PlayThrow();
        }

        // Chờ 0.8 giây cho viên gạch bay trên không trung rồi mới tính sát thương
        StartCoroutine(HandleDamageAndNotification());
    }

    IEnumerator HandleDamageAndNotification()
    {
        yield return new WaitForSeconds(0.8f); // Đã giảm từ 1s xuống 0.8s cho cảm giác dứt khoát hơn
        EnemyTakeDamage();
    }

    public void EnemyTakeDamage()
    {
        enemiesRemaining--;

        // COMBO BƯỚC 3: Đổi thành tiếng KÍNH VỠ / ĐỊCH LA LÊN (Chứ không gọi tiếng rầm tông xe nữa)
        if (AudioManager.instance != null) AudioManager.instance.PlayEnemyHurt();

        string pName = "BẠN";
        if (GameManager.instance != null) pName = GameManager.instance.playerName;

        int killedIndex = 10 - enemiesRemaining;
        ShowKillNotification($"{pName.ToUpper()} ĐÃ HẠ KẺ ĐỊCH THỨ {killedIndex}!");

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

        if (AudioManager.instance != null && AudioManager.instance.bgmSource != null)
        {
            AudioManager.instance.bgmSource.Stop();
            AudioManager.instance.PlayHit(); // Đây mới là lúc dùng tiếng RẦM (Tông xe)
        }

        StartCoroutine(ShowGameOverDelayed());
    }

    IEnumerator ShowGameOverDelayed()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(1.5f);
        gameOverPanel.SetActive(true);
    }

    public void WinGame()
    {
        isGameOver = true;
        winPanel.SetActive(true);
        Time.timeScale = 0;
    }
}