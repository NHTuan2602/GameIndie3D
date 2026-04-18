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
    public int enemiesRemaining = 10; // Địch có 10 tên, tương đương 10 máu

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
        // Khởi tạo thanh máu (10 nấc tương ứng 10 tên địch)
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

        // ÉP THANH MÁU CHẠY THEO SỐ ĐỊCH CÒN LẠI
        if (healthBar != null) healthBar.value = enemiesRemaining;
    }

    public void UseItemImmediately()
    {
        if (isGameOver) return;

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
        EnemyTakeDamage();
    }

    public void EnemyTakeDamage()
    {
        enemiesRemaining--; // Trừ 1 địch (đồng thời thanh máu cũng tự tụt 1)

        // Lấy tên từ GameManager (nếu có), nếu test màn lẻ không có GameManager thì mặc định là "BẠN"
        string pName = "BẠN";
        if (GameManager.instance != null)
        {
            pName = GameManager.instance.playerName;
        }

        // Hiện thông báo: "TUẤN ĐÃ HẠ KẺ ĐỊCH THỨ 1"
        int killedIndex = 10 - enemiesRemaining;
        ShowKillNotification($"{pName.ToUpper()} ĐÃ HẠ KẺ ĐỊCH THỨ {killedIndex}!");

        if (enemiesRemaining <= 0)
        {
            WinGame();
        }
    }

    void ShowKillNotification(string message)
    {
        if (killNotificationUI != null)
        {
            killMessageText.text = message;
            killNotificationUI.SetActive(true);
            StopCoroutine("HideNotification");
            StartCoroutine(HideNotification(2.5f)); // Hiện 2.5 giây rồi tắt
        }
    }

    IEnumerator HideNotification(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (killNotificationUI != null) killNotificationUI.SetActive(false);
    }

    public void GameOver(string r) { isGameOver = true; gameOverPanel.SetActive(true); Time.timeScale = 0; }
    public void WinGame() { isGameOver = true; winPanel.SetActive(true); Time.timeScale = 0; }
}