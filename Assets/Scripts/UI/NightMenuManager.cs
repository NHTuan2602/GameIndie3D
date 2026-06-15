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
        if (txtTieuDe != null && GameManager.instance != null)
        {
            txtTieuDe.text = $"ĐÊM THỨ {GameManager.instance.currentDay}\n22:00 - BẠN MUỐN LÀM GÌ?";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (btnNgu != null) btnNgu.onClick.AddListener(ChonNgu);
        if (btnDanhBac != null) btnDanhBac.onClick.AddListener(ChonDanhBac);
        if (btnThamThinh != null) btnThamThinh.onClick.AddListener(ChonThamThinh);

        // ==========================================
        // ĐÃ FIX: ẨN NÚT ĐÁNH BẠC NẾU CỜ ISCASINOLOCKED ĐƯỢC BẬT
        // ==========================================
        if (GameManager.instance != null && GameManager.instance.isCasinoLocked)
        {
            if (btnDanhBac != null) btnDanhBac.gameObject.SetActive(false);
        }

        OpenNightMenu();
    }

    public void OpenNightMenu()
    {
        if (nightMenuPanel != null) nightMenuPanel.SetActive(true);
    }

    private void CloseNightMenu()
    {
        if (nightMenuPanel != null) nightMenuPanel.SetActive(false);
    }

    private void ChonNgu()
    {
        CloseNightMenu();
        if (GameManager.instance != null) GameManager.instance.SleepThroughNight();
    }

    private void ChonDanhBac()
    {
        CloseNightMenu();
        SceneManager.LoadScene("NightGameScreen");
    }

    private void ChonThamThinh()
    {
        CloseNightMenu();
        SceneManager.LoadScene("NightStealthScene");
    }
}