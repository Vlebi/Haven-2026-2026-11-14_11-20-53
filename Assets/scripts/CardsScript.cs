using UnityEngine;

public class CardsScript : MonoBehaviour
{
    public HealthManager hm;
    public void Lifesteal()
    {

    }
    public void PLS()
    {

    }
    public void Regen()
    {

    }
    public void HalfHealthBack()
    {
        if(hm.currentHealth <= 10)
        {
            hm.currentHealth = hm.maxHealth / 2;
        }
    }
    public void Jackpot()
    {

    }
    public void TierPlus()
    {

    }
    public void EnemySlow()
    {

    }
    public void DmgBuff()
    {

    }
    public void DRedirection()
    {

    }
    public void Amulet()
    {

    }
    public void Explosion()
    {

    }
    public void SpeedBoost()
    {

    }
    public void ScoreMulti()
    {

    }
    public void Scam()
    {

    }
    public void ScoreBonus()
    {

    }
    public void MoreHP()
    {

    }
}
