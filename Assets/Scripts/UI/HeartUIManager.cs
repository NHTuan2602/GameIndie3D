using UnityEngine;
using UnityEngine.UI;

public class HeartUIManager : MonoBehaviour
{
    [Header("Kéo 3 icon trái tim vào đây (Từ trái sang phải)")]
    public GameObject[] heartIcons;

    void Update()
    {
        // Nếu không tìm thấy GameManager thì bỏ qua (tránh báo lỗi đỏ)
        if (GameManager.instance == null) return;

        // Tính số mạng còn lại. Giả sử tối đa 3 mạng.
        // Mỗi lần bị bắt, catchCount trong GameManager sẽ tăng lên.
        int livesLeft = 3 - GameManager.instance.catchCount;

        // Duyệt qua danh sách 3 trái tim để Bật / Tắt
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
            {
                // Nếu vị trí của trái tim nhỏ hơn số mạng còn lại -> Bật (True)
                // Ví dụ: livesLeft = 2 -> i=0 (Bật), i=1 (Bật), i=2 (Tắt)
                heartIcons[i].SetActive(i < livesLeft);
            }
        }
    }
}