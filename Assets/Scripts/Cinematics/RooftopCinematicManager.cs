using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class RooftopEscapeManager : MonoBehaviour
{
    public static RooftopEscapeManager instance;

    [Header("--- Diễn Viên & Đạo Cụ ---")]
    public GameObject player;
    public GameObject coiledRopeRooftop;
    public GameObject coiledRopeWall;
    public GameObject bikeChainVisual;
    public GameObject bikeVisual;
    public Image blackScreenFade;

    [Header("--- UI Tương tác (QTE) ---")]
    public TextMeshProUGUI interactPromptText;

    [Header("--- Diễn Viên Phản Diện ---")]
    public GameObject[] cinematicEnemies;
    public Transform enemyGateStartPos;

    [Header("--- Tọa độ Đóng Phim ---")]
    public Transform climbDownStart;
    public Transform climbDownEnd;
    public Transform wallClimbStart;
    public Transform wallClimbEnd;
    public Transform wallDropEnd;
    public Transform bikePosition;

    [Header("--- Âm Thanh Cinematic ---")]
    public AudioSource cinematicAudio;
    public AudioClip ropeThrowSound;
    public AudioClip climbingSound;
    public AudioClip cutSound;
    public AudioClip alarmSound;
    public AudioClip bikeRideSound;
    public AudioClip caughtJumpscareSound; // MỚI: Tiếng tèng téng teng khi bị bắt

    [Header("Chuyển Scene")]
    public string nextSceneName = "EscapeBikeScene";

    private bool hasRope = false;
    private bool sequenceStarted = false;
    private Transform mainCameraTransform;

    void Awake() { instance = this; }

    void Start()
    {
        if (cinematicAudio == null)
        {
            cinematicAudio = GetComponent<AudioSource>();
            if (cinematicAudio == null) cinematicAudio = gameObject.AddComponent<AudioSource>();
        }
        if (Camera.main != null) mainCameraTransform = Camera.main.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.instance != null && GameManager.instance.hasRope) hasRope = true;

        if (other.CompareTag("Player") && hasRope && !sequenceStarted)
        {
            sequenceStarted = true;
            GetComponent<Collider>().enabled = false;
            StartCoroutine(PlayEscapeSequence());
        }
    }

    IEnumerator PlayEscapeSequence()
    {
        if (player != null && player.GetComponent<PlayerController>() != null)
            player.GetComponent<PlayerController>().enabled = false;

        // ==========================================
        // PHẦN 1: TRÈO TỪ SÂN THƯỢNG XUỐNG (Chỉ chiếu 1 lần)
        // ==========================================
        if (climbDownStart != null) player.transform.position = climbDownStart.position;
        if (interactPromptText != null)
        {
            interactPromptText.text = "Nhấn [E] để buộc dây thừng trèo xuống";
            interactPromptText.gameObject.SetActive(true);
        }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
        if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);

        if (coiledRopeRooftop != null) coiledRopeRooftop.SetActive(true);
        if (cinematicAudio != null && ropeThrowSound != null) cinematicAudio.PlayOneShot(ropeThrowSound);
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(FadeScreen(1f, 0.5f));
        if (cinematicAudio != null && climbingSound != null) { cinematicAudio.clip = climbingSound; cinematicAudio.loop = true; cinematicAudio.Play(); }
        if (climbDownEnd != null) player.transform.position = climbDownEnd.position;
        yield return new WaitForSeconds(2f);
        if (cinematicAudio != null) { cinematicAudio.loop = false; cinematicAudio.Stop(); }
        yield return StartCoroutine(FadeScreen(0f, 0.5f));

        // ==========================================
        // VÒNG LẶP CHECKPOINT: NẾU THẤT BẠI SẼ QUAY LẠI ĐÂY
        // ==========================================
        bool escaped = false;
        while (!escaped)
        {
            // Reset vị trí xe đạp và xích (nếu chơi lại)
            if (bikeVisual != null) bikeVisual.SetActive(true);
            if (bikeChainVisual != null) bikeChainVisual.SetActive(true);

            // QUA BỨC TƯỜNG CAO
            yield return StartCoroutine(FadeScreen(1f, 0.3f));
            if (coiledRopeWall != null) coiledRopeWall.SetActive(true);
            if (wallClimbEnd != null) player.transform.position = wallClimbEnd.position;
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(FadeScreen(0f, 0.3f));

            // FIX LỖI ĐI XUYÊN TƯỜNG BẰNG ĐƯỜNG CONG PARABOL
            if (wallDropEnd != null)
            {
                float dropTime = 0.6f;
                float timer = 0f;
                Vector3 startPos = player.transform.position;
                Vector3 targetPos = wallDropEnd.position;

                while (timer < dropTime)
                {
                    timer += Time.deltaTime;
                    float progress = timer / dropTime;

                    // Đi ngang
                    Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
                    // Nhảy vòng cung (Cộng thêm chiều cao)
                    currentPos.y += Mathf.Sin(progress * Mathf.PI) * 1.5f;

                    player.transform.position = currentPos;
                    yield return null;
                }
            }

            // BÁO ĐỘNG & KẺ ĐỊCH XUẤT HIỆN
            if (cinematicAudio != null && alarmSound != null) { cinematicAudio.clip = alarmSound; cinematicAudio.loop = true; cinematicAudio.Play(); }
            foreach (GameObject enemyObj in cinematicEnemies)
            {
                if (enemyObj != null)
                {
                    EnemyPatrol patrol = enemyObj.GetComponent<EnemyPatrol>();
                    if (patrol != null) patrol.enabled = false;
                    NavMeshAgent agent = enemyObj.GetComponent<NavMeshAgent>();
                    if (agent != null && enemyGateStartPos != null)
                    {
                        agent.Warp(enemyGateStartPos.position);
                        agent.speed = 22f;
                        agent.SetDestination(bikePosition.position); // ĐÃ FIX: Chạy thẳng ra xe đạp
                    }
                    Animator anim = enemyObj.GetComponentInChildren<Animator>();
                    if (anim != null) { anim.SetBool("isWalking", false); anim.SetBool("isRunning", true); }
                }
            }

            // CHẠY TỚI XE ĐẠP
            if (bikePosition != null)
            {
                float runTime = 1.0f; float timer = 0f; Vector3 startPos = player.transform.position;
                while (timer < runTime) { timer += Time.deltaTime; player.transform.position = Vector3.Lerp(startPos, bikePosition.position, timer / runTime); player.transform.LookAt(bikePosition); yield return null; }
            }

            // ==========================================
            // TƯƠNG TÁC CÓ THỜI GIAN (QTE TIMEOUT)
            // ==========================================
            if (interactPromptText != null)
            {
                interactPromptText.text = "<color=red>NHẤN [E] ĐỂ CẮT XÍCH! KẺ ĐỊCH ĐANG TỚI!</color>";
                interactPromptText.gameObject.SetActive(true);
            }

            float timeLimit = 3.5f; // Người chơi có 3.5 giây để bấm E
            float qteTimer = 0f;
            bool qteSuccess = false;

            while (qteTimer < timeLimit)
            {
                qteTimer += Time.deltaTime;
                if (Input.GetKeyDown(KeyCode.E))
                {
                    qteSuccess = true;
                    break;
                }
                yield return null; // Chờ frame tiếp theo
            }

            if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);

            if (qteSuccess)
            {
                // THÀNH CÔNG: Thoát khỏi vòng lặp
                escaped = true;
            }
            else
            {
                // THẤT BẠI: Bị bắt, chạy Jumpscare và quay lại Checkpoint
                Debug.Log("<color=red>GAME OVER TẠM THỜI: Đã quá trễ!</color>");
                if (cinematicAudio != null && caughtJumpscareSound != null) cinematicAudio.PlayOneShot(caughtJumpscareSound);

                if (interactPromptText != null)
                {
                    interactPromptText.text = "<color=red>BẠN ĐÃ BỊ TÓM LẠI!</color>";
                    interactPromptText.gameObject.SetActive(true);
                }

                // Dừng kẻ địch
                foreach (GameObject enemyObj in cinematicEnemies)
                {
                    if (enemyObj != null) enemyObj.GetComponent<NavMeshAgent>().speed = 0;
                }

                yield return new WaitForSeconds(2f); // Nhìn dòng chữ bị tóm 2 giây
                if (interactPromptText != null) interactPromptText.gameObject.SetActive(false);

                // Mờ đen và đưa Player về chân tường để làm lại
                yield return StartCoroutine(FadeScreen(1f, 1f));
                player.transform.position = climbDownEnd.position;
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(FadeScreen(0f, 1f));
            }
        }

        // ==========================================
        // PHẦN 3: THÀNH CÔNG - PHÓNG XE & CAMERA LIA SAU LƯNG
        // ==========================================
        if (cinematicAudio != null && cutSound != null) cinematicAudio.PlayOneShot(cutSound);
        if (bikeChainVisual != null) bikeChainVisual.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        // Giật mình quay đầu nhìn kẻ địch
        if (enemyGateStartPos != null)
        {
            float turnTime = 0.3f; float turnTimer = 0f; Quaternion startRot = player.transform.rotation;
            Vector3 dir = (enemyGateStartPos.position - player.transform.position).normalized;
            dir.y = 0; Quaternion targetRot = Quaternion.LookRotation(dir);

            while (turnTimer < turnTime) { turnTimer += Time.deltaTime; player.transform.rotation = Quaternion.Slerp(startRot, targetRot, turnTimer / turnTime); yield return null; }
            yield return new WaitForSeconds(1f);
        }

        // BIẾN MẤT CÙNG XE ĐẠP (Giả vờ đã nhảy lên xe)
        if (bikeVisual != null) bikeVisual.SetActive(false);
        if (cinematicAudio != null && bikeRideSound != null) { cinematicAudio.Stop(); cinematicAudio.PlayOneShot(bikeRideSound); }

        // MỚI: CAMERA PAN RA SAU LƯNG ĐỂ NỐI SANG SCENE ĐUA XE
        if (mainCameraTransform != null)
        {
            Vector3 targetCamPos = player.transform.position - player.transform.forward * 4f + Vector3.up * 2f;
            float camPanTime = 1.5f;
            float camTimer = 0f;
            Vector3 startCamPos = mainCameraTransform.position;

            while (camTimer < camPanTime)
            {
                camTimer += Time.deltaTime;
                mainCameraTransform.position = Vector3.Lerp(startCamPos, targetCamPos, camTimer / camPanTime);
                mainCameraTransform.LookAt(player.transform.position + player.transform.forward * 10f); // Nhìn xa xăm về phía trước
                yield return null;
            }
        }

        // Mờ đen chuyển qua màn đua xe
        yield return StartCoroutine(FadeScreen(1f, 1.5f));
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeScreen(float targetAlpha, float duration)
    {
        if (blackScreenFade != null)
        {
            blackScreenFade.gameObject.SetActive(true);
            float startAlpha = blackScreenFade.color.a;
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                blackScreenFade.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, targetAlpha, time / duration));
                yield return null;
            }
            blackScreenFade.color = new Color(0, 0, 0, targetAlpha);
            if (targetAlpha == 0) blackScreenFade.gameObject.SetActive(false);
        }
    }
}