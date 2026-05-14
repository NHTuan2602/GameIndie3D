using UnityEngine;
using System.Collections;

public class TrashCanDistraction : MonoBehaviour
{
    [Header("Cài đặt Âm thanh")]
    public AudioClip thudSound;
    public float noiseRadius = 15f;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private bool isKnockedOver = false;
    private bool isResetting = false;
    private Rigidbody rb;

    void Start()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    public void KnockOver()
    {
        if (!isKnockedOver)
        {
            isKnockedOver = true;
            isResetting = false;

            // 1. Phát tiếng bịch ngay lúc bị sút
            if (thudSound != null)
            {
                AudioSource.PlayClipAtPoint(thudSound, transform.position);
            }

            // GÓC KHUẤT VẬT LÝ: Chờ 1.5 giây cho thùng rác rớt xuống và lăn xong thì mới báo AI
            StartCoroutine(NotifyAIAfterLanding());
        }
    }

    private IEnumerator NotifyAIAfterLanding()
    {
        // Đợi 1.5 giây
        yield return new WaitForSeconds(1.5f);

        // Lúc này transform.position chính là vị trí mới (nơi thùng rác đang nằm la liệt)
        EnemyPatrol[] allEnemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        foreach (EnemyPatrol enemy in allEnemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= noiseRadius)
            {
                Debug.Log($"<color=orange>[Thùng rác] Quái {enemy.name} nghe tiếng rớt và đang chạy tới vị trí mới!</color>");
                enemy.InvestigateNoise(transform.position);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isKnockedOver && !isResetting && other.CompareTag("Enemy"))
        {
            float distanceToTrash = Vector3.Distance(transform.position, other.transform.position);

            // ĐÃ NỚI LỎNG: Cho phép AI đứng cách 2 mét là đã khom lưng nhặt được rồi, tránh lỗi vấp
            if (distanceToTrash <= 2.0f)
            {
                Debug.Log("<color=yellow>[Thùng rác] Quái đã tới sát chân thùng rác! Khom lưng dọn... (Đợi 2s)</color>");
                StartCoroutine(ResetRoutine());
            }
        }
    }

    private IEnumerator ResetRoutine()
    {
        isResetting = true;
        yield return new WaitForSeconds(2f);
        ResetTrashCan();
    }

    private void ResetTrashCan()
    {
        isKnockedOver = false;
        isResetting = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            transform.position = originalPos;
            transform.rotation = originalRot;
            rb.isKinematic = false;
        }
        else
        {
            transform.position = originalPos;
            transform.rotation = originalRot;
        }

        Debug.Log("<color=green>[Thùng rác] Đã xếp ngay ngắn!</color>");
    }
}