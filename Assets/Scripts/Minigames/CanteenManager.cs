using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanteenManager : MonoBehaviour
{
    [Header("UI Căn Tin")]
    public GameObject dialoguePanel; // Bảng thoại của NPC
    public TextMeshProUGUI npcText;  // Chữ NPC nói
    public Button btnFinishLunch;    // Nút "Ăn xong, về làm việc"

    [Header("Âm thanh")]
    public AudioSource audioSource;
    public AudioClip eatingSound; // Tiếng nhai cơm/ồn ào (chạy trong 5s đầu)
    public AudioClip npcVoice;    // Tiếng "Hey" khi NPC xuất hiện

    // (Tùy chọn) Thêm một biến tiếng lạch cạch gõ chữ nếu có
    // public AudioClip typingSound; 

    private Coroutine typingCoroutine; // Biến lưu trữ tiến trình gõ chữ

    void Start()
    {
        // Ẩn bảng thoại lúc mới vào
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (btnFinishLunch != null) btnFinishLunch.gameObject.SetActive(false);

        // Nối nút bấm với hàm kết thúc giờ nghỉ
        if (btnFinishLunch != null) btnFinishLunch.onClick.AddListener(FinishLunchAndGoToWork);

        // Bắt đầu ăn cơm
        StartCoroutine(LunchRoutine());
    }

    IEnumerator LunchRoutine()
    {
        // 1. Phát tiếng ồn ào/nhai cơm
        if (audioSource != null && eatingSound != null)
        {
            audioSource.PlayOneShot(eatingSound);
        }

        // 2. Chờ đúng 5 giây
        yield return new WaitForSeconds(5f);

        int day = 1;
        if (GameManager.instance != null) day = GameManager.instance.currentDay;

        // 3. NPC CHỈ XUẤT HIỆN TỪ NGÀY 1 ĐẾN NGÀY 4
        if (day < 5)
        {
            if (audioSource != null && npcVoice != null) audioSource.PlayOneShot(npcVoice);
        }

        // Bật panel nền đen lên trước
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        // ĐÃ FIX: Khoan bật nút Finish Lunch vội, ép người chơi đọc hết chữ đã!
        if (btnFinishLunch != null) btnFinishLunch.gameObject.SetActive(false);

        string finalDialogue = "";

        // 4. KIỂM TRA NGÀY VÀ LẤY CỐT TRUYỆN HỒN MA
        switch (day)
        {
            case 1:
                finalDialogue = "Này cậu sinh viên năm nhất... mới bị thằng bạn lừa bán lên Tam Giác Vàng đúng không? Tôi thấy nhiều đứa trẻ như cậu gục vì roi điện ở cái xưởng này rồi. Ráng nuốt miếng cơm đi mà còn sức trả tiền viện phí cho mẹ cậu.\nNghe kỹ đây: khu này vận hành theo chu kỳ <color=yellow>5 ngày đêm thanh trừng</color>. Đêm mai đợi cai ngục đổi ca, lẻn xuống <color=yellow>Tầng 1</color> tìm cuốn <color=yellow>Sổ tay</color> vẽ sơ đồ lối thoát!";
                break;
            case 2:
                finalDialogue = "Mắt cậu thâm quầng hết rồi, nhưng qua được ngày đầu không bị chích chết là còn hy vọng.\nĐêm nay phải mò lên <color=yellow>Tầng 2</color>. Trong kho dụng cụ cũ có <color=yellow>Kềm cắt xích</color> và cuộn <color=yellow>Dây thừng</color>. Cẩn thận ánh đèn pin tuần tra — bị bắt 3 lần trong một đêm là chúng nó lôi ra giật điện tịch thu sạch đấy!";
                if (GameManager.instance != null) GameManager.instance.hasTalkedToNPC = true;
                break;
            case 3:
                finalDialogue = "Nghe tiếng còi ngoài hành lang không? Bọn quản lý đang siết chặt kiểm soát rồi. Cậu gom được Sổ tay, Kềm và Dây thừng chưa?\nMuốn mở được cổng sau để đào tẩu thì bắt buộc phải có <color=yellow>Chìa khóa</color>. Lão quản lý khóa nó trong tủ sắt ở phòng làm việc trên <color=yellow>Tầng 3</color>. Phải dùng Kềm mới phá được tủ!";
                break;
            case 4:
                // PLOT TWIST: Lời từ biệt của hồn ma
                finalDialogue = "Đủ cả 4 món chưa: <color=yellow>Sổ tay, Kềm, Dây thừng, Chìa khóa</color>... Nhìn cậu làm tôi nhớ bản thân mình nhiều năm trước. Tôi cũng từng trèo lên tới đỉnh bức tường rào khu tự trị này, nhưng... bọn lính gác phát hiện rồi chặt đứt dây thừng của tôi.\nĐêm nay tôi đi thật đây. <color=#EF476F>Đêm thứ 5</color> là hạn chót cuối cùng trước khi chúng nó thanh trừng toàn bộ trại. Hãy sống thay phần tôi và về gặp mẹ cậu đi!";
                break;
            case 5:
                // NGÀY 5: Không có NPC, chuyển sang độc thoại nội tâm (in nghiêng)
                finalDialogue = "<i>Ghế đối diện trống trơn... Ông lão hay ngồi kể chuyện vượt ngục suốt bốn ngày qua đã biến mất không một dấu vết.\nNgoài kia, tiếng roi điện nẹt liên hồi báo hiệu đêm thanh trừng sắp ập xuống. Không còn đường lui nữa — Đêm thứ 5 này mình PHẢI ĐÀO THOÁT khỏi Tam Giác Vàng!</i>";
                break;
            default:
                finalDialogue = "<i>Chỉ còn tiếng nhai cơm nguội lạnh lẽo giữa trại giam Tam Giác Vàng...</i>";
                break;
        }

        // Bắt đầu gõ chữ từ từ
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeDialogueRoutine(finalDialogue));
    }

    // ========================================================
    // ĐÃ FIX: MA THUẬT GÕ CHỮ CHO TEXTMESHPRO (KHÔNG LỖI THẺ MÀU)
    // ========================================================
    IEnumerator TypeDialogueRoutine(string dialogue)
    {
        npcText.text = dialogue; // Gán toàn bộ text (có cả thẻ màu)
        npcText.maxVisibleCharacters = 0; // Giấu hết chữ đi

        // Ép TextMeshPro tính toán lại số lượng chữ cái thực tế (không tính các thẻ <color>, <i>)
        npcText.ForceMeshUpdate();
        int totalCharacters = npcText.textInfo.characterCount;

        // Vòng lặp tăng dần số lượng chữ hiện ra màn hình
        for (int i = 0; i <= totalCharacters; i++)
        {
            npcText.maxVisibleCharacters = i;

            // Bật âm thanh lạch cạch ở đây nếu có (Tùy chọn)
            // if (audioSource != null && typingSound != null) audioSource.PlayOneShot(typingSound, 0.2f);

            yield return new WaitForSeconds(0.04f); // Tốc độ gõ chữ (thay đổi số này để nhanh/chậm)
        }

        // Chạy xong chữ rồi thì mới hiện nút cho đi làm
        yield return new WaitForSeconds(0.5f);
        if (btnFinishLunch != null) btnFinishLunch.gameObject.SetActive(true);
    }

    public void FinishLunchAndGoToWork()
    {
        // Phục hồi một chút máu khi ăn cơm
        if (GameManager.instance != null)
        {
            GameManager.instance.hp += 20;
            if (GameManager.instance.hp > GameManager.instance.maxHp)
                GameManager.instance.hp = GameManager.instance.maxHp;

            // Chuyển lại ca Chiều
            GameManager.instance.TransitionToPhase(GamePhase.Afternoon);
        }
    }
}