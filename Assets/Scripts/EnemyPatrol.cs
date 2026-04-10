using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Bắt buộc phải có cái này để chạy đếm ngược 5 giây

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Cài đặt Tuần tra")]
    public Transform[] waypoints;
    public float waitTime = 2f;

    [Header("Cài đặt Tốc độ")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 7f;

    [Header("Cài đặt Tầm nhìn")]
    public float viewRadius = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public Transform playerTarget;

    [Header("Cài đặt Đèn pin AI")]
    public Light enemyFlashlight;

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isChasing = false;

    // MỚI: Trạng thái choáng
    private bool isStunned = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }

        if (enemyFlashlight != null)
        {
            enemyFlashlight.color = Color.white;
            enemyFlashlight.spotAngle = viewAngle;
            enemyFlashlight.range = viewRadius;
        }
    }

    void Update()
    {
        // Nếu đang bị choáng thì đứng im, không nhìn, không đuổi
        if (isStunned) return;

        CheckPlayerInSight();

        if (isChasing)
        {
            agent.SetDestination(playerTarget.position);
        }
        else
        {
            PatrolRoutine();
        }
    }

    // ==========================================
    // PHẦN XỬ LÝ VA CHẠM (BẮT NGƯỜI & LÀM CHOÁNG)
    // ==========================================
    private void OnTriggerEnter(Collider other)
    {
        // Đặt bẫy Log để xem quái đụng trúng cái gì (Nhìn góc dưới bên trái màn hình Unity)
        Debug.Log("<color=cyan>QUÁI VẬT VỪA CHẠM VÀO: " + other.gameObject.name + " | CÓ TAG LÀ: " + other.tag + "</color>");

        // Nếu chạm đúng Player và quái đang không bị choáng
        if (other.CompareTag("Player") && !isStunned)
        {
            HandleCatchingPlayer();
        }
    }

    void HandleCatchingPlayer()
    {
        // Gọi sang GameManager để tính toán 3 mạng
        if (GameManager.instance != null)
        {
            bool shouldResetLevel = GameManager.instance.OnPlayerCaught();

            if (!shouldResetLevel)
            {
                // Nếu chưa hết 3 mạng -> Vùng vẫy làm choáng quái vật 5 giây
                StartCoroutine(GetStunned(5f));
            }
            // Nếu shouldResetLevel = true thì GameManager đã tự động đá về phòng ngủ rồi, quái không cần làm gì thêm.
        }
        else
        {
            Debug.LogError("LỖI: Không tìm thấy GameManager trong Scene! Hãy chạy game từ đầu hoặc ném Prefab GameManager vào Scene.");
        }
    }

    IEnumerator GetStunned(float duration)
    {
        isStunned = true;
        isChasing = false;
        agent.isStopped = true; // Phanh gấp, không di chuyển nữa

        // Hiệu ứng: Đèn pin và màu người đổi sang VÀNG CHÓI
        if (enemyFlashlight != null) enemyFlashlight.color = Color.yellow;
        GetComponent<Renderer>().material.color = Color.yellow;

        // Đứng hình 5 giây
        yield return new WaitForSeconds(duration);

        // Hết 5 giây, tỉnh dậy
        isStunned = false;
        agent.isStopped = false; // Cho phép đi lại
        if (enemyFlashlight != null) enemyFlashlight.color = Color.white;
        GetComponent<Renderer>().material.color = Color.white;

        // Trở về đi tuần tra (để người chơi có cơ hội trốn)
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    // ==========================================
    // PHẦN TẦM NHÌN (ĐÃ FIX LỖI TIA BẮN QUA ĐẦU)
    // ==========================================
    void CheckPlayerInSight()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= viewRadius)
        {
            CharacterController playerCC = playerTarget.GetComponent<CharacterController>();
            Vector3 playerCenter = playerTarget.position;

            if (playerCC != null) playerCenter = playerTarget.position + playerCC.center;
            else playerCenter = playerTarget.position + Vector3.up * 1f;

            Vector3 directionToPlayer = (playerCenter - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
            {
                Vector3 enemyEye = transform.position + Vector3.up * 1.5f;
                RaycastHit hit;

                if (Physics.Linecast(enemyEye, playerCenter, out hit))
                {
                    if (hit.transform == playerTarget || hit.transform.CompareTag("Player"))
                    {
                        if (!isChasing)
                        {
                            isChasing = true;
                            agent.speed = chaseSpeed;
                            GetComponent<Renderer>().material.color = Color.red;
                            if (enemyFlashlight != null) enemyFlashlight.color = Color.red;
                        }
                        return;
                    }
                }
            }
        }

        if (isChasing)
        {
            isChasing = false;
            agent.speed = patrolSpeed;
            GetComponent<Renderer>().material.color = Color.white;
            if (enemyFlashlight != null) enemyFlashlight.color = Color.white;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void PatrolRoutine()
    {
        if (waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
            }
            else
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTime)
                {
                    isWaiting = false;
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                }
            }
        }
    }
}