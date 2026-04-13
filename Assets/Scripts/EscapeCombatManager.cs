using UnityEngine;
using TMPro;

public class EscapeCombatManager : MonoBehaviour
{
    public static EscapeCombatManager Instance;

    [Header("Chỉ số Địch")]
    public float enemyHealth = 100f;
    public float attackDamage = 25f;
    public UnityEngine.UI.Slider enemyHealthBar;

    [Header("UI Giải toán")]
    public GameObject mathPanel;
    public TextMeshProUGUI questionText;
    public TMP_InputField answerField;

    private int currentAnswer;

    void Awake() => Instance = this;

    void Start()
    {
        GenerateQuestion();
    }

    public void GenerateQuestion()
    {
        int a = Random.Range(1, 10);
        int b = Random.Range(1, 10);
        currentAnswer = a + b;
        questionText.text = $"{a} + {b} = ?";
        answerField.text = "";
        answerField.ActivateInputField();
    }

    public void CheckAnswer()
    {
        if (int.Parse(answerField.text) == currentAnswer)
        {
            AttackEnemy();
            GenerateQuestion();
        }
    }

    void AttackEnemy()
    {
        enemyHealth -= attackDamage;
        enemyHealthBar.value = enemyHealth;

        // Hiệu ứng bắn súng/ném đồ
        Debug.Log("<color=green>TRÚNG ĐÍCH! Địch mất máu.</color>");

        if (enemyHealth <= 0)
        {
            Debug.Log("<color=cyan>CHIẾN THẮNG! Địch đã bị hạ gục.</color>");
            // Chuyển sang kết phim (Ending)
        }
    }
}