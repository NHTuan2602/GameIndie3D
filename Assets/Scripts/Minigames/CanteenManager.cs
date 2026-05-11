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

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (btnFinishLunch != null) btnFinishLunch.gameObject.SetActive(true);

        // 4. KIỂM TRA NGÀY VÀ CHẠY CỐT TRUYỆN HỒN MA
        switch (day)
        {
            case 1:
                npcText.text = "Này ma mới, tao ở cái trại này lâu lắm rồi... chứng kiến nhiều kẻ bỏ mạng lắm. Ráng nhét tí cơm vào bụng đi kẻo gục.\nĐêm mai đợi bảo vệ đi vệ sinh, thử lẻn xuống <color=yellow>Tầng 1</color> xem sao. Tìm cuốn <color=yellow>Sổ tay</color> để ghi chép đường thoát.";
                break;
            case 2:
                npcText.text = "Mặt mày xanh xao quá, nhưng vẫn còn sống là tốt rồi.\nĐêm nay mày phải mò lên <color=yellow>Tầng 2</color>. Trên khu nhà kho có <color=yellow>Kềm cắt xích</color> và <color=yellow>Dây thừng</color>, thiếu chúng nó thì không trèo tường được đâu.";
                if (GameManager.instance != null) GameManager.instance.hasTalkedToNPC = true;
                break;
            case 3:
                npcText.text = "Gần đây bọn cai ngục đi tuần gắt gao lắm. Mày tìm được đồ nghề chưa?\nNếu có đồ rồi thì phải có <color=yellow>Chìa khóa</color> cổng. Lão quản lý hay vứt nó ở phòng làm việc trên <color=yellow>Tầng 3</color>. Lên đó cẩn thận, tao từng bị tóm ở cầu thang đấy...";
                break;
            case 4:
                // PLOT TWIST: Lời từ biệt của hồn ma
                npcText.text = "Mày gom đủ 4 món chưa? <color=yellow>Sổ, Kềm, Dây, Chìa khóa</color>... Nhớ lấy bài học của tao. Ngày xưa tao cũng tới được cái cổng đó, nhưng... bọn nó chặt đứt dây thừng của tao.\nĐêm nay tao đi trước đây. Đêm mai (Đêm 5) là hạn chót của mày, bọn nó bắt đầu thanh trừng rồi. Tự bảo trọng!";
                break;
            case 5:
                // NGÀY 5: Không có NPC, chuyển sang độc thoại nội tâm (in nghiêng)
                npcText.text = "<i>Chỗ ngồi đối diện trống không... Lão già hay ngồi kể chuyện vượt ngục mấy hôm trước đâu rồi?\nKhông còn thời gian nữa. Tối nay mình PHẢI TRỐN!</i>";
                break;
            default:
                npcText.text = "<i>Chỉ có tiếng nhai cơm lạnh lẽo...</i>";
                break;
        }
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