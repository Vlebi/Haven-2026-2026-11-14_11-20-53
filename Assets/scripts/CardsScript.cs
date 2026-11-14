using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.U2D;

public class CardsScript : MonoBehaviour
{
    public HealthManager hm;
    public Attack atk;
    public EnemyMovement enemyMove;
    public Movement mvmnt;
    public score score;
    public void LifeSteal()
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
        atk.currentDamageBuff = (int)(atk.currentDamageBuff + 0.5);
        hm.maxHealth = hm.maxHealth + 20;
    }
    public void TierPlus()
    {
       
    }
    public void EnemySlow()
    {
        enemyMove.speedMultiplayer = 0.7f;
    }
    public void DmgBuff()
    {
        atk.currentDamageBuff = (int)(atk.currentDamageBuff + 0.5);
    }
    public void Amulet()
    {
        hm.shieldHealth += 50;
    }
    public void Explosion()
    {

    }
    public void SpeedBoost()
    {
        mvmnt.speed = mvmnt.speed * 1.2f;
    }
    public void ScoreMulti()
    {
        score.currentMult *= 2;
    }
    public void Scam()
    {
        atk.equippedWeaponIndex = 0; 
    }
    public void ScoreBonus()
    {
        score.getScore += 25;
    }
    public void MoreHP()
    {
        hm.maxHealth += 25;
    }
}