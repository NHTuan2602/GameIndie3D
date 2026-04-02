using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VictimSelectionManager : MonoBehaviour
{
    [Header("Danh sách 25 Nạn Nhân (Kéo thả vào đây)")]
    public List<VictimProfile> allVictims;

    [Header("UI Của 4 Thẻ Chọn")]
    public GameObject selectionPanel; // Bảng chứa lựa chọn
    public Button[] btnSelectVictims; // 4 Nút bấm chọn
    public TextMeshProUGUI[] nameTexts; // 4 Ô tên
    public TextMeshProUGUI[] jobTexts; // 4 Ô công việc
    public TextMeshProUGUI[] rewardTexts; // 4 Ô tiền thưởng
    public Image[] avatarImages;

    [Header("Tham chiếu")]
    public ScamMinigame minigameController;

    // Danh sách nội bộ để lọc không bị trùng
    private List<VictimProfile> normalPool = new List<VictimProfile>();
    private List<VictimProfile> trollPool = new List<VictimProfile>();

    // ĐÃ FIX: Chứa 4 người thay vì 3
    private VictimProfile[] currentChoices = new VictimProfile[4];

    void Start()
    {
        // Phân loại nạn nhân vào 2 thùng chứa
        foreach (var victim in allVictims)
        {
            if (victim.isTroll) trollPool.Add(victim);
            else normalPool.Add(victim);
        }

        // THÊM ĐÚNG DÒNG NÀY VÀO ĐỂ ÉP NÓ IN CHỮ RA NGAY KHI BẤM PLAY:
        ShowSelectionUI(1);
    }

    // GỌI HÀM NÀY MỖI KHI BẮT ĐẦU CA SÁNG/CHIỀU
    public void ShowSelectionUI(int currentDay)
    {
        selectionPanel.SetActive(true);
        List<VictimProfile> tempChoices = new List<VictimProfile>();

        // ĐÃ FIX: Luật Ngày 1: Toàn người bình thường (Random 4 người)
        if (currentDay == 1 || trollPool.Count == 0)
        {
            tempChoices = GetRandomVictims(normalPool, 4);
        }
        // ĐÃ FIX: Luật Ngày 2, 3, 4, 5: Bắt buộc có 1 Troll (Nếu còn) và 3 Bình thường (Tổng 4)
        else
        {
            tempChoices.AddRange(GetRandomVictims(trollPool, 1));
            tempChoices.AddRange(GetRandomVictims(normalPool, 3));
        }

        // Đảo lộn vị trí để người chơi không biết Troll nằm ở ô nào
        ShuffleList(tempChoices);

        // ĐÃ FIX: Hiển thị lên UI (Vòng lặp 4 lần)
        for (int i = 0; i < 4; i++)
        {
            if (i < tempChoices.Count)
            {
                currentChoices[i] = tempChoices[i];
                nameTexts[i].text = tempChoices[i].victimName;
                jobTexts[i].text = tempChoices[i].jobOrAge;

                // GIẤU SỐ TIỀN THƯỞNG
                rewardTexts[i].text = "Tài sản: ??? VNĐ";

                // Gán Avatar nếu có
                if (avatarImages != null && avatarImages.Length > i && tempChoices[i].avatar != null)
                {
                    avatarImages[i].sprite = tempChoices[i].avatar;
                }

                int index = i; // Bắt buộc phải gán biến tạm cho listener
                if (btnSelectVictims != null && btnSelectVictims.Length > i)
                {
                    btnSelectVictims[i].onClick.RemoveAllListeners();
                    btnSelectVictims[i].onClick.AddListener(() => OnVictimChosen(index));
                }
            }
        }
    }

    // Hàm bốc ngẫu nhiên N người từ một danh sách
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

    // Thuật toán tráo bài (Fisher-Yates)
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

    // Khi người chơi bấm chọn 1 nạn nhân
    private void OnVictimChosen(int index)
    {
        VictimProfile chosenVictim = currentChoices[index];

        // Xóa nạn nhân đã chọn khỏi Pool để sau này không xuất hiện lại nữa
        if (chosenVictim.isTroll) trollPool.Remove(chosenVictim);
        else normalPool.Remove(chosenVictim);

        selectionPanel.SetActive(false);

        // Truyền dữ liệu sang Minigame để bắt đầu gõ phím
        minigameController.maxMoneyReward = chosenVictim.potentialReward;
        minigameController.karmaPenalty = chosenVictim.karmaPenalty;
        minigameController.StartMiniGame(chosenVictim.rounds, chosenVictim.victimName, chosenVictim.avatar);
    }
}