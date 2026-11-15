using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BossIntroManager : MonoBehaviour
{
    [Header("--- UI QUẢN LÝ DẰN MẶT ---")]
    public GameObject introPanel; // Tấm nền đen che toàn màn hình
    public TextMeshProUGUI txtDialogue;
    public Button btnStartWork; // Nút: "Rõ, thưa sếp!"

    [Header("--- ÂM THANH ---")]
    public AudioSource audioSource;
    public AudioClip slamDeskSound; // Tiếng đập bàn hoặc roi điện nẹt tạch tạch
    public AudioClip typingSound;

    [TextArea(3, 5)]
    public string bossDialogue = "Tỉnh lại rồi hả, thằng sinh viên năm nhất? Chào mừng tới <color=#FFD166>Khu tự trị Tam Giác Vàng</color>!\nThằng cốt của mày cầm tiền hoa hồng bán đứng mày rồi cút thẳng về nước rồi. Đừng mơ tới trường học hay tiền viện phí nữa — ở đây, mạng mày thuộc về bọn tao.\nMỗi ngày mày phải gõ lừa đủ ít nhất <color=red>2 con mồi</color>. Đủ KPI thì có cơm ăn mà sống qua <color=red>5 ngày đêm</color>, còn lười biếng hoặc từ chối mục tiêu? Đừng trách tao chích cây roi điện này tới chết!\nNGỒI VÀO MÁY VÀ BẮT ĐẦU LÀM VIỆC!";

    void Start()
    {
        // 1. CHỈ KÍCH HOẠT VÀO SÁNG NGÀY 1
        if (GameManager.instance != null && GameManager.instance.currentDay == 1 && GameManager.instance.currentPhase == GamePhase.Morning)
        {
            introPanel.SetActive(true);
            btnStartWork.gameObject.SetActive(false);
            StartCoroutine(PlayIntroRoutine());
        }
        else
        {
            introPanel.SetActive(false);
        }

        if (btnStartWork != null)
            btnStartWork.onClick.AddListener(CloseIntro);
    }

    // =======================================================
    // ĐÃ FIX: DÙNG UPDATE ĐỂ BẢO VỆ CON CHUỘT VÀ LẮNG NGHE PHÍM
    // =======================================================
    void Update()
    {
        // Chỉ chạy các lệnh này nếu bảng của Sếp đang mở
        if (introPanel != null && introPanel.activeSelf)
        {
            // 1. LIÊN TỤC ÉP HIỆN CHUỘT (Chống lại các script khác giấu chuột)
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 2. CHO PHÉP BẤM ESC HOẶC ENTER ĐỂ TẮT BẢNG (Chỉ hoạt động khi nút đã hiện ra)
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            {
                if (btnStartWork != null && btnStartWork.gameObject.activeSelf)
                {
                    CloseIntro();
                }
            }
        }
    }

    IEnumerator PlayIntroRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (audioSource != null && slamDeskSound != null)
            audioSource.PlayOneShot(slamDeskSound, 1f);

        txtDialogue.text = bossDialogue;
        txtDialogue.maxVisibleCharacters = 0;
        txtDialogue.ForceMeshUpdate();
        int totalChars = txtDialogue.textInfo.characterCount;

        for (int i = 0; i <= totalChars; i++)
        {
            txtDialogue.maxVisibleCharacters = i;

            if (audioSource != null && typingSound != null && i % 2 == 0)
                audioSource.PlayOneShot(typingSound, 0.3f);

            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(1f);
        btnStartWork.gameObject.SetActive(true);
    }

    void CloseIntro()
    {
        introPanel.SetActive(false);

        // Khi đóng bảng thoại, trả lại quyền ẩn chuột cho game chơi bình thường
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}