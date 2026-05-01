using UnityEngine;

public class IntroStoryTrigger : MonoBehaviour
{
    [Header("Kịch bản tự giới thiệu")]
    public DialogueLine[] introStoryLines;

    void Start()
    {
        // Vừa vào game là gọi hàm StartDialogue, truyền kịch bản vào
        // Chỗ () => { ... } nghĩa là: "Nói xong thì làm cái trong ngoặc này nhé"
        DialogueManager.instance.StartDialogue(introStoryLines, () =>
        {
            Debug.Log("Đã giới thiệu xong! Bắt đầu cho người chơi đi lại chơi game.");
            // Ở đây bạn có thể gọi hàm bật nhiệm vụ mới, xuất hiện quái, v.v...
        });
    }
}