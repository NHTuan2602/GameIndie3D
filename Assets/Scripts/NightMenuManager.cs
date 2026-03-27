using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class NightMenuManager : MonoBehaviour
{
    [Header("--- HIỂN THỊ CHỮ ---")]
    public TextMeshProUGUI txtTieuDe;

    [Header("--- KẾT NỐI NÚT BẤM ---")]
    public Button btnNgu;
    public Button btnDanhBac;
    public Button btnThamThinh;

    [Header("--- GIAO DIỆN ---")]
    public GameObject nightMenuPanel;

    void Start()
    {
        // Lấy dữ liệu Ngày từ Vị Thần GameManager để in ra màn hình
        if (txtTieuDe != null && GameManager.instance != null)
        {
            txtTieuDe.text = $"ĐÊM THỨ {GameManager.instance.currentDay}\n22:00 - BẠN MUỐN LÀM GÌ?";
        }

        // Đảm bảo chuột luôn hiển thị và tự do ở Menu này
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tự động cắm dây cho các nút
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
        // ĐÃ XÓA DÒNG KHÓA CHUỘT: Để chuột còn sống mà chơi game ngày hôm sau!
    }

    private void ChonNgu()
    {
        CloseNightMenu();
        // ĐÃ XÓA DÒNG CAMPUCHIA: Chỉ giao việc cho GameManager. GameManager sẽ tự gọi Màn Đen!
        if (GameManager.instance != null) GameManager.instance.SleepThroughNight();
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