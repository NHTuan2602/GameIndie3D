using System.Collections.Generic;
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

        List<VictimProfile> tempPool = new List<VictimProfile>(allAvailableVictims);

        for (int i = 0; i < 4; i++)
        {
            if (tempPool.Count == 0)
            {
                tempPool = new List<VictimProfile>(allAvailableVictims);
            }

            int randomIndex = Random.Range(0, tempPool.Count);
            currentDayVictims[i] = tempPool[randomIndex];
            tempPool.RemoveAt(randomIndex);

            VictimProfile victim = currentDayVictims[i];

            string diffString = "";
            switch (victim.difficultyLevel)
            {
                case VictimProfile.Difficulty.De: diffString = "Dễ"; break;
                case VictimProfile.Difficulty.TrungBinh: diffString = "Trung Bình"; break;
                case VictimProfile.Difficulty.Kho: diffString = "Khó"; break;
            }

            // ĐÃ SỬA: FORMAT HIỂN THỊ TIỀN VNĐ CÓ DẤU PHẨY (Ví dụ: 10,000,000 VNĐ)
            string rewardText = victim.jobOrAge + " | Thưởng: " + victim.potentialReward.ToString("N0") + " VNĐ";

            victimRows[i].SetupRow(
                victim.victimName,
                rewardText,
                diffString,
                victim.staminaCost
            );

            int index = i;
            victimRows[i].actionButton.onClick.RemoveAllListeners();
            victimRows[i].actionButton.onClick.AddListener(() => OnVictimSelected(index));
        }

        selectionPanel.SetActive(true);
    }

    public void OnVictimSelected(int index)
    {
        VictimProfile selectedVictim = currentDayVictims[index];

        if (GameManager.instance != null)
        {
            // Trừ Thể lực khi bắt đầu gọi
            bool canScam = GameManager.instance.StartScammingVictim(selectedVictim.staminaCost);
            if (!canScam) return;
        }

        // ĐÃ NÂNG CẤP: Truyền số tiền thật (VNĐ) và Chỉ số Nghiệp chướng qua Minigame
        scamMinigame.maxMoneyReward = selectedVictim.potentialReward;
        scamMinigame.karmaPenalty = selectedVictim.karmaPenalty;

        // Cân bằng lại tiền thưởng nóng của Sếp nếu lừa 5/5 (Ví dụ bằng 10% tiền lừa được)
        scamMinigame.bossBonus = selectedVictim.potentialReward * 0.1f;

        selectionPanel.SetActive(false);

        // ĐÃ NÂNG CẤP: Gửi kèm thêm Cả Avatar của nạn nhân qua nữa!
        scamMinigame.StartMiniGame(selectedVictim.rounds, selectedVictim.victimName, selectedVictim.avatar);
    }
}