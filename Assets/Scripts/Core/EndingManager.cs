using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

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
    public AudioClip restartSound;

    void Start()
    {
        // Rã đông thời gian
        Time.timeScale = 1f;

        // Hiện chuột
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (btnReturnMenu != null) btnReturnMenu.gameObject.SetActive(false);
        if (btnRestartDay != null) btnRestartDay.gameObject.SetActive(false);

        if (txtDayLabel != null) txtDayLabel.gameObject.SetActive(false);
        if (txtDialogue != null) txtDialogue.gameObject.SetActive(false);

        EndingType endingToPlay = GameManager.instance != null ? GameManager.instance.currentEnding : EndingType.None;

        if (enableTestMode)
        {
            endingToPlay = testEndingToPlay;
        }

        SetupEnding(endingToPlay);

        // Gán sự kiện click (Xoá cái cũ trước để chống click đúp)
        if (btnReturnMenu != null)
        {
            btnReturnMenu.onClick.RemoveAllListeners();
            btnReturnMenu.onClick.AddListener(ReturnToMainMenu);
        }

        if (btnRestartDay != null)
        {
            btnRestartDay.onClick.RemoveAllListeners();
            btnRestartDay.onClick.AddListener(RestartCurrentDay);
        }
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
                dayLabel = "SA LƯỚI PHÁP LUẬT";
                finalDialogue = "Qua 5 ngày đêm ở Khu tự trị Tam Giác Vàng, để sinh tồn và gom tiền viện phí cho mẹ, bạn đã trót lừa quá nhiều nạn nhân nhẹ dạ và dần bán rẻ lương tâm cho tổ chức. Ngày trốn thoát khỏi trại cũng là lúc lực lượng cảnh sát ập vào bắt giữ bạn vì tội lừa đảo xuyên quốc gia. Thoát được roi điện, nhưng còng số 8 đã khép lại tuổi 18 của cậu sinh viên năm nhất.";
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
                    finalDialogue = "Đêm thanh trừng thứ 5 bùng phát thành cuộc bạo loạn đẫm máu khắp Khu tự trị Tam Giác Vàng! Nhờ bí mật gom được phần lớn đồ nghề vượt ngục trong các đêm qua, bạn đã liều mình cắt hàng rào thép gai giữa làn khói lửa, thoát thân về biên giới trong gang tấc để kịp trở về bên giường bệnh của mẹ.";
                    finalSprite = sprTrueEscape;
                    finalBgm = bgmTrueEscape;
                }
                else
                {
                    dayLabel = "GỤC NGÃ TRONG ĐÊM THANH TRỪNG";
                    finalDialogue = "Cuộc bạo loạn rạng sáng Ngày thứ 6 nhấn chìm Khu tự trị Tam Giác Vàng trong tiếng súng và roi điện... Do chuẩn bị quá thiếu thốn và bị lộ hành tung từ trước, cậu sinh viên năm nhất tay không tấc sắt đã gục ngã ngay sát hàng rào thép gai. Giấc mơ trả viện phí cứu mẹ mãi mãi khép lại nơi đất khách.";
                    finalSprite = sprDeath;
                    finalBgm = bgmDeath;
                }

                if (sfxAudioSource != null && riotSound != null) sfxAudioSource.PlayOneShot(riotSound);
                break;

            case EndingType.Death:
                int currentDay = GameManager.instance != null ? GameManager.instance.currentDay : 1;

                if (currentDay < 6)
                {
                    dayLabel = $"CHẾT TẠI TAM GIÁC VÀNG — NGÀY {currentDay}";
                    finalDialogue = "Những cú chích điện cao áp từ tên Quản lý khi không đạt KPI và các đòn tra tấn ban đêm đã rút cạn hơi thở cuối cùng của bạn. Cậu sinh viên năm nhất gục xuống sàn bê tông lạnh lẽo của Tam Giác Vàng, không bao giờ còn có thể gửi tiền viện phí về cho mẹ nữa...";
                }
                else
                {
                    dayLabel = "TAN XÁC NGÀY THANH TRỪNG";
                    finalDialogue = "Rạng sáng Ngày thứ 6, cuộc thanh trừng và bạo loạn nổ ra. Với cơ thể kiệt quệ sau 5 ngày đêm bị chích điện tra tấn (Máu < 20), bạn không thể gượng dậy nổi giữa biển người hỗn loạn. Tên bạn bị xóa vĩnh viễn khỏi Khu tự trị Tam Giác Vàng...";
                }

                finalSprite = sprDeath;
                finalBgm = bgmDeath;
                break;

            case EndingType.TrueEscape:
                dayLabel = "ĐÀO THOÁT HOÀN HẢO — TRUE ENDING";
                finalDialogue = "Sau 5 ngày đêm cắn răng lừa gạt qua KPI để giữ mạng và lặng lẽ thu thập trọn vẹn Sổ tay, Kềm, Dây thừng, Chìa khóa theo chỉ dẫn của ông lão tiền nhiệm, bạn đã cắt xích vượt rào thoát khỏi Khu tự trị Tam Giác Vàng và phóng xuyên truy sát tới đồn biên phòng! Mọi tội ác của đường dây bị vạch trần — bạn sống sót trở về bệnh viện nắm tay mẹ và vĩnh viễn thoát khỏi bóng ma bạo hành.";
                finalSprite = sprTrueEscape;
                finalBgm = bgmTrueEscape;
                break;

            case EndingType.BadCredit:
                dayLabel = "NÔ LỆ TÍN DỤNG ĐEN";
                finalDialogue = "Vì quá nóng vội kiếm tiền trả viện phí cho mẹ ngay Đêm thứ 1, bạn sa vào sới Tài Xỉu rồi gánh món nợ lãi mẹ đẻ lãi con không đáy. Khu tự trị Tam Giác Vàng tước sạch mọi quyền chuộc thân — biến bạn thành cỗ máy gõ lừa đảo khổ sai vĩnh viễn cho tới khi kiệt sức.";
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

    IEnumerator TypeDialogue(string dialogue, string dayLabel, EndingType ending)
    {
        if (txtDayLabel != null)
        {
            txtDayLabel.text = dayLabel;
            txtDayLabel.gameObject.SetActive(true);
        }

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

        if (txtDialogue != null)
        {
            txtDialogue.gameObject.SetActive(false);
        }

        if (btnReturnMenu != null)
        {
            btnReturnMenu.gameObject.SetActive(true);
            btnReturnMenu.interactable = true;
        }

        int currentDay = GameManager.instance != null ? GameManager.instance.currentDay : 1;
        if (btnRestartDay != null && ending == EndingType.Death && currentDay < 6)
        {
            btnRestartDay.gameObject.SetActive(true);
            btnRestartDay.interactable = true;
        }
    }

    void RestartCurrentDay()
    {
        StartCoroutine(PlaySoundAndRestartRoutine());
    }

    IEnumerator PlaySoundAndRestartRoutine()
    {
        if (btnRestartDay != null) btnRestartDay.interactable = false;
        if (btnReturnMenu != null) btnReturnMenu.interactable = false;

        if (sfxAudioSource != null && restartSound != null)
        {
            sfxAudioSource.PlayOneShot(restartSound, 1f);
            yield return new WaitForSeconds(1.5f);
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.ResetDayForRetry();
        }

        try
        {
            SceneManager.LoadScene(mainGameplaySceneName);
        }
        catch (Exception e)
        {
            Debug.LogError($"[LỖI CHƯA ADD SCENE]: Bạn chưa kéo file Scene '{mainGameplaySceneName}' vào File -> Build Settings! Lỗi gốc: {e.Message}");
        }
    }

    void ReturnToMainMenu()
    {
        Debug.Log("---> Đã bấm nút! Đang tải Scene: MainMenu");
        try
        {
            SceneManager.LoadScene("MainMenu");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LỖI CHƯA ADD SCENE]: Bạn chưa kéo file Scene 'MainMenu' vào File -> Build Settings! Hãy ấn Ctrl + Shift + B để kéo vào. Lỗi gốc: {e.Message}");
        }
    }
}