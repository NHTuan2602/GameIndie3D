using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    public LayerMask obstacleMask;

    [Header("Cài đặt Đèn pin AI")]
    public Light enemyFlashlight;

    [Header("Tối ưu Tìm đường")]
    public float pathUpdateDelay = 0.2f;
    private float pathUpdateTimer = 0f;

    [Header("Tối ưu Tầm nhìn (Chống nhìn xuyên đầu)")]
    public float headHeightOffset = 1.6f;

    private NavMeshAgent agent;
    private Animator anim;
    private PlayerHide playerHideState; // Biến để AI "đọc vị" người chơi
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isChasing = false;
    private bool isStunned = false;
    [Header("Trí nhớ AI (Chống cà giật cầu thang)")]
    public float loseSightDelay = 2f; // Sẽ tìm thêm 2 giây nếu mất dấu
    private float timeSinceLastSeen = 0f;
    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        if (agent == null) agent = GetComponentInChildren<NavMeshAgent>();

        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponentInParent<Animator>();

        if (playerTarget != null)
        {
            // Tự động tìm script Núp của người chơi
            playerHideState = playerTarget.GetComponent<PlayerHide>();
        }

        if (agent != null)
        {
            agent.speed = patrolSpeed;
            if (waypoints.Length > 0) agent.SetDestination(waypoints[0].position);
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
        if (agent == null || playerTarget == null) return;

        if (anim != null)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            anim.SetBool("isWalking", isMoving && !isChasing);
            anim.SetBool("isRunning", isMoving && isChasing);
        }

        if (isStunned) return;

        CheckPlayerInSight();

        if (isChasing)
        {
            pathUpdateTimer += Time.deltaTime;
            if (pathUpdateTimer >= pathUpdateDelay)
            {
                agent.SetDestination(playerTarget.position);
                pathUpdateTimer = 0f;
            }
        }
        else
        {
            PatrolRoutine();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isStunned)
        {
            // LOGIC CHỐNG NÚP TRỄ & CHỐNG CHẾT OAN
            if (playerHideState != null && playerHideState.isHidden)
            {
                if (!isChasing)
                {
                    // Đang núp an toàn, AI đi ngang đụng phải -> Lờ đi
                    return;
                }
                else
                {
                    // NÚP TRỄ LÚC NÓ ĐANG RƯỢT -> Bị lôi ra đánh!
                    Debug.Log("<color=red>AI: Dám chui tủ trước mặt tao à? Ra đây!</color>");
                }
            }

            HandleCatchingPlayer();
        }
    }

    void HandleCatchingPlayer()
    {
        if (GameManager.instance != null)
        {
            // Gọi lệnh và nhận kết quả xem người chơi còn mạng không
            bool isOutForTonight = GameManager.instance.OnPlayerCaught();

            if (!isOutForTonight)
            {
                // Nếu vẫn còn lượt (lần 1 hoặc 2), bảo vệ bị choáng để người chơi chạy tiếp
                StartCoroutine(GetStunned(5f));
            }
            // Nếu isOutForTonight là true, GameManager đã tự chuyển Scene, AI không cần làm gì thêm
        }
    }

    IEnumerator GetStunned(float duration)
    {
        isStunned = true;
        isChasing = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (anim != null)
        {
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", false);
        }

        if (enemyFlashlight != null) enemyFlashlight.color = Color.yellow;

        yield return new WaitForSeconds(duration);

        isStunned = false;

        if (agent != null) agent.isStopped = false;
        if (enemyFlashlight != null) enemyFlashlight.color = Color.white;

        if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void CheckPlayerInSight()
    {
        // NẾU NGƯỜI CHƠI ĐÃ NÚP AN TOÀN TRƯỚC -> AI BỊ MÙ
        if (playerHideState != null && playerHideState.isHidden && !isChasing) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        bool canSeePlayerThisFrame = false; // Biến đánh dấu xem frame này có thấy không

        if (distanceToPlayer <= viewRadius)
        {
            Vector3 targetEyePos = playerTarget.position + (Vector3.up * headHeightOffset);
            Vector3 headPosition = transform.position + (Vector3.up * headHeightOffset);
            Vector3 directionToPlayer = (targetEyePos - headPosition).normalized;

            if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
            {
                // Bắn tia Raycast
                if (!Physics.Raycast(headPosition, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    canSeePlayerThisFrame = true;
                    timeSinceLastSeen = 0f; // Đang nhìn thấy -> Reset trí nhớ về 0

                    if (!isChasing)
                    {
                        isChasing = true;
                        agent.speed = chaseSpeed;
                        pathUpdateTimer = pathUpdateDelay;
                        if (enemyFlashlight != null) enemyFlashlight.color = Color.red;
                    }
                }
            }
        }

        // --- LOGIC XỬ LÝ KHI ĐANG RƯỢT ĐUỔI ---
        if (isChasing)
        {
            if (canSeePlayerThisFrame)
            {
                // Đang thấy tận mắt -> Cứ rượt tiếp, thoát hàm
                return;
            }
            else
            {
                // BỊ KHUẤT TẦM NHÌN (Vấp cầu thang, nấp sau tường...)
                timeSinceLastSeen += Time.deltaTime; // Bắt đầu đếm ngược thời gian nhớ

                // Nếu núp tủ trước mặt nó (quá gần) thì nó không tha
                if (playerHideState != null && playerHideState.isHidden && distanceToPlayer < 3f)
                {
                    timeSinceLastSeen = 0f; // Bắt nó nhớ mãi mãi để lôi ra đập
                    return;
                }

                // Nếu quá 2 giây mà vẫn không thấy -> CHÍNH THỨC MẤT DẤU
                if (timeSinceLastSeen > loseSightDelay)
                {
                    isChasing = false;
                    agent.speed = patrolSpeed;
                    if (enemyFlashlight != null) enemyFlashlight.color = Color.white;
                    if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
                }

                // Lưu ý: Nếu timeSinceLastSeen CHƯA QUÁ 2 giây, isChasing vẫn là true
                // Hàm Update() ở trên vẫn sẽ bắt con AI chạy tiếp về phía bạn! Cà giật sẽ bị triệt tiêu!
            }
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
                if (anim != null) anim.SetBool("isWalking", false);
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