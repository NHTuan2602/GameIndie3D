using UnityEngine;

public class PlayerHide : MonoBehaviour
{
    [Header("Trạng thái Núp")]
    public bool isHidden = false;

    void Update()
    {
        // Nhấn E để Núp / Bỏ núp (Sau này bạn có thể ghép vào Cánh cửa tủ)
        if (Input.GetKeyDown(KeyCode.R))
        {
            isHidden = !isHidden;

            if (isHidden)
            {
                Debug.Log("<color=cyan>PLAYER: Đã nín thở núp vào góc!</color>");
                // Bạn có thể thêm code tắt MeshRenderer hoặc khóa di chuyển ở đây
            }
            else
            {
                Debug.Log("<color=cyan>PLAYER: Đã chui ra ngoài!</color>");
            }
        }
    }
}