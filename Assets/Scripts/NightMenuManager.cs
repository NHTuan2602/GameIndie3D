using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // KHAI BÁO THƯ VIỆN ĐỂ DÙNG TEXT

public class NightMenuManager : MonoBehaviour
{
    [Header("--- HIỂN THỊ CHỮ ---")]
    public TextMeshProUGUI txtTieuDe; // Lỗ cắm mới để kéo cái Text tiêu đề vào

    [Header("--- KẾT NỐI NÚT BẤM ---")]
    public Button btnNgu;
    public Button btnDanhBac;
    public Button btnThamThinh;

    [Header("--- GIAO DIỆN ---")]
    public GameObject nightMenuPanel;

    void Start()
    {
        // 1. Lấy dữ liệu Ngày từ Vị Thần GameManager để in ra màn hình
        if (txtTieuDe != null && GameManager.instance != null)
        {
            txtTieuDe.text = $"ĐÊM THỨ {GameManager.instance.currentDay}\n22:00 - BẠN MUỐN LÀM GÌ?";
        }

        // 2. Mở khóa chuột
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (btnNgu != null) btnNgu.onClick.AddListener(ChonNgu);
        if (btnDanhBac != null) btnDanhBac.onClick.AddListener(ChonDanhBac);
        if (btnThamThinh != null) btnThamThinh.onClick.AddListener(ChonThamThinh);

        OpenNightMenu();
    }

    public void OpenNightMenu()
    {
        if (nightMenuPanel != null) nightMenuPanel.SetActive(true);
    }

    private void CloseNightMenu()
    {
        if (nightMenuPanel != null) nightMenuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ChonNgu()
    {
        CloseNightMenu();
        if (GameManager.instance != null) GameManager.instance.SleepThroughNight();
        SceneManager.LoadScene("CampuchiaScene"); // Đổi đúng tên Scene ban ngày của bạn
    }

    private void ChonDanhBac()
    {
        CloseNightMenu();
        SceneManager.LoadScene("NightGameScreen"); // Đổi đúng tên Scene đánh bạc
    }

    private void ChonThamThinh()
    {
        CloseNightMenu();
        // SceneManager.LoadScene("StealthScene"); 
    }
}