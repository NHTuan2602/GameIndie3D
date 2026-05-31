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

    [Header("--- UI NÚT BẤM ---")]
    public Button btnReturnMenu;
    public Button btnRestartDay;

    [Header("--- TÊN SCENE CHƠI GAME CHÍNH ---")]
    public string mainGameplaySceneName = "ScamScreen";

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
        // ========================================================
        // ĐÃ FIX: ÉP MỞ KHÓA VÀ HIỆN CON TRỎ CHUỘT KHI LOAD ENDING
        // ========================================================
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (btnReturnMenu != null) btnReturnMenu.gameObject.SetActive(false);
        if (btnRestartDay != null) btnRestartDay.gameObject.SetActive(false);

        // Tạm thời tắt các UI chữ lúc mới Load Scene
        if (txtDayLabel != null) txtDayLabel.gameObject.SetActive(false);
        if (txtDialogue != null) txtDialogue.gameObject.SetActive(false);

        EndingType endingToPlay = GameManager.instance != null ? GameManager.instance.currentEnding : EndingType.None;

        if (enableTestMode)
        {
            endingToPlay = testEndingToPlay;
        }

        SetupEnding(endingToPlay);

        if (btnReturnMenu != null)
            btnReturnMenu.onClick.AddListener(ReturnToMainMenu);

        if (btnRestartDay != null)
            btnRestartDay.onClick.AddListener(RestartCurrentDay);
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
                int itemCollected = 0;
                if (GameManager.instance != null)
                {
                    itemCollected = (GameManager.instance.hasNotebook ? 1 : 0) +
                                    (GameManager.instance.hasNippers ? 1 : 0) +
                                    (GameManager.instance.hasRope ? 1 : 0) +
                                    (GameManager.instance.hasKey ? 1 : 0);
                }

                if (itemCollected >= 3)
                {
                    dayLabel = "BẠO LOẠN TẨU THOÁT";
                    finalDialogue = "Khu trại xảy ra bạo loạn lớn! Dù đêm trước đó bị lính gác phát hiện, nhưng nhờ giữ được đủ công cụ sinh tồn cốt lõi, bạn đã lợi dụng trận hỗn chiến để tự cắt rào, bẻ khóa và trốn thoát thành công khỏi địa ngục!";
                    finalSprite = sprTrueEscape;
                    finalBgm = bgmTrueEscape;
                }
                else
                {
                    dayLabel = "KẾT CỤC BI THẢM";
                    finalDialogue = "Khu trại xảy ra bạo loạn lớn... Vì chỉ gom được quá ít vật dụng phòng thân, bạn đã không thể sống sót qua cuộc hỗn chiến. BẠN ĐÃ BIẾN MẤT KHÔNG ĐỂ LẠI DẤU VẾT...";
                    finalSprite = sprDeath;
                    finalBgm = bgmDeath;
                }

                if (sfxAudioSource != null && riotSound != null) sfxAudioSource.PlayOneShot(riotSound);
                break;

            case EndingType.Death:
                int currentDay = GameManager.instance != null ? GameManager.instance.currentDay : 1;

                if (currentDay < 6)
                {
                    dayLabel = $"GỤC NGÃ TẠI NGÀY {currentDay}";
                    finalDialogue = "Những trận đòn roi, cú chích điện và áp lực chỉ tiêu đã rút cạn sinh lực của bạn. Bạn đã kiệt sức (Máu = 0) và gục ngã xuống sàn lạnh lẽo...";
                }
                else
                {
                    dayLabel = "KẾT CỤC BI THẢM";
                    finalDialogue = "Khu trại xảy ra bạo loạn lớn... Do thể lực của bạn lúc này quá yếu (Máu < 20), bạn đã không chịu đựng nổi những cú giẫm đạp và đã bỏ mạng trong biển người hỗn loạn. BẠN ĐÃ BIẾN MẤT KHÔNG ĐỂ LẠI DẤU VẾT...";
                }

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

        StartCoroutine(TypeDialogue(finalDialogue, dayLabel, ending));
    }

    // ========================================================
    // ĐÃ FIX: TRÌNH TỰ XUẤT HIỆN MƯỢT MÀ KHÔNG BỊ ĐÈ UI
    // ========================================================
    IEnumerator TypeDialogue(string dialogue, string dayLabel, EndingType ending)
    {
        // 1. Bật Tiêu đề lên ngay lập tức để người chơi biết chuyện gì xảy ra
        if (txtDayLabel != null)
        {
            txtDayLabel.text = dayLabel;
            txtDayLabel.gameObject.SetActive(true);
        }

        // 2. Chạy hiệu ứng máy chữ
        txtDialogue.text = "";
        txtDialogue.gameObject.SetActive(true);

        foreach (char letter in dialogue.ToCharArray())
        {
            txtDialogue.text += letter;
            if (sfxAudioSource != null && typingSound != null)
            {
                sfxAudioSource.PlayOneShot(typingSound, 0.2f);
            }
            yield return new WaitForSeconds(0.04f); // Tốc độ gõ
        }

        // 3. Chờ 3 giây để người chơi đọc xong dòng text
        yield return new WaitForSeconds(3f);

        // 4. TẮT đoạn text đi để nhường chỗ trống ở giữa màn hình cho các nút bấm
        if (txtDialogue != null)
        {
            txtDialogue.gameObject.SetActive(false);
        }

        // 5. Hiện nút Về Menu
        if (btnReturnMenu != null)
        {
            btnReturnMenu.gameObject.SetActive(true);
        }

        // 6. Hiện nút Chơi Lại (Nếu đủ điều kiện chết ngày 1-5)
        int currentDay = GameManager.instance != null ? GameManager.instance.currentDay : 1;
        if (btnRestartDay != null && ending == EndingType.Death && currentDay < 6)
        {
            btnRestartDay.gameObject.SetActive(true);
        }
    }

    void RestartCurrentDay()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.hp = GameManager.instance.maxHp;
            GameManager.instance.currentEnding = EndingType.None;
        }
        SceneManager.LoadScene(mainGameplaySceneName);
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}