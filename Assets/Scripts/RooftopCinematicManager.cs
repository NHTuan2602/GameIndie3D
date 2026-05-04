using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
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

    [Header("Chuyển Scene")]
    public string nextSceneName = "EscapeBikeScene";

    private bool hasRope = false;
    private bool sequenceStarted = false;

    void Awake() { instance = this; }

    void Start()
    {
        if (cinematicAudio == null)
        {
            cinematicAudio = GetComponent<AudioSource>();
            if (cinematicAudio == null) cinematicAudio = gameObject.AddComponent<AudioSource>();
        }
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

        // ĐOẠN 1: TRÈO XUỐNG
        if (climbDownStart != null) player.transform.position = climbDownStart.position;
        if (coiledRopeRooftop != null) coiledRopeRooftop.SetActive(true);
        if (cinematicAudio != null && ropeThrowSound != null) cinematicAudio.PlayOneShot(ropeThrowSound);
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeScreen(1f, 0.5f));
        if (cinematicAudio != null && climbingSound != null) { cinematicAudio.clip = climbingSound; cinematicAudio.loop = true; cinematicAudio.Play(); }
        if (climbDownEnd != null) player.transform.position = climbDownEnd.position;
        yield return new WaitForSeconds(2f);
        if (cinematicAudio != null) { cinematicAudio.loop = false; cinematicAudio.Stop(); }
        yield return StartCoroutine(FadeScreen(0f, 0.5f));

        // ĐOẠN 2: QUA TƯỜNG
        yield return StartCoroutine(FadeScreen(1f, 0.3f));
        if (coiledRopeWall != null) coiledRopeWall.SetActive(true);
        if (wallClimbEnd != null) player.transform.position = wallClimbEnd.position;
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(FadeScreen(0f, 0.3f));
        if (wallDropEnd != null)
        {
            float dropTime = 0.5f; float timer = 0f; Vector3 startPos = player.transform.position;
            while (timer < dropTime) { timer += Time.deltaTime; player.transform.position = Vector3.Lerp(startPos, wallDropEnd.position, timer / dropTime); yield return null; }
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
                    agent.speed = 12f; // Chạy nhanh hơn nữa để kịp lọt vào camera
                    agent.SetDestination(player.transform.position);
                }
                Animator anim = enemyObj.GetComponentInChildren<Animator>();
                if (anim != null) { anim.SetBool("isWalking", false); anim.SetBool("isRunning", true); }
            }
        }

        // ĐOẠN 3: CẮT XÍCH & QUAY ĐẦU (ĐÃ FIX NHỊP ĐIỆU)
        if (bikePosition != null)
        {
            float runTime = 1.2f; float timer = 0f; Vector3 startPos = player.transform.position;
            while (timer < runTime) { timer += Time.deltaTime; player.transform.position = Vector3.Lerp(startPos, bikePosition.position, timer / runTime); player.transform.LookAt(bikePosition); yield return null; }
        }

        if (cinematicAudio != null && cutSound != null) cinematicAudio.PlayOneShot(cutSound);
        if (bikeChainVisual != null) bikeChainVisual.SetActive(false);

        yield return new WaitForSeconds(0.2f); // Khựng lại vì giật mình

        // QUAY ĐẦU NHÌN ĐỐI THỦ
        if (enemyGateStartPos != null)
        {
            float turnTime = 0.3f; float turnTimer = 0f; Quaternion startRot = player.transform.rotation;
            Vector3 dir = (enemyGateStartPos.position - player.transform.position).normalized;
            dir.y = 0; Quaternion targetRot = Quaternion.LookRotation(dir);

            while (turnTimer < turnTime) { turnTimer += Time.deltaTime; player.transform.rotation = Quaternion.Slerp(startRot, targetRot, turnTimer / turnTime); yield return null; }

            // ĐÃ TĂNG: Đứng hình nhìn kẻ địch lâu hơn để thấy chúng đang lao tới
            yield return new WaitForSeconds(2.0f); // Tăng lên 2 giây cho kịch tính
        }

        if (bikeVisual != null) bikeVisual.SetActive(false);
        if (cinematicAudio != null && bikeRideSound != null) { cinematicAudio.Stop(); cinematicAudio.PlayOneShot(bikeRideSound); }

        // ĐÃ SỬA: Mờ đen chậm lại (2 giây) để tạo cảm giác thoát hiểm trong gang tấc
        yield return StartCoroutine(FadeScreen(1f, 2.0f));
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