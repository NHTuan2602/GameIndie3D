using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void BamNutChoiMoi()
    {
        // 1. ĐỐT SẠCH SỔ NỢ: Xóa toàn bộ dữ liệu (Tiền, Máu, Số ván bạc, Vị trí...)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. TIÊU DIỆT ÔNG TRÙM CŨ: Xóa GameManager của ván trước (nếu nó còn sót lại)
        if (GameManager.instance != null)
        {
            Destroy(GameManager.instance.gameObject);
        }

        // 3. BẮT ĐẦU GAME MỚI (Sửa lại tên Scene mở đầu game của bạn cho đúng)
        SceneManager.LoadScene("SampleScene");
    }

    public void BamNutThoat()
    {
        Application.Quit();
    }
}