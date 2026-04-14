using UnityEngine;
using TMPro;

public class MathGateSystem : MonoBehaviour
{
    public TextMeshPro questionText;
    public TextMeshPro[] laneTexts; // 3 text hiển thị đáp án trên 3 làn
    public GameObject[] deathTriggers; // Vực thẳm ở các làn sai

    private int correctAnswer;

    public void SetupGate(int a, int b, int correctLane)
    {
        correctAnswer = a + b;
        questionText.text = $"{a} + {b} = ?";

        for (int i = 0; i < 3; i++)
        {
            if (i == correctLane)
            {
                laneTexts[i].text = correctAnswer.ToString();
                deathTriggers[i].SetActive(false); // Làn đúng thì an toàn
            }
            else
            {
                laneTexts[i].text = (correctAnswer + Random.Range(-5, 5)).ToString();
                deathTriggers[i].SetActive(true); // Làn sai là vực thẳm
            }
        }
    }
}