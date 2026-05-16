using System.Collections;
using UnityEngine;

public class WakeUpManager : MonoBehaviour
{
    // Da xoa tieng Viet co dau de chong loi Unity
    public GameObject topEyelid;
    public GameObject bottomEyelid;
    public float wakeUpDuration = 3.0f;

    void Start()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.canWalk = false;
            player.canLook = false;
        }

        StartCoroutine(OpenEyesRoutine());
    }

    IEnumerator OpenEyesRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        float timer = 0f;
        Vector3 startScale = new Vector3(1, 1, 1);
        Vector3 endScale = new Vector3(1, 0, 1);

        while (timer < wakeUpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / wakeUpDuration;

            if (topEyelid != null) topEyelid.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            if (bottomEyelid != null) bottomEyelid.transform.localScale = Vector3.Lerp(startScale, endScale, progress);

            yield return null;
        }

        PlayerMovement player2 = FindFirstObjectByType<PlayerMovement>();
        if (player2 != null)
        {
            player2.canWalk = true;
            player2.canLook = true;
        }

        Debug.Log("Da tinh day!");
    }
}