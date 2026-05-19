using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("--- DEV TEST ---")]
    public bool enableTestMode = false;
    public EndingType testEndingToPlay;

    [Header("--- UI CHỮ ---")]
    public TextMeshProUGUI txtDayLabel;
    public TextMeshProUGUI txtDialogue;
    public Button btnReturnMenu;

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
    public AudioSource bgmAudioSource;
    public AudioSource sfxAudioSource;
    public AudioClip typingSound;
    public AudioClip riotSound;

    void Start()
    {
        // Ép ẩn nút và tiêu đề ngay từ frame đầu tiên
        if (btnReturnMenu != null) btnReturnMenu.gameObject.SetActive(false);
        if (txtDayLabel != null) txtDayLabel.gameObject.SetActive(false);

        EndingType endingToPlay = GameManager.instance != null ? GameManager.instance.currentEnding : EndingType.None;

        if (enableTestMode)
        {
            endingToPlay = testEndingToPlay;
        }

        SetupEnding(endingToPlay);

        if (btnReturnMenu != null)
            btnReturnMenu.onClick.AddListener(ReturnToMainMenu);
    }

    void SetupEnding(EndingType ending)
    {
        string finalDialogue = "";
        Sprite finalSprite = null;
        AudioClip finalBgm = null;
        string dayLabel = "NGÀY CUỐI CÙNG";

        switch (ending)
        {
            case EndingType.Arrested:
                dayLabel = "LƯỚI TRỜI";
                finalDialogue = "Lưới trời lồng lộng, thưa mà khó lọt. Bạn đã bị lực lượng chức năng tóm gọn cùng toàn bộ đường dây lừa đảo...";
                finalSprite = sprArrested;
                finalBgm = bgmArrested;
                break;

            case EndingType.RiotSurvivor:
                dayLabel = "KẺ SỐNG SÓT";
                finalDialogue = "Khu trại xảy ra bạo loạn lớn... Nhờ thành tích lừa đảo xuất sắc, bạn giữ được mạng sống nhưng mãi mãi kẹt lại nơi địa ngục trần gian này.";
                finalSprite = sprRiotSurvivor;
                finalBgm = bgmRiotSurvivor;
                if (sfxAudioSource != null && riotSound != null) sfxAudioSource.PlayOneShot(riotSound);
                break;

            case EndingType.Death:
                dayLabel = "KẾT CỤC BI THẢM";
                finalDialogue = "Chống đối thất bại hoặc không hoàn thành chỉ tiêu... BẠN ĐÃ BIẾN MẤT KHÔNG ĐỂ LẠI DẤU VẾT...";
                finalSprite = sprDeath;
                finalBgm = bgmDeath;
                break;

            case EndingType.TrueEscape:
                dayLabel = "TỰ DO MANG TÊN BẠN";
                finalDialogue = "Kế hoạch vượt ngục hoàn hảo! Bạn đã trốn thoát thành công khỏi bầy quỷ dữ và báo ngay cho cảnh sát.";
                finalSprite = sprTrueEscape;
                finalBgm = bgmTrueEscape;
                break;

            case EndingType.BadCredit:
                dayLabel = "CON NỢ";
                finalDialogue = "Bị cuốn vào vòng xoáy cờ bạc, bạn đã bán mình cho tổ chức tín dụng đen và bị chuyển đi nơi khác...";
                break;
        }

        if (imgEndingCard != null && finalSprite != null)
        {
            imgEndingCard.sprite = finalSprite;
            imgEndingCard.color = new Color(1f, 1f, 1f, 1f);
        }

        if (bgmAudioSource != null && finalBgm != null)
        {
            bgmAudioSource.clip = finalBgm;
            bgmAudioSource.Play();
        }

        // Truyền cả đoạn text Tiêu đề vào luồng Coroutine để đợi
        StartCoroutine(TypeDialogue(finalDialogue, dayLabel));
    }

    IEnumerator TypeDialogue(string dialogue, string dayLabel)
    {
        txtDialogue.text = "";

        // Đảm bảo chữ được bật lên khi bắt đầu gõ
        txtDialogue.gameObject.SetActive(true);

        foreach (char letter in dialogue.ToCharArray())
        {
            txtDialogue.text += letter;

            if (sfxAudioSource != null && typingSound != null)
            {
                sfxAudioSource.PlayOneShot(typingSound, 0.2f);
            }

            yield return new WaitForSeconds(0.04f); // Tốc độ gõ chữ
        }

        // ĐÃ FIX: Chờ 3 giây để người chơi kịp đọc hết câu
        yield return new WaitForSeconds(3f);

        // Tắt đoạn text đi để trả lại background sạch sẽ
        txtDialogue.gameObject.SetActive(false);

        // Sau đó mới BẬT Tiêu đề Ending và Nút bấm lên
        if (txtDayLabel != null)
        {
            txtDayLabel.text = dayLabel;
            txtDayLabel.gameObject.SetActive(true);
        }

        if (btnReturnMenu != null)
        {
            btnReturnMenu.gameObject.SetActive(true);
        }
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}