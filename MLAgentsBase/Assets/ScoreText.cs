using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    int scoreTeam1;
    int scoreTeam2;

    public void AddTeam1Score()
    {
        scoreTeam1++;
        UpdateText();
    }

    public void AddTeam2Score()
    {
        scoreTeam2++;
        UpdateText();
    }

    public void UpdateText()
    {
        scoreText.text = $"blue {scoreTeam1} : {scoreTeam2} purple";
    }
}
