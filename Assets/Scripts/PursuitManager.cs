using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PursuitManager : MonoBehaviour
{
    public static PursuitManager instance;

    [Header("Chỉ số Sinh tồn & Chiến đấu")]
    public float distanceRan = 0f;
    public float winDistance = 10f;
    public int enemiesRemaining = 10;
    public int currentEnemyHealth = 3;

    [Header("Kết nối UI & Object")]
    public EscapeBikeController player;
    public GameObject projectilePrefab;
    public TextMeshProUGUI statusUI;
    public GameObject killNotificationUI;
    public TextMeshProUGUI killMessageText;
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

        distanceRan += (player.forwardSpeed * Time.deltaTime) / 1000f;

        if (player.forwardSpeed <= 10f) GameOver("BỊ BẮT DO TỐC ĐỘ QUÁ THẤP!");

        // Chỉ hiện số Tốc độ theo đúng ý bạn
        if (statusUI != null) statusUI.text = $"{player.forwardSpeed:F0} km/h";
    }

    public void UseItemImmediately()
    {
        if (isGameOver) return;

        // Đẻ cục gạch (chỉ để bay cho đẹp mắt)
        if (projectilePrefab != null)
        {
            Instantiate(projectilePrefab, player.transform.position + Vector3.up, Quaternion.identity);
        }

        // Bắt đầu chờ 1 giây mới thực sự trừ máu
        StartCoroutine(HandleDamageAndNotification());
    }

    IEnumerator HandleDamageAndNotification()
    {
        yield return new WaitForSeconds(1f);
        EnemyTakeDamage(); // Gọi hàm public ở dưới
    }

    // ĐÃ THÊM PUBLIC CHO HÀM NÀY
    public void EnemyTakeDamage()
    {
        currentEnemyHealth--;
        Debug.Log("Địch trúng đòn! Máu còn: " + currentEnemyHealth);

        if (currentEnemyHealth <= 0)
        {
            enemiesRemaining--;
            ShowKillNotification($"ĐÃ TIÊU DIỆT XE TRUY ĐUỔI #{10 - enemiesRemaining}!");

            if (enemiesRemaining <= 0) WinGame();
            else currentEnemyHealth = 3;
        }
    }

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

    // ĐÃ THÊM PUBLIC CHO 2 HÀM NÀY
    public void GameOver(string r) { isGameOver = true; gameOverPanel.SetActive(true); Time.timeScale = 0; }
    public void WinGame() { isGameOver = true; winPanel.SetActive(true); Time.timeScale = 0; }
}