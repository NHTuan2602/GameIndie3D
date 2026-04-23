using UnityEngine;

public class NightManager : MonoBehaviour
{
    [Header("Cài đặt Đêm Hiện Tại")]
    public int currentNight = 2;

    [Header("Quản lý Đồ Vật (Sổ, Dây, Kìm, Chìa)")]
    // Index 0: Đêm 2 (Sổ), Index 1: Đêm 3 (Dây)...
    public GameObject[] nightItems;

    [Header("Quản lý Vật Cản (Chặn lối đi)")]
    // Kéo đống thùng chặn tầng 4 vào ô Element 0 (Đêm 2)
    // Kéo cửa khóa chặn tầng 5 vào ô Element 1 (Đêm 3)...
    public GameObject[] blockers;

    void Start()
    {
        SetupNight(currentNight);
    }

    public void SetupNight(int nightIndex)
    {
        // 1. TẮT HẾT TẤT CẢ ĐỒ VẬT
        foreach (GameObject item in nightItems)
        {
            if (item != null) item.SetActive(false);
        }

        // 2. BẬT ĐÚNG ĐỒ VẬT CỦA ĐÊM NAY
        if (nightIndex >= 2 && nightIndex <= 5)
        {
            int itemIndex = nightIndex - 2;
            if (nightItems[itemIndex] != null)
                nightItems[itemIndex].SetActive(true);
        }

        // 3. XỬ LÝ LỐI ĐI (MỞ KHÓA MAP DẦN DẦN)
        // Ví dụ: Đêm 2 (blockers[0] BẬT), Đêm 3 (blockers[0] TẮT, mở đường)
        for (int i = 0; i < blockers.Length; i++)
        {
            if (blockers[i] != null)
            {
                // Nếu đêm hiện tại lớn hơn đêm của vật cản này -> Tắt vật cản (Mở đường)
                // Ví dụ: currentNight = 3. i = 0 (vật cản đêm 2). 3 > 2 -> Tắt đống thùng.
                bool shouldBlock = currentNight <= (i + 2);
                blockers[i].SetActive(shouldBlock);
            }
        }

        Debug.Log("<color=cyan>ĐÃ KHỞI TẠO XONG MAP CHO ĐÊM: " + nightIndex + "</color>");
    }
}