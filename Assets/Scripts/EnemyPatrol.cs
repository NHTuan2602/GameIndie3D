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
    public float slowSpeed = 1.5f; // Tốc độ rùa bò sau khi chộp hụt người chơi

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

    [Header("Tối ưu Tầm nhìn")]
    public float headHeightOffset = 1.6f;

    // ==========================================
    // MỚI: CƠ CHẾ WALLHACK 10 GIÂY
    // ==========================================
    [Header("Cơ chế Chó Săn (Wallhack)")]
    public float wallhackChaseTime = 10f; // Bám đuôi xuyên tường 10 giây
    private float currentChaseTimer = 0f; // Đồng hồ đếm ngược

    private NavMeshAgent agent;
    private Animator anim;
    private PlayerHide playerHideState;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    private bool isChasing = false;
    private bool isSlowed = false; // Thay thế isStunned bằng isSlowed

    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        if (agent == null) agent = GetComponentInChildren<NavMeshAgent>();

        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponentInParent<Animator>();

        if (playerTarget != null)
        {
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
            anim.SetBool("isWalking", isMoving && !isChasing && !isSlowed);
            anim.SetBool("isRunning", isMoving && isChasing && !isSlowed);
            // Nếu muốn bạn có thể thêm anim.SetBool("isLimping", isSlowed) sau này
        }

        CheckPlayerInSight();

        if (isChasing)
        {
            // GIẢM ĐỒNG HỒ ĐẾM NGƯỢC 10 GIÂY
            currentChaseTimer -= Time.deltaTime;

            if (currentChaseTimer > 0)
            {
                // CÒN THỜI GIAN -> TRUY CÙNG ĐUỔI TẬN (XUYÊN TƯỜNG)
                pathUpdateTimer += Time.deltaTime;
                if (pathUpdateTimer >= pathUpdateDelay)
                {
                    agent.SetDestination(playerTarget.position);
                    pathUpdateTimer = 0f;
                }
            }
            else
            {
                // HẾT 10 GIÂY -> BỎ CUỘC
                GiveUpChase();
            }
        }
        else
        {
            PatrolRoutine();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerHideState != null && playerHideState.isHidden)
            {
                if (!isChasing) return;
                else Debug.Log("<color=red>AI: Tưởng chui tủ mà thoát à? Ra đây!</color>");
            }

            // Tránh việc AI bị gọi hàm bắt nhiều lần liên tục
            if (!isSlowed) HandleCatchingPlayer();
        }
    }

    void HandleCatchingPlayer()
    {
        if (GameManager.instance != null)
        {
            bool isOutForTonight = GameManager.instance.OnPlayerCaught();
            if (!isOutForTonight)
            {
                // Người chơi vùng vẫy thành công -> Bảo vệ bị chậm 5 giây
                StartCoroutine(ApplySlowPenalty(5f));
            }
        }
        else
        {
            StartCoroutine(ApplySlowPenalty(5f));
        }
    }

    // ==========================================
    // MỚI: BẢO VỆ BỊ GIẢM TỐC ĐỘ 5 GIÂY (Không đứng im)
    // ==========================================
    IEnumerator ApplySlowPenalty(float duration)
    {
        isSlowed = true;

        if (agent != null)
        {
            agent.speed = slowSpeed; // Ép xuống tốc độ đi bộ chậm
        }

        if (enemyFlashlight != null) enemyFlashlight.color = Color.yellow; // Đèn vàng báo hiệu đang yếu

        Debug.Log("<color=orange>BẢO VỆ: Áu, nó đá mình! Đi chậm lại 5 giây...</color>");

        yield return new WaitForSeconds(duration);

        // HẾT 5 GIÂY -> HỒI PHỤC THỂ LỰC
        isSlowed = false;

        if (agent != null)
        {
            // Nếu vẫn đang trong 10s rượt đuổi thì phi nhanh tiếp, không thì đi tuần
            agent.speed = isChasing ? chaseSpeed : patrolSpeed;
        }

        if (enemyFlashlight != null)
        {
            enemyFlashlight.color = isChasing ? Color.red : Color.white;
        }

        Debug.Log("<color=green>BẢO VỆ: Đã hồi phục! Rượt tiếp!</color>");
    }

    void CheckPlayerInSight()
    {
        // Núp rồi thì không phát hiện MỚI được, nhưng nếu đang rượt thì vẫn bị Rada quét
        if (playerHideState != null && playerHideState.isHidden && !isChasing) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= viewRadius)
        {
            Vector3 targetEyePos = playerTarget.position + (Vector3.up * headHeightOffset);
            Vector3 headPosition = transform.position + (Vector3.up * headHeightOffset);
            Vector3 directionToPlayer = (targetEyePos - headPosition).normalized;

            if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
            {
                if (!Physics.Raycast(headPosition, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    // QUAN TRỌNG: MỖI KHI NHÌN THẤY, RADA ĐƯỢC SẠC ĐẦY LẠI 10 GIÂY
                    currentChaseTimer = wallhackChaseTime;

                    if (!isChasing)
                    {
                        isChasing = true;
                        if (!isSlowed) agent.speed = chaseSpeed; // Chỉ tăng tốc nếu không bị thọt
                        if (enemyFlashlight != null && !isSlowed) enemyFlashlight.color = Color.red;
                    }
                }
            }
        }
    }

    void GiveUpChase()
    {
        isChasing = false;

        if (!isSlowed)
        {
            agent.speed = patrolSpeed;
            if (enemyFlashlight != null) enemyFlashlight.color = Color.white;
        }

        if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
        Debug.Log("AI: Cắt đuôi thành công! Quay về đi tuần.");
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