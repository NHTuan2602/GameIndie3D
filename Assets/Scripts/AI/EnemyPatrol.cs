using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    // ==========================================
    // MỚI: BIẾN TOÀN CỤC BÁO HIỆU NGƯỜI CHƠI TRỐN
    // ==========================================
    public static bool isPlayerHidden = false; 

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

    [Header("Cài đặt Âm thanh Bước chân (MỚI)")]
    public AudioSource footstepSource;
    public float patrolStepInterval = 0.6f;
    public float chaseStepInterval = 0.3f;
    public float slowStepInterval = 0.9f;
    private float stepTimer = 0f;

    private NavMeshAgent agent;
    private Animator anim;
    private int currentWaypointIndex = 0;

    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isChasing = false;
    private bool isSlowed = false;
    private bool isInvestigating = false;
    private Coroutine investigateRoutine;

    void Start()
    {
        isPlayerHidden = false; // Reset lại mỗi khi load màn
        agent = GetComponentInParent<NavMeshAgent>();
        if (agent == null) agent = GetComponentInChildren<NavMeshAgent>();

        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponentInParent<Animator>();

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

        if (footstepSource == null) footstepSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (agent == null || playerTarget == null) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (anim != null)
        {
            anim.SetBool("isWalking", isMoving && !isChasing && !isSlowed);
            anim.SetBool("isRunning", isMoving && isChasing && !isSlowed);
        }

        // ==========================================
        // MỚI: NGƯỜI CHƠI VÀO PHÒNG TRỐN -> HỦY RƯỢT ĐUỔI
        // ==========================================
        if (isPlayerHidden)
        {
            if (isChasing) GiveUpChase();
            else PatrolRoutine();
            HandleFootsteps(isMoving);
            return; // Dừng chạy code bên dưới, AI không check tầm nhìn nữa
        }

        CheckPlayerInSight();
        HandleFootsteps(isMoving);

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
        else if (!isInvestigating)
        {
            PatrolRoutine();
        }
    }

    void HandleFootsteps(bool isMoving)
    {
        if (isMoving)
        {
            stepTimer += Time.deltaTime;

            float currentInterval = patrolStepInterval;
            if (isChasing) currentInterval = chaseStepInterval;
            else if (isSlowed) currentInterval = slowStepInterval;

            if (stepTimer >= currentInterval)
            {
                if (AudioManager.instance != null && footstepSource != null)
                {
                    AudioManager.instance.PlayFootstep(footstepSource);
                }
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // MỚI: Cấm quái cắn nếu người chơi đang trong phòng trốn
        if (other.CompareTag("Player") && !isPlayerHidden)
        {
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
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= viewRadius)
        {
            Vector3 targetEyePos = playerTarget.position + (Vector3.up * headHeightOffset);
            Vector3 headPosition = transform.position + (Vector3.up * headHeightOffset);
            Vector3 directionToPlayer = (targetEyePos - headPosition).normalized;

            if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
            {
                // Nhớ đổi ObstacleMask thành Default trong Inspector để nó không nhìn thấu tường nhé!
                if (!Physics.Raycast(headPosition, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    currentChaseTimer = wallhackChaseTime;

                    if (!isChasing)
                    {
                        isChasing = true;

                        if (isInvestigating && investigateRoutine != null)
                        {
                            StopCoroutine(investigateRoutine);
                            isInvestigating = false;
                        }

                        if (!isSlowed) agent.speed = chaseSpeed;
                        if (enemyFlashlight != null && !isSlowed) enemyFlashlight.color = Color.red;

                        if (AudioManager.instance != null) AudioManager.instance.PlayAlarm(transform.position);
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

    public void InvestigateNoise(Vector3 noisePosition)
    {
        if (isChasing || isSlowed) return;

        isInvestigating = true;
        isWaiting = false;

        if (enemyFlashlight != null) enemyFlashlight.color = new Color(1f, 0.5f, 0f);

        if (investigateRoutine != null) StopCoroutine(investigateRoutine);
        investigateRoutine = StartCoroutine(InvestigateRoutine(noisePosition));
    }

    IEnumerator InvestigateRoutine(Vector3 targetPos)
    {
        agent.SetDestination(targetPos);
        while (agent.pathPending || agent.remainingDistance > 1.5f) yield return null;

        if (anim != null) anim.SetBool("isWalking", false);
        yield return new WaitForSeconds(4f);

        isInvestigating = false;
        if (enemyFlashlight != null) enemyFlashlight.color = Color.white;
        if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}