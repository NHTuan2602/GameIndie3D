using UnityEngine;

public class ItemHover : MonoBehaviour
{
    [Header("Cài đặt Chuyển động")]
    public float spinSpeed = 100f;  // Tốc độ xoay (độ/giây)
    public float bobSpeed = 3f;     // Nhịp đập bồng bềnh
    public float bobHeight = 0.5f;  // Độ cao nảy lên

    private Vector3 startPos;

    void Start()
    {
        // Ghi nhớ vị trí ban đầu lúc nó vừa được sinh ra
        startPos = transform.position;
    }

    void Update()
    {
        // 1. XOAY VÒNG TRÒN quanh trục Y
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);

        // 2. NHẤP NHÔ LÊN XUỐNG bằng hàm Sin
        float newY = startPos.y + (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}