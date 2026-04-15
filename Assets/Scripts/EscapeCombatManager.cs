using UnityEngine;
using TMPro;

public class EscapeCombatManager : MonoBehaviour
{
    public static EscapeCombatManager Instance;

    [Header("Chỉ số Địch")]
    public float enemyHealth = 100f;
    public float attackDamage = 20f;
    public UnityEngine.UI.Slider enemyHealthBar;

    [Header("Vật phẩm ném")]
    public int brickCount = 0; // Số lượng đồ đang có
    public TextMeshProUGUI inventoryUI;
    public GameObject projectilePrefab; // Prefab viên gạch/chai lọ
    public Transform throwPoint; // Vị trí ném (trên xe đạp)

    [Header("UI Giải toán")]
    public TextMeshProUGUI questionText;
    public TMP_InputField answerField;
    private int currentAnswer;

    void Awake() => Instance = this;

    void Start() { GenerateQuestion(); }

    void Update()
    {
        // Nhấn Space để ném đồ nếu có gạch
        if (Input.GetKeyDown(KeyCode.Space) && brickCount > 0)
        {
            ThrowObject();
        }
    }

    public void GenerateQuestion()
    {
        int a = Random.Range(10, 50);
        int b = Random.Range(10, 50);
        currentAnswer = a + b;
        questionText.text = $"{a} + {b} = ?";
        answerField.text = "";
        answerField.ActivateInputField();
    }

    public void CheckAnswer()
    {
        if (int.Parse(answerField.text) == currentAnswer)
        {
            brickCount++; // Giải đúng nhận thêm 1 món đồ
            UpdateUI();
            GenerateQuestion();
        }
    }

    void ThrowObject()
    {
        brickCount--;
        UpdateUI();

        // Spawn vật thể ném về phía sau
        GameObject projectile = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        // Code cho vật thể bay ngược lại và trừ máu địch khi va chạm

        AttackEnemy();
    }

    void AttackEnemy()
    {
        enemyHealth -= attackDamage;
        enemyHealthBar.value = enemyHealth;
        if (enemyHealth <= 0) WinByCombat();
    }

    void UpdateUI() { if (inventoryUI) inventoryUI.text = "Đồ ném: x" + brickCount; }
    void WinByCombat() { Debug.Log("ĐỊCH ĐÃ TÉ XE! BẠN ĐÃ THOÁT."); }
}