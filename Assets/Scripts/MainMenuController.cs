using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Cài đặt Chuyển Cảnh")]
    public string firstLevelName = "BusScene";
    public GameObject blackScreenFade; // Tấm màn đen để Fade Out

    [Header("Bảng tin nhắn (Điện thoại)")]
    public TextMeshProUGUI phoneMessageText; // Kéo Text đòi nợ vào đây
    public Button quitGameButton; // Kéo nút Quit của điện thoại vào đây

    [Header("Giấy nhận việc")]
    public TextMeshProUGUI paperTitleText; // Kéo Text "GIẤY BÁO VIỆN PHÍ" vào đây
    public TextMeshProUGUI paperInstructionText; // Kéo Text "[CLICK ĐỂ NHẬN VIỆC]" vào đây
    public Button startGameButton; // Kéo nút Start của tờ giấy vào đây

    [Header("Âm thanh (Audio Clips)")]
    public AudioClip paperCrumpleSound; // Tiếng lạo xạo giấy khi di chuột vào tờ giấy
    public AudioClip phoneVibrateSound; // Tiếng rung điện thoại khi di chuột vào điện thoại
    public AudioClip phoneRingSound; // Tiếng "Ting" khi tin nhắn đòi nợ đột ngột sáng lên

    private AudioSource audioSource;
    private bool isTransitioning = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Thả chuột để người chơi tương tác
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (blackScreenFade != null) blackScreenFade.SetActive(false);

        // Đảm bảo ban đầu mọi thứ tăm tối
        phoneMessageText.gameObject.SetActive(false); // Tin nhắn điện thoại đen ngòm

        StartCoroutine(StartMenuSequence()); // Bắt đầu kịch bản đèn chập chờn
    }

    IEnumerator StartMenuSequence()
    {
        yield return new WaitForSeconds(2f); // Đợi 2 giây trầm cảm

        // HIỆU ỨNG 1: Điện thoại đột ngột phát sáng và rung
        if (phoneRingSound != null) audioSource.PlayOneShot(phoneRingSound);
        if (phoneVibrateSound != null) audioSource.PlayOneShot(phoneVibrateSound); // Phối hợp code rung thực tế nếu có
        phoneMessageText.gameObject.SetActive(true); // Sáng màn hình hiện tin nhắn
        phoneMessageText.color = Color.green; // Tô màu xanh kiểu Matrix

        // TODO: Bạn có thể thêm code ở đây để cái điện thoại rung vật lý
    }

    // ==========================================
    // CÁC HÀM XỬ LÝ KHI DI CHUỘT VÀO (HOVER) - Nối dây trong Inspector
    // ==========================================
    public void OnPointerEnterPaper()
    {
        if (isTransitioning) return;
        paperInstructionText.text = "> CLICK ĐỂ NHẬN VIỆC <"; // Đổi nội dung mời gọi
        paperInstructionText.color = Color.yellow; // Đổi màu
        if (paperCrumpleSound != null) audioSource.PlayOneShot(paperCrumpleSound, 0.5f); // Tiếng lạo xạo giấy
    }

    public void OnPointerExitPaper()
    {
        if (isTransitioning) return;
        paperInstructionText.text = "[CLICK ĐỂ NHẬN VIỆC]"; // Reset nội dung gốc
        paperInstructionText.color = Color.white; // Reset màu
    }

    public void OnPointerEnterPhone()
    {
        if (isTransitioning) return;
        quitGameButton.image.color = new Color(0, 1, 0, 0.5f); // Sáng nhẹ màn hình xanh
        if (phoneVibrateSound != null) audioSource.PlayOneShot(phoneVibrateSound, 0.3f); // Rung nhẹ
    }

    public void OnPointerExitPhone()
    {
        if (isTransitioning) return;
        quitGameButton.image.color = new Color(0, 0, 0, 0f); // Tắt màn hình về đen kịt
    }

    // ==========================================
    // CÁC HÀM XỬ LÝ KHI CLICK (NỐI VÀO BUTTON ONCLICK)
    // ==========================================
    public void OnClickStartGame()
    {
        Debug.Log("Đã chốt nhận việc! Đang chuyển cảnh...");
        StartCoroutine(TransitionToGame());
    }

    public void OnClickQuitGame()
    {
        Debug.Log("Không chịu nổi áp lực. Thoát game!");
        Application.Quit();
    }

    IEnumerator TransitionToGame()
    {
        isTransitioning = true;
        // Bật màn hình đen che full màn hình
        if (blackScreenFade != null)
        {
            blackScreenFade.SetActive(true);
            // Bạn có thể dùng Animation hoặc code Lerp Alpha để màn hình đen từ từ ở đây
        }

        // Đợi 1.5 giây tạo cảm giác suspense
        yield return new WaitForSeconds(1.5f);

        // Load Scene Xe Bus (hoặc Scene Intro Siêu thị nếu bạn tách riêng)
        SceneManager.LoadScene(firstLevelName);
    }
}