using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RoommateCasinoEvent : MonoBehaviour
{
    [Header("--- UI HỘI THOẠI RỦ RÊ ---")]
    public GameObject dialoguePanel; // Khung đen mờ chứa hội thoại
    public TextMeshProUGUI txtDialogue;
    public Button btnJoinCasino; // Nút: "Ngồi vào sới"
    public Button btnRefuse; // Nút: "Từ chối"

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

        // KỊCH BẢN NÀY CHỈ ĐƯỢC KÍCH HOẠT VÀO ĐÚNG ĐÊM NGÀY 1
        if (GameManager.instance != null && GameManager.instance.currentDay == 1)
        {
            StartCoroutine(TriggerEventRoutine());
        }
    }

    IEnumerator TriggerEventRoutine()
    {
        // 1. Đợi 4 giây cho hệ thống WakeUpManager mở mắt người chơi xong hoàn toàn
        yield return new WaitForSeconds(4.0f);

        // 2. Khóa không cho người chơi di chuyển hay xoay chuột đi chỗ khác
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.canWalk = false;
            player.canLook = false;
        }

        // Mở khóa chuột để bấm nút
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Bật Panel hội thoại lên
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (btnJoinCasino != null) btnJoinCasino.gameObject.SetActive(false);
        if (btnRefuse != null) btnRefuse.gameObject.SetActive(false);

        // 4. Bắt đầu gõ chữ mồi chài
        txtDialogue.text = "";
        foreach (char letter in inviteText.ToCharArray())
        {
            txtDialogue.text += letter;
            if (audioSource != null && typingSound != null) audioSource.PlayOneShot(typingSound, 0.4f);
            yield return new WaitForSeconds(0.04f);
        }

        yield return new WaitForSeconds(0.5f);

        // Hiện nút bấm
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
        // Nếu người chơi cứng đầu bấm "Từ chối" -> Ép buộc phải chơi
        StopAllCoroutines();
        StartCoroutine(ForcePlayRoutine());
    }

    IEnumerator ForcePlayRoutine()
    {
        if (btnRefuse != null) btnRefuse.gameObject.SetActive(false);
        if (btnJoinCasino != null) btnJoinCasino.gameObject.SetActive(false);

        txtDialogue.text = "";
        txtDialogue.color = Color.red; // Chữ chuyển sang màu đỏ máu đe dọa

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
            // Thay đổi Text của nút thành sự ép buộc
            btnJoinCasino.GetComponentInChildren<TextMeshProUGUI>().text = "Bấm để bước tới sới bạc...";
            btnJoinCasino.gameObject.SetActive(true);
        }
    }

    void GoToCasinoScene()
    {
        // Chuyển thẳng sang phân cảnh Minigame Tài Xỉu
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