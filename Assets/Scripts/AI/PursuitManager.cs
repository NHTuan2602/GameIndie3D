using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    [Header("Âm thanh UI")]
    public AudioSource uiAudioSource;
    public AudioClip retrySound;

    private bool isGameOver = false;
    private bool isRetrying = false;

    void Awake() { instance = this; }

    void Start()
    {
        if (uiAudioSource == null) uiAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isGameOver)
        {
            if (gameOverPanel != null && gameOverPanel.activeSelf && !isRetrying)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    RetryGame();
                }
            }
            return;
        }

        distanceRan += (player.forwardSpeed * Time.deltaTime) / 1000f;

        if (player.forwardSpeed <= 10f) GameOver("BỊ BẮT DO TỐC ĐỘ QUÁ THẤP!");

        if (statusUI != null) statusUI.text = $"{player.forwardSpeed:F0} km/h";
    }

    public void RetryGame()
    {
        if (isRetrying) return;
        isRetrying = true;

        if (BikeAudioManager.instance != null)
        {
            BikeAudioManager.instance.StopAllSounds();
        }

        if (uiAudioSource != null && retrySound != null)
        {
            uiAudioSource.ignoreListenerPause = true;
            uiAudioSource.PlayOneShot(retrySound);
        }

        StartCoroutine(ReloadSceneDelayed());
    }

    IEnumerator ReloadSceneDelayed()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

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
        if (healthBar != null) healthBar.value = enemiesRemaining;
        if (BikeAudioManager.instance != null) BikeAudioManager.instance.PlayEnemyHurt();

        string pName = "BẠN";
        if (GameManager.instance != null) pName = GameManager.instance.playerName;

        int killedIndex = 10 - enemiesRemaining;
        ShowKillNotification($"{pName.ToUpper()} ĐÃ HẠ KẺ ĐỊCH THỨ {killedIndex}!");

        if (enemiesRemaining <= 0) WinGame();
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

    // ========================================================
    // 1. GAME OVER DO ĐÂM XE (ĐÓNG BĂNG NGAY LẬP TỨC)
    // ========================================================
    public void GameOver(string r)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (BikeAudioManager.instance != null)
        {
            if (BikeAudioManager.instance.bgmSource != null) BikeAudioManager.instance.bgmSource.Stop();
            BikeAudioManager.instance.PlayHit();
        }

        StartCoroutine(ShowGameOverDelayed());
    }

    IEnumerator ShowGameOverDelayed()
    {
        Time.timeScale = 0; // Đóng băng TỨC THÌ
        yield return new WaitForSecondsRealtime(1.5f);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // ========================================================
    // 2. GAME OVER DO RỚT VỰC (ĐỢI RƠI XONG MỚI ĐÓNG BĂNG)
    // ========================================================
    public void FallGameOver(string r)
    {
        if (isGameOver) return; // Chống spam lỗi Double Trigger
        isGameOver = true;

        if (BikeAudioManager.instance != null)
        {
            if (BikeAudioManager.instance.bgmSource != null) BikeAudioManager.instance.bgmSource.Stop();
            // Có thể thêm âm thanh la hét lúc rớt vực ở đây nếu có
        }

        StartCoroutine(ShowFallGameOverDelayed());
    }

    IEnumerator ShowFallGameOverDelayed()
    {
        // KHÔNG đóng băng Time.timeScale ở đây để xe có thời gian rớt xuống bởi trọng lực

        // Chờ 1.5 giây thời gian trong game cho xe rớt khuất màn hình
        yield return new WaitForSeconds(1.5f);

        // Lúc này mới đóng băng thời gian và hiện bảng Game Over
        Time.timeScale = 0;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.currentEnding = EndingType.TrueEscape;
        }

        if (BikeAudioManager.instance != null && BikeAudioManager.instance.bgmSource != null)
        {
            BikeAudioManager.instance.bgmSource.Stop();
        }

        StartCoroutine(TransitionToEndingScene());
    }

    IEnumerator TransitionToEndingScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("EndingScene");
    }
}