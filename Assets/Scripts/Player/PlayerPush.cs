using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    [Header("Cài đặt Lực Đá")]
    public float pushPower = 5f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic) return;
        if (hit.moveDirection.y < -0.3f) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.AddForce(pushDir * pushPower, ForceMode.Impulse);

        // ĐÃ THÊM: Kích hoạt tiếng kêu và gọi quái từ script của thùng rác
        TrashCanDistraction trashCan = hit.gameObject.GetComponent<TrashCanDistraction>();
        if (trashCan != null)
        {
            trashCan.KnockOver();
        }
    }
}