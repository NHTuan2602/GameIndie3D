using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Cài đặt Tuần tra")]
    public Transform[] waypoints;
    public float waitTime = 2f;

    [Header("Cài đặt Tốc độ")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 7f;

    [Header("Cài đặt Tầm nhìn (Field of View)")]
    public float viewRadius = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public Transform playerTarget;

    [Header("Cài đặt Đèn pin AI")]
    [Tooltip("Kéo thả Spot Light của Enemy vào đây")]
    public Light enemyFlashlight;

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }

        // Tự động đồng bộ góc chiếu sáng và độ xa của đèn pin với tầm nhìn của AI
        if (enemyFlashlight != null)
        {
            enemyFlashlight.color = Color.white;
            enemyFlashlight.spotAngle = viewAngle;
            enemyFlashlight.range = viewRadius;
        }
    }

    void Update()
    {
        CheckPlayerInSight();

        if (isChasing)
        {
            // Cập nhật liên tục vị trí của Player để đuổi theo
            agent.SetDestination(playerTarget.position);
        }
        else
        {
            PatrolRoutine();
        }
    }

    void CheckPlayerInSight()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= viewRadius)
        {
            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
            {
                Vector3 enemyEye = transform.position + Vector3.up * 1f;
                Vector3 playerEye = playerTarget.position + Vector3.up * 1f;

                RaycastHit hit;

                if (Physics.Linecast(enemyEye, playerEye, out hit))
                {
                    if (hit.transform == playerTarget)
                    {
                        // PHÁT HIỆN NGƯỜI CHƠI -> BẬT CHẾ ĐỘ RƯỢT ĐUỔI
                        if (!isChasing)
                        {
                            isChasing = true;
                            agent.speed = chaseSpeed;
                            GetComponent<Renderer>().material.color = Color.red;

                            // Đổi màu đèn pin sang ĐỎ rực
                            if (enemyFlashlight != null) enemyFlashlight.color = Color.red;
                        }
                        return;
                    }
                }
            }
        }

        // MẤT DẤU NGƯỜI CHƠI -> QUAY LẠI ĐI TUẦN
        if (isChasing)
        {
            isChasing = false;
            agent.speed = patrolSpeed;
            GetComponent<Renderer>().material.color = Color.white;

            // Đổi màu đèn pin về lại màu TRẮNG
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