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
    public Transform playerTarget; // BẮT BUỘC KÉO PLAYER VÀO ĐÂY Ở INSPECTOR

    [Header("Cài đặt Đèn pin AI")]
    public Light enemyFlashlight;

    private NavMeshAgent agent;
    private Animator anim;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isChasing = false;
    private bool isStunned = false;

    void Start()
    {
        // Tự động tìm Agent và Animator thông minh (gắn ở đâu cũng chạy được)
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
    }

    void Update()
    {
        // CHỐNG LỖI SẬP GAME: Nếu quên kéo Player vào Inspector thì code dừng lại an toàn
        if (agent == null || playerTarget == null) return;

        // Xử lý Animation mượt mà
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
            agent.SetDestination(playerTarget.position);
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
            HandleCatchingPlayer();
        }
    }

    void HandleCatchingPlayer()
    {
        if (GameManager.instance != null)
        {
            bool shouldResetLevel = GameManager.instance.OnPlayerCaught();
            if (!shouldResetLevel) StartCoroutine(GetStunned(5f));
        }
        else
        {
            // Dự phòng: Nếu chưa có GameManager, cứ choáng tạm 5s
            StartCoroutine(GetStunned(5f));
        }
    }

    IEnumerator GetStunned(float duration)
    {
        isStunned = true;
        isChasing = false;
        if (agent != null) agent.isStopped = true;

        if (anim != null)
        {
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", false);
        }

        if (enemyFlashlight != null) enemyFlashlight.color = Color.yellow;

        yield return new WaitForSeconds(duration);

        isStunned = true; // Sửa lỗi: Giữ trạng thái stun đến khi hết hàm
        isStunned = false;

        if (agent != null) agent.isStopped = false;
        if (enemyFlashlight != null) enemyFlashlight.color = Color.white;

        if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void CheckPlayerInSight()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= viewRadius)
        {
            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
            {
                // Phát hiện trong nón nhìn -> Đuổi!
                if (!isChasing)
                {
                    isChasing = true;
                    agent.speed = chaseSpeed;
                    if (enemyFlashlight != null) enemyFlashlight.color = Color.red;
                }
                return;
            }
        }

        // Nếu người chơi chạy thoát khỏi tầm nhìn
        if (isChasing)
        {
            isChasing = false;
            agent.speed = patrolSpeed;
            if (enemyFlashlight != null) enemyFlashlight.color = Color.white;
            if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
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