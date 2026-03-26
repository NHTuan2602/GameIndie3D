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

        // 3. NPC Xuất hiện!
        if (audioSource != null && npcVoice != null) audioSource.PlayOneShot(npcVoice);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (btnFinishLunch != null) btnFinishLunch.gameObject.SetActive(true);

        // 4. KIỂM TRA NGÀY VÀ ĐƯA RA GỢI Ý (Đọc từ GameManager)
        if (GameManager.instance != null)
        {
            int day = GameManager.instance.currentDay;
            switch (day)
            {
                case 1:
                    npcText.text = "Khẽ thôi... Tao biết mày muốn trốn. Đêm mai (Đêm 2) đợi bảo vệ đi vệ sinh, thử lẻn ra hành lang xem sao.";
                    break;
                case 2:
                    npcText.text = "Giỏi lắm. Đêm nay mày phải tìm được cái cờ lê ở phòng kho. Không có nó, không cắt được xích cổng đâu.";
                    GameManager.instance.hasTalkedToNPC = true; // Mở khóa chức năng Thám Thính từ đây
                    break;
                case 3:
                    npcText.text = "Cửa sau có mật mã. Tối nay vào phòng lão quản lý tìm tờ bản đồ và mật mã đi. Bị bắt là chết đấy!";
                    break;
                case 4:
                    npcText.text = "Tao giấu một cái điện thoại nắp gập dưới đệm phòng mày. Tối nay lôi ra mà gọi ngầm cho cảnh sát biên giới đi!";
                    break;
                case 5:
                    npcText.text = "Đêm nay là đêm cuối... Tao nghe nói bọn nó định thanh trừng mày. Có đủ đồ nghề chưa? Cút khỏi đây ngay đêm nay đi!";
                    break;
                default:
                    npcText.text = "Cứ giữ mồm giữ miệng. Sống sót là trên hết.";
                    break;
            }
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