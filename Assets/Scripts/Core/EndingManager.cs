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
                finalDialogue = "Bạn đã sống sót khỏi cuộc bạo loạn và được thăng chức, trong lúc về quê ăn tết thì đã bị lực lượng chức năng bắt. Lưới trời lồng lộng, thưa nhưng khó thoát.";
                finalSprite = sprArrested;
                finalBgm = bgmArrested;
                break;

            case EndingType.RiotSurvivor:
                // Tính số lượng đồ đang có trong tay người chơi lúc này
                int itemCollected = 0;
                if (GameManager.instance != null)
                {
                    itemCollected = (GameManager.instance.hasNotebook ? 1 : 0) +
                                    (GameManager.instance.hasNippers ? 1 : 0) +
                                    (GameManager.instance.hasRope ? 1 : 0) +
                                    (GameManager.instance.hasKey ? 1 : 0);
                }

                // FIX: Gộp chung 3 và 4 món vào 1 ending Bạo Loạn Tẩu Thoát thành công
                if (itemCollected >= 3)
                {
                    dayLabel = "BẠO LOẠN TẨU THOÁT";
                    finalDialogue = "Khu trại xảy ra bạo loạn lớn! Dù trước đó bị lính gác phát hiện, nhưng nhờ sở hữu đủ công cụ sinh tồn cốt lõi, bạn đã lợi dụng trận hỗn chiến để tự cắt rào, bẻ khóa và trốn thoát thành công khỏi địa ngục!";
                    finalSprite = sprTrueEscape; // Sử dụng hình trốn thoát
                    finalBgm = bgmTrueEscape;    // Nhạc hào hùng chiến thắng
                }
                else
                {
                    // Fallback an toàn: Đáng lẽ GameManager đã đưa nhánh <=2 món sang Death. 
                    // Nhưng nếu rớt vào đây, nó sẽ tự trả về kết cục bi thảm do thiếu đồ.
                    dayLabel = "KẾT CỤC BI THẢM";
                    finalDialogue = "Khu trại xảy ra bạo loạn lớn... Vì chỉ gom được quá ít vật dụng phòng thân, bạn đã không thể sống sót qua cuộc hỗn chiến. BẠN ĐÃ BIẾN MẤT KHÔNG ĐỂ LẠI DẤU VẾT...";
                    finalSprite = sprDeath;
                    finalBgm = bgmDeath;
                }

                // Tiếng ồn bạo loạn chung cho nhánh này
                if (sfxAudioSource != null && riotSound != null) sfxAudioSource.PlayOneShot(riotSound);
                break;

            case EndingType.Death:
                dayLabel = "KẾT CỤC BI THẢM";
                finalDialogue = "Chống đối thất bại, máu xuống quá thấp hoặc thiếu công cụ sinh tồn... BẠN ĐÃ BIẾN MẤT KHÔNG ĐỂ LẠI DẤU VẾT...";
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
                finalSprite = sprDeath;
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

        StartCoroutine(TypeDialogue(finalDialogue, dayLabel));
    }

    IEnumerator TypeDialogue(string dialogue, string dayLabel)
    {
        txtDialogue.text = "";
        txtDialogue.gameObject.SetActive(true);

        foreach (char letter in dialogue.ToCharArray())
        {
            txtDialogue.text += letter;
            if (sfxAudioSource != null && typingSound != null)
            {
                sfxAudioSource.PlayOneShot(typingSound, 0.2f);
            }
            yield return new WaitForSeconds(0.04f);
        }

        yield return new WaitForSeconds(3f);
        txtDialogue.gameObject.SetActive(false);

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