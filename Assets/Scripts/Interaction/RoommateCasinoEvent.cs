using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RoommateCasinoEvent : MonoBehaviour
{
    [Header("--- KẾT NỐI MENU 3 NÚT (ĐỂ ẨN ĐI VÀ CHẶN LỆNH) ---")]
    [Tooltip("Kéo object chứa chữ '22:00...' và 3 nút vào đây để tắt nó đi")]
    public GameObject nightMenuUI;
    public Button btnNgu;
    public Button btnDanhBac;
    public Button btnThamThinh;

    [Header("--- UI HỘI THOẠI RỦ RÊ ---")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI txtDialogue;
    public Button btnJoinCasino;
    public Button btnRefuse;

    [Header("--- ÂM THANH ---")]
    public AudioSource audioSource;
    public AudioClip typingSound;
    public AudioClip laughSound;

    [TextArea(3, 5)]
    public string inviteText = "Ê ma mới! Làm việc cả ngày mệt rã rời rồi đúng không? Lại đây, mấy anh em đang mở sới Tài Xỉu giải trí. Vào làm vài ván biết đâu gỡ lại tiền chuộc thân, đổi đời luôn! Vào đây!";
    private string refuseText = "Mày khinh anh em à? Ở cái trại này không có luật từ chối đâu. LẠI ĐÂY NGỒI XUỐNG!";

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // KIỂM TRA: KỊCH BẢN NÀY CHỈ CÓ TÁC DỤNG VÀO ĐÚNG ĐÊM 1
        if (GameManager.instance != null && GameManager.instance.currentDay == 1)
        {
            // ĐÃ FIX: Không tự động chạy Coroutine nữa. Chuyển sang "Cướp cò" 3 nút bấm.
            HijackNightMenuButtons();
        }
    }

    void HijackNightMenuButtons()
    {
        // Xóa sạch lệnh cũ của 3 nút, ép nó chạy hàm OnAnyActionClicked của chúng ta
        if (btnNgu != null) { btnNgu.onClick.RemoveAllListeners(); btnNgu.onClick.AddListener(OnAnyActionClicked); }
        if (btnDanhBac != null) { btnDanhBac.onClick.RemoveAllListeners(); btnDanhBac.onClick.AddListener(OnAnyActionClicked); }
        if (btnThamThinh != null) { btnThamThinh.onClick.RemoveAllListeners(); btnThamThinh.onClick.AddListener(OnAnyActionClicked); }
    }

    // Hàm này sẽ chạy khi người chơi bấm BẤT KỲ nút nào trong 3 nút vào Đêm 1
    public void OnAnyActionClicked()
    {
        // 1. TẮT menu 3 nút đi để bảng hội thoại không bị đè lên nhau (Chuẩn ý cô giáo)
        if (nightMenuUI != null) nightMenuUI.SetActive(false);

        // 2. Kích hoạt bảng rủ rê
        StartCoroutine(TriggerEventRoutine());
    }

    IEnumerator TriggerEventRoutine()
    {
        // ĐÃ XÓA thời gian chờ 4 giây. Bấm nút là hiện bảng rủ rê ngay lập tức.
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.canWalk = false;
            player.canLook = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (btnJoinCasino != null) btnJoinCasino.gameObject.SetActive(false);
        if (btnRefuse != null) btnRefuse.gameObject.SetActive(false);

        txtDialogue.text = "";
        foreach (char letter in inviteText.ToCharArray())
        {
            txtDialogue.text += letter;
            if (audioSource != null && typingSound != null) audioSource.PlayOneShot(typingSound, 0.4f);
            yield return new WaitForSeconds(0.04f);
        }

        yield return new WaitForSeconds(0.5f);

        if (btnJoinCasino != null)
        {
            btnJoinCasino.gameObject.SetActive(true);
            btnJoinCasino.onClick.RemoveAllListeners();
            btnJoinCasino.onClick.AddListener(GoToCasinoScene);
        }
        if (btnRefuse != null)
        {
            btnRefuse.gameObject.SetActive(true);
            btnRefuse.onClick.RemoveAllListeners();
            btnRefuse.onClick.AddListener(OnRefuseClicked);
        }
    }

    void OnRefuseClicked()
    {
        StopAllCoroutines();
        StartCoroutine(ForcePlayRoutine());
    }

    IEnumerator ForcePlayRoutine()
    {
        if (btnRefuse != null) btnRefuse.gameObject.SetActive(false);
        if (btnJoinCasino != null) btnJoinCasino.gameObject.SetActive(false);

        txtDialogue.text = "";
        txtDialogue.color = Color.red;

        if (audioSource != null && laughSound != null) audioSource.PlayOneShot(laughSound, 0.8f);

        foreach (char letter in refuseText.ToCharArray())
        {
            txtDialogue.text += letter;
            if (audioSource != null && typingSound != null) audioSource.PlayOneShot(typingSound, 0.5f);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.5f);
        if (btnJoinCasino != null)
        {
            btnJoinCasino.GetComponentInChildren<TextMeshProUGUI>().text = "Bấm để bước tới sới bạc...";
            btnJoinCasino.gameObject.SetActive(true);
        }
    }

    void GoToCasinoScene()
    {
        if (DayTransitionManager.instance != null)
        {
            DayTransitionManager.instance.StartTransition("NightGameScreen");
        }
        else
        {
            SceneManager.LoadScene("NightGameScreen");
        }
    }
}