using UnityEngine;
using TMPro;
using System.Collections;

public class BlinkUI : MonoBehaviour
{
    private TextMeshProUGUI txt;
    public float blinkSpeed = 0.5f; // Tốc độ nhấp nháy

    void Awake()
    {
        txt = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            txt.enabled = !txt.enabled; // Bật tắt liên tục
            // Dùng WaitForSecondsRealtime vì lúc Game Over thời gian (TimeScale) đang bị đóng băng = 0
            yield return new WaitForSecondsRealtime(blinkSpeed);
        }
    }
}