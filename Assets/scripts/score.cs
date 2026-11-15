using System.Collections;
using TMPro;
using UnityEngine;

public class score : MonoBehaviour
{
    public float timePassedBetweenDash;
    public EnemyHealthManager EHM;
    public float currentscore = 0;
    public float currentMult = 1;
    public float getScore = 100;
    public float scoreStreak = 1;
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        scoreText = RuntimeHudText.GetOrCreate("ScoreText", "Score: 0", new Vector2(20f, -20f));
        UpdateScoreText();
    }

    void Update()
    {
        if (scoreStreak > 5)
        {
            scoreStreak = 4;
        }
        timePassedBetweenDash += Time.deltaTime;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = 
    }
    public void scoreStreakSystem()
    {
        if (EHM.hitsRemaining <= 0 && timePassedBetweenDash <= 1.5f)
        {
            
            scoreStreak++;
            timePassedBetweenDash = 0;
        }
        else
        {
            scoreStreak = 1;
            Debug.Log("Score Reset");
        }
    }
}
