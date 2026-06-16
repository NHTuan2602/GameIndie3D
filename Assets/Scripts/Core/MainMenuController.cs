using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // ĐÃ THÊM: Để đọc được khung gõ chữ

public class MainMenuController : MonoBehaviour
{
    // ĐÃ THÊM: Kéo cái khung gõ tên (TMP_InputField) ngoài màn hình vào đây
    public TMP_InputField nameInputField;

    public void BamNutChoiMoi()
    {
        // 1. ĐỐT SẠCH SỔ NỢ CŨ
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. LƯU TÊN NGƯỜI CHƠI VÀO BỘ NHỚ
        string tenDaNhap = "Người chơi ";
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            tenDaNhap = nameInputField.text;
        }
        // Lưu thẳng vào Registry của máy tính để gọi mọi lúc mọi nơi
        PlayerPrefs.SetString("PlayerName", tenDaNhap);
        PlayerPrefs.Save();

        // 3. TIÊU DIỆT ÔNG TRÙM CŨ
        if (GameManager.instance != null) Destroy(GameManager.instance.gameObject);

        // 4. BẮT ĐẦU GAME MỚI
        SceneManager.LoadScene("SampleScene");
    }
}