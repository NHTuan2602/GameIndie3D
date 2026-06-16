using UnityEngine;

public class door : MonoBehaviour
{
    public int UseDoors = 1;
    public bool Door_in_use = false;

    public void Interact()
    {
        // ==========================================
        // ĐÃ FIX: CHỐT CHẶN KHÔNG CHO TẨU THOÁT SỚM
        // ==========================================
        if (GameManager.instance != null && GameManager.instance.currentDay < 5)
        {
            Debug.Log("<color=red>Cửa khóa chặt! Chưa đến đêm tẩu thoát (Đêm 5)!</color>");
            // MẸO: Nếu bạn có hệ thống hiện chữ (DialogueManager), hãy gọi nó ở đây để báo cho người chơi biết.
            return;
        }

        // Logic mở cửa cũ của bạn
        if (!Door_in_use)
        {
            StartCoroutine(OpenDoorRoutine());
        }
    }

    System.Collections.IEnumerator OpenDoorRoutine()
    {
        Door_in_use = true;

        // Bạn có thể chèn code xoay bản lề cửa hoặc LoadScene sang màn Đua Xe ở đây
        Debug.Log("Cánh cửa tự do đã mở!");

        yield return new WaitForSeconds(1f);
        Door_in_use = false;
    }
}