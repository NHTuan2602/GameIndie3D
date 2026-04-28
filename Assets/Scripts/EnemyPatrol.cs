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
    public float slowSpeed = 1.5f;

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

    [Header("Cơ chế Chó Săn (Wallhack)")]
    public float wallhackChaseTime = 10f;
    private float currentChaseTimer = 0f;

    private NavMeshAgent agent;
    private Animator anim;
    private PlayerHide playerHideState;
    private int currentWaypointIndex = 0;

    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isChasing = false;
    private bool isSlowed = false;

    // MỚI: TRẠNG THÁI KIỂM TRA TIẾNG ỒN
    private bool isInvestigating = false;
    private Coroutine investigateRoutine;

    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        if (agent == null) agent = GetComponentInChildren<NavMeshAgent>();

        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponentInParent<Animator>();

        if (playerTarget != null) playerHideState = playerTarget.GetComponent<PlayerHide>();

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
            // Animation đi bộ kích hoạt cả lúc tuần tra VÀ lúc đi kiểm tra tiếng ồn
            anim.SetBool("isWalking", isMoving && !isChasing && !isSlowed);
            anim.SetBool("isRunning", isMoving && isChasing && !isSlowed);
        }

        CheckPlayerInSight();

        if (isChasing)
        {
            currentChaseTimer -= Time.deltaTime;
            if (currentChaseTimer > 0)
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
                GiveUpChase();
            }
        }
        else if (!isInvestigating) // Không rượt, không tìm tiếng ồn thì mới đi tuần
        {
            PatrolRoutine();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerHideState != null && playerHideState.isHidden && !isChasing) return;
            if (!isSlowed) HandleCatchingPlayer();
        }
    }

    void HandleCatchingPlayer()
    {
        if (GameManager.instance != null)
        {
            bool isOutForTonight = GameManager.instance.OnPlayerCaught();
            if (!isOutForTonight) StartCoroutine(ApplySlowPenalty(5f));
        }
        else StartCoroutine(ApplySlowPenalty(5f));
    }

    IEnumerator ApplySlowPenalty(float duration)
    {
        isSlowed = true;
        if (agent != null) agent.speed = slowSpeed;
        if (enemyFlashlight != null) enemyFlashlight.color = Color.yellow;

        yield return new WaitForSeconds(duration);

        isSlowed = false;
        if (agent != null) agent.speed = isChasing ? chaseSpeed : patrolSpeed;
        if (enemyFlashlight != null) enemyFlashlight.color = isChasing ? Color.red : Color.white;
    }

    void CheckPlayerInSight()
    {
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
                    currentChaseTimer = wallhackChaseTime;

                    if (!isChasing)
                    {
                        isChasing = true;

                        // NẾU ĐANG TÌM TIẾNG ỒN MÀ THẤY MẶT -> HỦY TÌM, CHUYỂN SANG RƯỢT!
                        if (isInvestigating && investigateRoutine != null)
                        {
                            StopCoroutine(investigateRoutine);
                            isInvestigating = false;
                        }

                        if (!isSlowed) agent.speed = chaseSpeed;
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

    // ==========================================
    // MỚI: HÀM NGHE VÀ TÌM KIẾM TIẾNG ỒN
    // ==========================================
    public void InvestigateNoise(Vector3 noisePosition)
    {
        // 1. Phớt lờ tiếng ồn nếu đang rượt trối chết hoặc đang bị choáng chân
        if (isChasing || isSlowed) return;

        // 2. Chuyển sang trạng thái tìm kiếm
        isInvestigating = true;
        isWaiting = false; // Xóa trạng thái đứng yên tuần tra

        if (enemyFlashlight != null) enemyFlashlight.color = new Color(1f, 0.5f, 0f); // Đèn chuyển màu Cam cam (Nghi ngờ)

        Debug.Log("<color=orange>AI: Tiếng gì đấy? Để ra xem thử...</color>");

        // 3. Hủy lệnh tìm kiếm cũ (nếu có tiếng ồn mới) và chạy lệnh mới
        if (investigateRoutine != null) StopCoroutine(investigateRoutine);
        investigateRoutine = StartCoroutine(InvestigateRoutine(noisePosition));
    }

    IEnumerator InvestigateRoutine(Vector3 targetPos)
    {
        // Ra lệnh đi tới chỗ phát ra tiếng động
        agent.SetDestination(targetPos);

        // Chờ AI đi tới nơi (Chừa lại khoảng 1.5m để không đâm đầu vào bàn ghế)
        while (agent.pathPending || agent.remainingDistance > 1.5f)
        {
            yield return null;
        }

        // Tới nơi rồi -> Đứng ngó nghiêng 4 giây
        if (anim != null) anim.SetBool("isWalking", false);
        Debug.Log("<color=yellow>AI: Đứng tìm... Không có ai à?</color>");

        yield return new WaitForSeconds(4f);

        // Không thấy gì -> Quay lại đi tuần
        Debug.Log("<color=green>AI: Chắc chuột chạy. Quay lại đi tuần thôi.</color>");
        isInvestigating = false;

        if (enemyFlashlight != null) enemyFlashlight.color = Color.white;
        if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}