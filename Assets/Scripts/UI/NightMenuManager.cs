using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class NightMenuManager : MonoBehaviour
{
    [Header("--- HIỂN THỊ CHỮ ---")]
    public TextMeshProUGUI txtTieuDe;
    [Tooltip("Hiển thị thông tin tổng quan chỉ số người chơi trong đêm")]
    public TextMeshProUGUI txtThongTinNhanVat;

    [Header("--- KẾT NỐI NÚT BẤM ---")]
    public Button btnNgu;
    public Button btnDanhBac;
    public Button btnThamThinh;

    [Header("--- GIAO DIỆN ---")]
    public GameObject nightMenuPanel;

    private CanvasGroup menuCanvasGroup;

    void Start()
    {
        if (nightMenuPanel != null)
        {
            menuCanvasGroup = nightMenuPanel.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null) menuCanvasGroup = nightMenuPanel.AddComponent<CanvasGroup>();
        }

        UpdateNightMenuDisplay();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (btnNgu != null) btnNgu.onClick.AddListener(ChonNgu);
        if (btnDanhBac != null) btnDanhBac.onClick.AddListener(ChonDanhBac);
        if (btnThamThinh != null) btnThamThinh.onClick.AddListener(ChonThamThinh);

        // Ẩn nút đánh bạc nếu cờ isCasinoLocked được bật
        if (GameManager.instance != null && GameManager.instance.isCasinoLocked)
        {
            if (btnDanhBac != null) btnDanhBac.gameObject.SetActive(false);
        }

        // Tự động thêm hiệu ứng phản hồi nút bấm
        UIButtonJuice.EnhanceAllButtonsIn(gameObject);

        OpenNightMenu();
    }

    private void UpdateNightMenuDisplay()
    {
        int day = GameManager.instance != null ? GameManager.instance.currentDay : 1;
        int hp = GameManager.instance != null ? GameManager.instance.hp : 100;
        int maxHp = GameManager.instance != null ? GameManager.instance.maxHp : 100;
        float money = GameManager.instance != null ? GameManager.instance.money : 0f;

        if (txtTieuDe != null)
        {
            txtTieuDe.text = $"<size=75%><color=#A8DADC>22:00 ĐÊM — KHU TỰ TRỊ TAM GIÁC VÀNG</color></size>\n" +
                             $"<b><color=#EF476F>ĐÊM THỨ {day}/5</color></b> - BẠN MUỐN LÀM GÌ?";
        }

        if (txtThongTinNhanVat != null)
        {
            txtThongTinNhanVat.text = $"Thể lực: <color=#06D6A0><b>{hp}/{maxHp} HP</b></color>  |  Tài sản: <color=#FFD166><b>{money:N0} VNĐ</b></color>";
        }

        // Tự động nâng cấp nhãn nút bấm nếu dùng TMP con
        FormatButtonLabel(btnNgu, "🛌  ĐI NGỦ SỚM", "Hồi phục thể lực, chuẩn bị cho ca làm ngày mai (An toàn)");
        FormatButtonLabel(btnDanhBac, "🎲  SÒNG BẠC NGẦM", "Thử vận may tại chiếu bạc đen (Nguy cơ nợ nần & bị bắt)");
        FormatButtonLabel(btnThamThinh, "🕵️  LÉN LÚT THÁM THÍNH", "Điều tra khu phức hợp tìm chìa khóa & đường thoát (Nguy hiểm)");
    }

    private void FormatButtonLabel(Button btn, string title, string sub)
    {
        if (btn == null) return;
        TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null && !tmp.text.Contains(sub))
        {
            tmp.text = $"<b>{title}</b>\n<size=65%><color=#D1D5DB>{sub}</color></size>";
        }
    }

    public void OpenNightMenu()
    {
        if (nightMenuPanel != null)
        {
            nightMenuPanel.SetActive(true);
            if (menuCanvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeInRoutine());
            }
        }
    }

    IEnumerator FadeInRoutine()
    {
        menuCanvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.25f);
            yield return null;
        }
        menuCanvasGroup.alpha = 1f;
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
