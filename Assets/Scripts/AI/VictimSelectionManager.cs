using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VictimSelectionManager : MonoBehaviour
{
    [Header("Danh sách 25 Nạn Nhân (Kéo thả vào đây)")]
    public List<VictimProfile> allVictims;

    [Header("UI Của 4 Thẻ Chọn")]
    public GameObject selectionPanel;
    public Button[] btnSelectVictims;
    public TextMeshProUGUI[] nameTexts;
    public TextMeshProUGUI[] jobTexts;
    public TextMeshProUGUI[] rewardTexts;
    public Image[] avatarImages;

    [Header("Tham chiếu")]
    public ScamMinigame minigameController;

    private List<VictimProfile> normalPool = new List<VictimProfile>();
    private List<VictimProfile> trollPool = new List<VictimProfile>();
    private VictimProfile[] currentChoices = new VictimProfile[4];

    void Start()
    {
        foreach (var victim in allVictims)
        {
            if (victim.isTroll) trollPool.Add(victim);
            else normalPool.Add(victim);
        }

        ShowSelectionUI(1);
    }

    public void ShowSelectionUI(int currentDay)
    {
        selectionPanel.SetActive(true);
        List<VictimProfile> tempChoices = new List<VictimProfile>();

        if (currentDay == 1 || trollPool.Count == 0)
        {
            tempChoices = GetRandomVictims(normalPool, 4);
        }
        else
        {
            tempChoices.AddRange(GetRandomVictims(trollPool, 1));
            tempChoices.AddRange(GetRandomVictims(normalPool, 3));
        }

        ShuffleList(tempChoices);

        for (int i = 0; i < 4; i++)
        {
            if (i < tempChoices.Count)
            {
                currentChoices[i] = tempChoices[i];
                nameTexts[i].text = tempChoices[i].victimName;
                jobTexts[i].text = tempChoices[i].jobOrAge;

                string diffVN = GetDifficultyString(tempChoices[i].difficultyLevel);
                rewardTexts[i].text = $"Độ khó: <color=#FFFF00>{diffVN}</color>";

                if (avatarImages != null && avatarImages.Length > i && tempChoices[i].avatar != null)
                {
                    avatarImages[i].sprite = tempChoices[i].avatar;
                }

                int index = i;
                if (btnSelectVictims != null && btnSelectVictims.Length > i)
                {
                    btnSelectVictims[i].onClick.RemoveAllListeners();
                    btnSelectVictims[i].onClick.AddListener(() => OnVictimChosen(index));
                }
            }
        }
    }

    private string GetDifficultyString(VictimProfile.Difficulty diff)
    {
        switch (diff)
        {
            case VictimProfile.Difficulty.De: return "Dễ";
            case VictimProfile.Difficulty.TrungBinh: return "Trung Bình";
            case VictimProfile.Difficulty.Kho: return "Khó";
            default: return "Chưa rõ";
        }
    }

    private List<VictimProfile> GetRandomVictims(List<VictimProfile> pool, int amount)
    {
        List<VictimProfile> result = new List<VictimProfile>();
        List<VictimProfile> tempPool = new List<VictimProfile>(pool);

        for (int i = 0; i < amount; i++)
        {
            if (tempPool.Count == 0) break;
            int rand = Random.Range(0, tempPool.Count);
            result.Add(tempPool[rand]);
            tempPool.RemoveAt(rand);
        }
        return result;
    }

    private void ShuffleList(List<VictimProfile> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            VictimProfile temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void OnVictimChosen(int index)
    {
        VictimProfile chosenVictim = currentChoices[index];

        if (chosenVictim.isTroll) trollPool.Remove(chosenVictim);
        else normalPool.Remove(chosenVictim);

        selectionPanel.SetActive(false);

        minigameController.maxMoneyReward = chosenVictim.potentialReward;
        minigameController.karmaPenalty = chosenVictim.karmaPenalty;

        // ==========================================
        // ĐÃ FIX: TRUYỀN THÊM isTroll VÀO HỆ THỐNG
        // ==========================================
        minigameController.StartMiniGame(chosenVictim.rounds, chosenVictim.victimName, chosenVictim.avatar, chosenVictim.isTroll);
    }
}