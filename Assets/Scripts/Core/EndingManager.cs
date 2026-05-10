using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    // ==========================================
    // CẦU DAO TEST: ĐỂ TEST CÁC LOẠI ENDING
    // ==========================================
    [Header("--- DEV TEST (TEST KHÔNG CẦN CHƠI GAME) ---")]
    [Tooltip("Tích vào đây để chạy thẳng Ending mà không cần chơi từ đầu")]
    public bool enableTestMode = false;
    [Tooltip("Chọn Ending muốn xem (Chỉ có tác dụng khi tích vào ô trên)")]
    public EndingType testEndingToPlay = EndingType.TrueEscape;

    [Header("--- UI CHỮ ---")]
    [Tooltip("Dùng để hiện chữ NGÀY 6 lúc đầu, và TÊN ENDING lúc sau")]
    public TextMeshProUGUI txtDayLabel;
    [Tooltip("Dùng để chạy chữ kể chuyện lạch cạch")]
    public TextMeshProUGUI txtDialogue;
    public GameObject btnReturnMenu;

    [Header("--- UI HÌNH ẢNH ENDING ---")]
    public Image imgEndingCard;

    // Kéo 4 tấm hình vẽ Ending của bạn vào 4 ô này (Đã bỏ Bad Credit)
    public Sprite sprArrested;
    public Sprite sprRiotSurvivor;
    public Sprite sprDeath;
    public Sprite sprTrueEscape;

    [Header("Cài đặt Âm thanh")]
    public AudioSource audioSource;
    public AudioClip typingSound;
    public AudioClip riotSound;

    void Start()
    {
        // Tắt sạch sẽ mọi thứ lúc mới vào màn hình đen
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

        // ==========================================
        // 1. XÁC ĐỊNH ENDING SẼ CHẠY (Ưu tiên Test Mode)
        // ==========================================
        EndingType activeEnding = EndingType.None;

        if (enableTestMode)
        {
            activeEnding = testEndingToPlay;
            Debug.Log("<color=magenta>DEV CHEAT: ĐANG CHẠY TEST MODE CHO ENDING: " + activeEnding + "</color>");
        }
        else if (GameManager.instance != null)
        {
            activeEnding = GameManager.instance.currentEnding;
            Debug.Log("<color=green>KẾT NỐI GAMEMANAGER THÀNH CÔNG! ĐANG CHẠY KỊCH BẢN: " + activeEnding + "</color>");
        }
        else
        {
            Debug.Log("<color=red>LỖI MẤT KẾT NỐI: KHÔNG TÌM THẤY GAMEMANAGER!</color>");
        }

        // ==========================================
        // 2. XÁC ĐỊNH NGÀY ĐỂ HIỆN LABEL
        // ==========================================
        bool isDay6 = (activeEnding == EndingType.Arrested || activeEnding == EndingType.RiotSurvivor || activeEnding == EndingType.Death);

        if (isDay6)
        {
            txtDayLabel.text = "NGÀY THỨ 6...\n<size=50%>SỰ KIỆN BẠO LOẠN</size>";
            if (audioSource != null && riotSound != null) audioSource.PlayOneShot(riotSound);
        }
        else
        {
            txtDayLabel.text = "ĐÊM THỨ 5...";
        }
        txtDayLabel.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);
        txtDayLabel.gameObject.SetActive(false);

        // ==========================================
        // 3. CHỌN KỊCH BẢN KỂ CHUYỆN VÀ TÊN ENDING
        // ==========================================
        switch (activeEnding)
        {
            case EndingType.Arrested:
                // ĐÃ SỬA: Kịch bản bị tóm ở sân bay khi về quê ăn Tết
                storyText = "Khu trại xảy ra bạo loạn lớn...\nNhờ thành tích lừa đảo xuất sắc, tôi được tổ chức ưu ái bảo vệ an toàn.\nVài năm sau, mang theo số tiền bẩn khổng lồ, tôi đáp chuyến bay về nước ăn Tết...\nNhưng Cục An Ninh Mạng đã đợi sẵn ngay tại sảnh chờ. Lưới trời tuy thưa nhưng khó lọt.";
                endingName = "ENDING A: LƯỚI TRỜI";
                finalImage = sprArrested;
                break;

            case EndingType.RiotSurvivor:
                storyText = "Khu trại xảy ra bạo loạn lớn!\nNhờ thể lực còn tốt và vài công cụ giấu sẵn trong người...\nTôi đã đánh gục lính gác và hòa vào dòng người lẩn trốn vào rừng sâu.\nTôi đã sống sót... nhưng bóng ma tâm lý sẽ còn ám ảnh mãi mãi.";
                endingName = "NEUTRAL ENDING: KẺ SỐNG SÓT";
                finalImage = sprRiotSurvivor;
                break;

            case EndingType.Death:
                storyText = "Khu trại xảy ra bạo loạn lớn...\nNhưng tôi đã quá kiệt sức sau những ngày làm việc cường độ cao.\nKhông có công cụ trong tay, tôi gục ngã giữa những tiếng la hét...\nMọi thứ mờ dần... Trò chơi kết thúc.";
                endingName = "BAD ENDING: TỬ VONG";
                finalImage = sprDeath;
                break;

            case EndingType.TrueEscape:
                storyText = "Tôi đã thu thập đủ 4 món đồ nghề.\nCắt rào, đu dây và phóng xe tẩu thoát thành công trong màn đêm!\nTôi đã tự cứu lấy chính mình trước khi sự việc tồi tệ hơn.";
                endingName = "TRUE ENDING: TỰ DO";
                finalImage = sprTrueEscape;
                break;
        }

        // ==========================================
        // 4. CHẠY CHỮ KỂ CHUYỆN (TYPEWRITER)
        // ==========================================
        txtDialogue.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        foreach (char c in storyText)
        {
            txtDialogue.text += c;
            if (c != ' ' && c != '\n' && audioSource != null && typingSound != null)
            {
                audioSource.PlayOneShot(typingSound, 0.1f);
            }
            yield return new WaitForSeconds(0.04f); // Tốc độ gõ chữ
        }

        // Chờ người chơi đọc xong rồi tắt chữ
        yield return new WaitForSeconds(3.5f);
        txtDialogue.gameObject.SetActive(false);

        // ==========================================
        // 5. HIỆN TÊN ENDING VÀ BỨC ẢNH
        // ==========================================
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

        // ==========================================
        // 6. HIỆN NÚT CHƠI LẠI
        // ==========================================
        yield return new WaitForSeconds(1.5f);
        if (btnReturnMenu != null) btnReturnMenu.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReturnToMainMenu()
    {
        // Dọn dẹp GameManager cũ trước khi quay về Menu để tránh lỗi trùng lặp
        if (GameManager.instance != null) Destroy(GameManager.instance.gameObject);
        SceneManager.LoadScene("MainMenu"); // Thay bằng tên Scene Menu thực tế của bạn
    }
}