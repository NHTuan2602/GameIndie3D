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
    public string bossDialogue = "Chào mừng đến với địa ngục, ma mới! Ở cái xưởng này không có chỗ cho kẻ vô dụng phế thải.\nMỗi ngày mày phải lừa được ít nhất <color=red>2 con mồi</color>. Làm tốt thì có cơm ăn, còn không đạt KPI thì chuẩn bị tinh thần nếm mùi roi điện đi!\nNGỒI VÀO MÁY VÀ BẮT ĐẦU LÀM VIỆC!";

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
            // Các ngày khác hoặc ca Chiều thì dẹp luôn, vào làm việc ngay
            introPanel.SetActive(false);
        }

        if (btnStartWork != null)
            btnStartWork.onClick.AddListener(CloseIntro);
    }

    IEnumerator PlayIntroRoutine()
    {
        // Đợi màn hình Load xong 0.5s
        yield return new WaitForSeconds(0.5f);

        // Đập bàn / Nẹt điện thị uy
        if (audioSource != null && slamDeskSound != null)
            audioSource.PlayOneShot(slamDeskSound, 1f);

        // Chuẩn bị Text
        txtDialogue.text = bossDialogue;
        txtDialogue.maxVisibleCharacters = 0;
        txtDialogue.ForceMeshUpdate();
        int totalChars = txtDialogue.textInfo.characterCount;

        // Bắt đầu nhả chữ nhanh (Giọng điệu gắt gỏng)
        for (int i = 0; i <= totalChars; i++)
        {
            txtDialogue.maxVisibleCharacters = i;

            // Giảm tần suất tiếng lạch cạch để khỏi chói tai (phát âm thanh ở các ký tự chẵn)
            if (audioSource != null && typingSound != null && i % 2 == 0)
                audioSource.PlayOneShot(typingSound, 0.3f);

            yield return new WaitForSeconds(0.03f); // Tốc độ gõ khá nhanh
        }

        // Chờ 1 giây sau khi nói xong mới hiện nút chốt
        yield return new WaitForSeconds(1f);
        btnStartWork.gameObject.SetActive(true);
    }

    void CloseIntro()
    {
        // Tắt Panel, trả lại quyền điều khiển để người chơi tương tác với VictimSelection
        introPanel.SetActive(false);
    }
}