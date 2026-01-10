using System;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ScoreText : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    int scoreTeam1;
    int scoreTeam2;

    public void Awake()
    {
        scoreText.text = "00";
    }
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
    private void UpdateText()
    {
        scoreText.text = $"blue {scoreTeam1} : {scoreTeam2} purple";
    }

}
