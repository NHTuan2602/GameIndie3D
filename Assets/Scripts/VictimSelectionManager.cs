using System.Collections.Generic; // MỚI THÊM: Bắt buộc có dòng này để dùng được List (Hộp ảo)
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VictimSelectionManager : MonoBehaviour
{
    [Header("Giao diện UI")]
    public GameObject selectionPanel;
    public ScamMinigame scamMinigame;

    [Header("Kho Dữ liệu (Kéo các file Kịch Bản vào đây)")]
    public VictimProfile[] allAvailableVictims;

    [Header("Danh sách 4 Hàng UI (Kéo 4 cái VictimRow vào đây)")]
    public VictimRowUI[] victimRows = new VictimRowUI[4];

    private VictimProfile[] currentDayVictims = new VictimProfile[4];

    void Start()
    {
        GenerateVictimList();
    }

    public void GenerateVictimList()
    {
        if (allAvailableVictims.Length == 0) return;

        // ==========================================
        // MỚI SỬA: TẠO HỘP BỐC THĂM ẢO ĐỂ KHÔNG BỊ TRÙNG
        // ==========================================
        List<VictimProfile> tempPool = new List<VictimProfile>(allAvailableVictims);

        for (int i = 0; i < 4; i++)
        {
            // Rào chắn an toàn: Nếu hộp lỡ hết thăm thì đổ đầy lại
            if (tempPool.Count == 0)
            {
                tempPool = new List<VictimProfile>(allAvailableVictims);
            }

            // Bốc 1 lá thăm từ hộp ảo
            int randomIndex = Random.Range(0, tempPool.Count);
            currentDayVictims[i] = tempPool[randomIndex];

            // XÉ LÁ THĂM ĐÓ ĐI ĐỂ NGƯỜI SAU KHÔNG BỐC TRÚNG NỮA!
            tempPool.RemoveAt(randomIndex);

            VictimProfile victim = currentDayVictims[i];

            // ==========================================
            // BỘ PHIÊN DỊCH ENUM SANG TIẾNG VIỆT
            // ==========================================
            string diffString = "";
            switch (victim.difficultyLevel)
            {
                case VictimProfile.Difficulty.De:
                    diffString = "Dễ";
                    break;
                case VictimProfile.Difficulty.TrungBinh:
                    diffString = "Trung Bình";
                    break;
                case VictimProfile.Difficulty.Kho:
                    diffString = "Khó";
                    break;
            }

            // GỌI HÀM VÀ TRUYỀN BIẾN diffString VÀO ĐÂY
            victimRows[i].SetupRow(
                victim.victimName,
                victim.jobOrAge + " | Thưởng: $" + victim.potentialReward,
                diffString,
                victim.staminaCost
            );

            // Gắn lệnh cho nút "OK" của từng hàng
            int index = i;
            victimRows[i].actionButton.onClick.RemoveAllListeners();
            victimRows[i].actionButton.onClick.AddListener(() => OnVictimSelected(index));
        }

        selectionPanel.SetActive(true);
    }

    public void OnVictimSelected(int index)
    {
        VictimProfile selectedVictim = currentDayVictims[index];

        // Gõ cửa GameManager xin phép trừ thể lực
        if (GameManager.instance != null)
        {
            bool canScam = GameManager.instance.StartScammingVictim(selectedVictim.staminaCost);
            // Nếu kiệt sức chết, cấm không cho mở màn hình gõ chữ
            if (!canScam) return;
        }

        // Cập nhật số tiền thưởng cho Mini-game
        scamMinigame.maxMoneyReward = selectedVictim.potentialReward;

        // Tắt bảng danh sách, Mở màn hình gõ chữ
        selectionPanel.SetActive(false);
        scamMinigame.StartMiniGame(selectedVictim.rounds);
    }
}