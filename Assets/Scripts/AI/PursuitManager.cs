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

    [Header("Âm thanh UI (MỚI)")]
    public AudioSource uiAudioSource; // Kéo thả AudioSource vào đây
    public AudioClip retrySound;      // Kéo file âm thanh tiếng Click/Ting vào đây

    private bool isGameOver = false;
    private bool isRetrying = false; // Biến chống spam nút bấm

    void Awake() { instance = this; }

    void Start()
    {
          /*  if (healthBar != null)
            {
                healthBar.maxValue = 10;
                healthBar.value = enemiesRemaining;
            }
*/
            // Tự động tìm AudioSource nếu bạn quên kéo
        if (uiAudioSource == null) uiAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isGameOver)
        {
            // Nếu GameOver, ấn Space để chơi lại (chỉ nhận lệnh 1 lần duy nhất)
            if (Input.GetKeyDown(KeyCode.Space) && gameOverPanel.activeSelf && !isRetrying)
            {
                RetryGame();
            }
            return;
        }

        distanceRan += (player.forwardSpeed * Time.deltaTime) / 1000f;

        if (player.forwardSpeed <= 10f) GameOver("BỊ BẮT DO TỐC ĐỘ QUÁ THẤP!");

        if (statusUI != null) statusUI.text = $"{player.forwardSpeed:F0} km/h";

        //if (healthBar != null) healthBar.value = enemiesRemaining;
    }

    // ================== HỆ THỐNG CHƠI LẠI (MỚI) ==================
    // Hàm public này có thể gắn thẳng vào Event OnClick() của UI Button nếu muốn dùng chuột!
    public void RetryGame()
    {
        if (isRetrying) return; // Đã bấm rồi thì không cho bấm chồng lên nữa
        isRetrying = true;

        if (uiAudioSource != null && retrySound != null)
        {
            uiAudioSource.ignoreListenerPause = true; // Cho phép phát tiếng dù game đang pause
            uiAudioSource.PlayOneShot(retrySound);
        }

        StartCoroutine(ReloadSceneDelayed());
    }

    IEnumerator ReloadSceneDelayed()
    {
        // Chờ 0.5 giây ĐỜI THỰC (Bỏ qua TimeScale = 0) để âm thanh kịp phát xong
        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // =============================================================

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