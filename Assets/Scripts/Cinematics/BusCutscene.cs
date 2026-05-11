using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BusCutscene : MonoBehaviour
{
    [Header("--- DI CHUYỂN XE BUS ---")]
    public Transform stopPoint;
    public float driveSpeed = 20f;

    [Header("--- HỆ THỐNG CỔNG ---")]
    public Transform leftGate;
    public Transform rightGate;
    [Tooltip("Nên để tầm 40-50 nếu xe chạy nhanh")]
    public float gateOpenDistance = 45f;
    public float gateOpenSpeed = 1.5f;

    public float leftOpenAngle = -90f;
    public float rightOpenAngle = 90f;

    private bool isGateOpening = false;

    void Start()
    {
        if (stopPoint == null || leftGate == null || rightGate == null)
        {
            Debug.LogError("<color=red>LỖI: Bạn chưa kéo đủ Cổng hoặc StopPoint vào xe bus!</color>");
            return;
        }
        StartCoroutine(CutsceneSequence());
    }

    IEnumerator CutsceneSequence()
    {
        while (Vector3.Distance(transform.position, stopPoint.position) > 0.5f)
        {
            // Di chuyển xe
            transform.position = Vector3.MoveTowards(transform.position, stopPoint.position, driveSpeed * Time.deltaTime);

            // TÍNH KHOẢNG CÁCH ĐẾN CỔNG
            float distToGate = Vector3.Distance(transform.position, leftGate.position);

            // IN RA CONSOLE ĐỂ KIỂM TRA (Bạn nhìn vào đây lúc chạy game)
            // Debug.Log("Khoảng cách đến cổng: " + distToGate);

            if (!isGateOpening && distToGate <= gateOpenDistance)
            {
                Debug.Log("<color=cyan>ĐÃ CHẠM NGƯỠNG KÍCH HOẠT! ĐANG MỞ CỔNG...</color>");
                isGateOpening = true;
                StartCoroutine(OpenGatesRoutine());
            }

            yield return null;
        }

        Debug.Log("<color=green>XE ĐÃ DỪNG! Đang chờ 2 giây để chuyển sang ScamScreen...</color>");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("ScamScreen");
    }

    IEnumerator OpenGatesRoutine()
    {
        Quaternion startLeft = leftGate.localRotation;
        Quaternion startRight = rightGate.localRotation;

        // Dùng Euler trực tiếp để tránh lỗi quay ngược
        Quaternion targetLeft = Quaternion.Euler(0, leftOpenAngle, 0);
        Quaternion targetRight = Quaternion.Euler(0, rightOpenAngle, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * gateOpenSpeed;
            leftGate.localRotation = Quaternion.Slerp(startLeft, targetLeft, t);
            rightGate.localRotation = Quaternion.Slerp(startRight, targetRight, t);
            yield return null;
        }
    }

    // Vẽ một vòng tròn đỏ trong Scene để bạn dễ nhìn tầm kích hoạt
    void OnDrawGizmosSelected()
    {
        if (leftGate != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leftGate.position, gateOpenDistance);
        }
    }
}