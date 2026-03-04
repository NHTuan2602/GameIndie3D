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

    [Header("Gắn 4 hàng UI vào đây")]
    public Button[] victimButtons = new Button[4];       // Kéo 4 nút OK vào
    public TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[4];   // Kéo 4 chữ Tên vào
    public TextMeshProUGUI[] infoTexts = new TextMeshProUGUI[4];   // Kéo 4 chữ Nghề vào
    public TextMeshProUGUI[] staminaTexts = new TextMeshProUGUI[4];// Kéo 4 chữ Thể lực vào

    private VictimProfile[] currentDayVictims = new VictimProfile[4];

    void Start()
    {
        GenerateVictimList();
    }

    public void GenerateVictimList()
    {
        if (allAvailableVictims.Length == 0) return;

        for (int i = 0; i < 4; i++)
        {
            // Bốc ngẫu nhiên (hoặc lặp lại nếu có ít file)
            int randomIndex = Random.Range(0, allAvailableVictims.Length);
            currentDayVictims[i] = allAvailableVictims[randomIndex];

            // TỰ ĐỘNG ĐỔI "NEW TEXT" THÀNH THÔNG TIN THẬT
            nameTexts[i].text = currentDayVictims[i].victimName;
            infoTexts[i].text = currentDayVictims[i].jobOrAge + " | Có thể lừa: $" + currentDayVictims[i].potentialReward;
            staminaTexts[i].text = "-" + currentDayVictims[i].staminaCost + " HP";

            // TỰ ĐỘNG GẮN LỆNH CHO NÚT "OK"
            int index = i;
            victimButtons[i].onClick.RemoveAllListeners();
            victimButtons[i].onClick.AddListener(() => OnVictimSelected(index));
        }

        selectionPanel.SetActive(true);
    }

    public void OnVictimSelected(int index)
    {
        VictimProfile selectedVictim = currentDayVictims[index];

        // Cập nhật số tiền thưởng tối đa cho Mini-game
        scamMinigame.maxMoneyReward = selectedVictim.potentialReward;

        // Tắt bảng danh sách, Mở điện thoại gõ chữ
        selectionPanel.SetActive(false);
        scamMinigame.StartMiniGame(selectedVictim.rounds);
    }
}