using UnityEngine;

public class SmartTrafficVehicle : MonoBehaviour
{
    [Header("Cài đặt Tốc độ")]
    public float defaultSpeed = 30f;
    private float currentSpeed;

    [Header("Cảm biến Radar (Raycast)")]
    public float sensorLength = 25f; // Tầm nhìn xa của radar (Mũi tên đỏ)
    public float safeDistance = 8f; // Khoảng cách tối thiểu để đạp phanh gấp
    public Vector3 sensorOffset = new Vector3(0, 1f, 0); // Nâng radar lên 1m khỏi mặt đường để không quét nhầm nhựa đường

    void Start()
    {
        currentSpeed = defaultSpeed;
    }

    void Update()
    {
        // 1. Chạy hệ thống Radar để quyết định tốc độ
        HandleTrafficAI();

        // 2. Di chuyển xe liên tục theo hướng mũi tên xanh dương (trục Z)
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self);
    }

    void HandleTrafficAI()
    {
        // Điểm bắt đầu bắn tia (ngay mũi xe, nâng lên một chút)
        Vector3 origin = transform.position + sensorOffset;
        Vector3 direction = transform.forward; // Luôn bắn thẳng về phía trước của đầu xe

        // Tự động vẽ tia laser màu đỏ trong tab Scene để bạn dễ canh chỉnh tầm nhìn
        Debug.DrawRay(origin, direction * sensorLength, Color.red);

        RaycastHit hit;

        // Bóp cò bắn Radar
        if (Physics.Raycast(origin, direction, out hit, sensorLength))
        {
            // KỂ KIỂM TRA QUAN TRỌNG: Đảm bảo tia laser chỉ phản ứng với các xe khác
            // Lưu ý: Các xe tải/xe khác của bạn PHẢI ĐƯỢC GẮN TAG LÀ "Obstacle"
            if (hit.collider.CompareTag("Obstacle"))
            {
                float distanceToCarAhead = hit.distance;

                if (distanceToCarAhead <= safeDistance)
                {
                    // Quá gần -> Phanh gấp! 
                    // Ép tốc độ thấp hơn xe tải một chút (ví dụ 18km/h) để không bị húc đít
                    currentSpeed = Mathf.Lerp(currentSpeed, 18f, Time.deltaTime * 5f);
                }
                else
                {
                    // Thấy xe tải từ xa -> Bắt đầu rà phanh giảm tốc từ từ xuống 20km/h
                    currentSpeed = Mathf.Lerp(currentSpeed, 20f, Time.deltaTime * 2f);
                }

                return; // Thoát hàm ngay để không chạy lệnh tăng tốc ở bên dưới
            }
        }

        // Nếu Radar không thấy ai (Đường phía trước thoáng) -> Đạp ga về lại tốc độ 30!
        currentSpeed = Mathf.Lerp(currentSpeed, defaultSpeed, Time.deltaTime * 2f);
    }
}