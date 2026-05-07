using UnityEngine;
using UnityEngine.UI;

public class BossHealthManager : MonoBehaviour
{
    public static BossHealthManager instance;
    public float maxHP = 100f;
    public float currentHP;
    public Slider hpSlider;

    void Awake() { instance = this; }

    void Start()
    {
        currentHP = maxHP;
        if (hpSlider) hpSlider.maxValue = maxHP;
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (hpSlider) hpSlider.value = currentHP;
        Debug.Log("<color=red>BOSS BỊ TRÚNG ĐÒN!</color>");

        if (currentHP <= 0) WinGame();
    }

    void WinGame()
    {
        Debug.Log("<color=cyan>BỌN TRUY ĐUỔI ĐÃ BỊ CẮT ĐUÔI! CHIẾN THẮNG!</color>");
        // Gọi kịch bản Ending tại đây
    }
}