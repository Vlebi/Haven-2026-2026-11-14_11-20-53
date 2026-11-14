using System.Collections;
using UnityEngine;

public class score : MonoBehaviour
{
    public float timePassedBetweenDash;
    public EnemyHealthManager EHM;
    public float currentscore = 0;
    public float currentMult = 1;
    public float getScore = 100;
    public float scoreStreak = 1;

    void Update()
    {
        if (scoreStreak > 5)
        {
            scoreStreak = 4;
        }
    }
    public void scoreStreakSystem()
    {
        timePassedBetweenDash = Time.time;
        if (EHM.HitsRemainging <= 0 && timePassedBetweenDash >= 1.5f)
        {
            scoreStreak++;
            Debug.Log("Streak =" + scoreStreak);
            timePassedBetweenDash = 0;
        }
        else
        {
            scoreStreak = 1;
            Debug.Log("Score Reset");
        }
    }
}
