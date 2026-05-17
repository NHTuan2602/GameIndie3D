using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("--- DEV TEST ---")]
    public bool enableTestMode = false;
    public EndingType testEndingToPlay = EndingType.TrueEscape;

    [Header("--- UI CHỮ ---")]
    public TextMeshProUGUI txtDayLabel;
    public TextMeshProUGUI txtDialogue;
    public GameObject btnReturnMenu;

    [Header("--- UI HÌNH ẢNH ENDING ---")]
    public Image imgEndingCard;
    public Sprite sprArrested;
    public Sprite sprRiotSurvivor;
    public Sprite sprDeath;
    public Sprite sprTrueEscape;

    [Header("--- NHẠC NỀN RIÊNG CHO TỪNG ENDING ---")]
    public AudioClip bgmArrested;
    public AudioClip bgmRiotSurvivor;
    public AudioClip bgmDeath;
    public AudioClip bgmTrueEscape;

    [Header("Cài đặt Âm thanh Hiệu ứng")]
    public AudioSource bgmAudioSource; // Nguồn phát nhạc nền (Nhạc chạy liên tục)
    public AudioSource sfxAudioSource; // Nguồn phát tiếng gõ chữ (Âm thanh ngắn)
    public AudioClip typingSound;
    public AudioClip riotSound;

    void Start()
    {
        if (btnReturnMenu != null) btnReturnMenu.SetActive(false);
        if (imgEndingCard != null) imgEndingCard.gameObject.SetActive(false);
        if (txtDayLabel != null) txtDayLabel.gameObject.SetActive(false);
        txtDialogue.text = "";

        StartCoroutine(PlayEndingSequence());
    }

    IEnumerator PlayEndingSequence()
    {
        string storyText = "Lỗi: Không tìm thấy kết cục...";
        string endingName = "UNKNOWN ENDING";
        Sprite finalImage = null;
        AudioClip finalBGM = null;

        // 1. XÁC ĐỊNH ENDING
        EndingType activeEnding = EndingType.None;

        if (enableTestMode)
        {
            activeEnding = testEndingToPlay;
        }
        else if (GameManager.instance != null)
        {
            activeEnding = GameManager.instance.currentEnding;
        }

        // Bỏ qua Ending Vay Tiền nếu đã xử lý ở Scene khác
        if (activeEnding == EndingType.BadCredit)
        {
            Debug.LogWarning("Ending Vay Tiền đã được xử lý bên màn khác, đang thoát luồng này...");
            yield break;
        }

        bool isDay6 = (activeEnding == EndingType.Arrested || activeEnding == EndingType.RiotSurvivor || activeEnding == EndingType.Death);

        if (isDay6)
        {
            txtDayLabel.text = "NGÀY THỨ 6...\n<size=50%>SỰ KIỆN BẠO LOẠN</size>";
            if (sfxAudioSource != null && riotSound != null) sfxAudioSource.PlayOneShot(riotSound);
        }
        else
        {
            txtDayLabel.text = "ĐÊM THỨ 5...";
        }

        txtDayLabel.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        txtDayLabel.gameObject.SetActive(false);

        // 3. XÁC ĐỊNH DỮ LIỆU TỪNG ENDING
        switch (activeEnding)
        {
            case EndingType.Arrested:
                storyText = "Khu trại xảy ra bạo loạn lớn...\nNhờ thành tích lừa đảo xuất sắc, tôi được tổ chức ưu ái bảo vệ an toàn.\nVài năm sau, mang theo số tiền bẩn khổng lồ, tôi đáp chuyến bay về nước ăn Tết...\nNhưng Cục An Ninh Mạng đã đợi sẵn ngay tại sảnh chờ. Lưới trời tuy thưa nhưng khó lọt.";
                endingName = "ENDING A: LƯỚI TRỜI";
                finalImage = sprArrested;
                finalBGM = bgmArrested;
                break;

            case EndingType.RiotSurvivor:
                storyText = "Khu trại xảy ra bạo loạn lớn!\nNhờ thể lực còn tốt và vài công cụ giấu sẵn trong người...\nTôi đã đánh gục lính gác và hòa vào dòng người lẩn trốn vào rừng sâu.\nTôi đã sống sót... nhưng bóng ma tâm lý sẽ còn ám ảnh mãi mãi.";
                endingName = "NEUTRAL ENDING: KẺ SỐNG SÓT";
                finalImage = sprRiotSurvivor;
                finalBGM = bgmRiotSurvivor;
                break;

            case EndingType.Death:
                storyText = "Khu trại xảy ra bạo loạn lớn...\nNhưng tôi đã quá kiệt sức sau những ngày làm việc cường độ cao.\nKhông có công cụ trong tay, tôi gục ngã giữa những tiếng la hét...\nMọi thứ mờ dần... Trò chơi kết thúc.";
                endingName = "BAD ENDING: TỬ VONG";
                finalImage = sprDeath;
                finalBGM = bgmDeath;
                break;

            case EndingType.TrueEscape:
                storyText = "Tôi đã thu thập đủ 4 món đồ nghề.\nCắt rào, đu dây và phóng xe tẩu thoát thành công trong màn đêm!\nTôi đã tự cứu lấy chính mình trước khi sự việc tồi tệ hơn.";
                endingName = "TRUE ENDING: TỰ DO";
                finalImage = sprTrueEscape;
                finalBGM = bgmTrueEscape;
                break;
        }

        // BẬT NHẠC NỀN CHÍNH THỨC
        if (bgmAudioSource != null && finalBGM != null)
        {
            bgmAudioSource.clip = finalBGM;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }

        // 4. CHẠY CHỮ KỂ CHUYỆN
        txtDialogue.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        foreach (char c in storyText)
        {
            txtDialogue.text += c;
            if (c != ' ' && c != '\n' && sfxAudioSource != null && typingSound != null)
            {
                sfxAudioSource.PlayOneShot(typingSound, 0.1f);
            }
            yield return new WaitForSeconds(0.04f);
        }

        yield return new WaitForSeconds(3.5f);
        txtDialogue.gameObject.SetActive(false);

        // 5. HIỆN TÊN ENDING VÀ BỨC ẢNH
        if (txtDayLabel != null)
        {
            txtDayLabel.text = "<size=150%><b><color=yellow>" + endingName + "</color></b></size>";
            txtDayLabel.gameObject.SetActive(true);
        }

        if (imgEndingCard != null && finalImage != null)
        {
            imgEndingCard.sprite = finalImage;
            imgEndingCard.gameObject.SetActive(true);
        }

        // 6. HIỆN NÚT CHƠI LẠI
        yield return new WaitForSeconds(1.5f);
        if (btnReturnMenu != null) btnReturnMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReturnToMainMenu()
    {
        if (GameManager.instance != null) Destroy(GameManager.instance.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
}