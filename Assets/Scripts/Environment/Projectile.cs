using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 40f;

    void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // CHỈ XÓA VIÊN ĐẠN CHO ĐẸP MẮT (Vì hệ thống 1 giây bên kia đã lo việc trừ máu rồi)
            Destroy(gameObject);
        }
    }
}