using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class CardsScript : MonoBehaviour
{
    public HealthManager hm;
    public Attack atk;
    public EnemyMovement enemyMove;
    public Movement mvmnt;
    public score score;
    public List<CardOption> allCards; // Fill this out in the Inspector
    public Button[] cardButtons;      // Your 3 UI buttons on the Canvas
    public TextMeshProUGUI[] cardTitles;
    public TextMeshProUGUI[] cardDescriptions;
    public void LifeSteal()
    {

    }
    public void PLS()
    {

    }
    public void Regen()
    {
        hm.regenMult += 1;
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
        atk.currentDamageBuff += 0.5f;
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
        atk.currentDamageBuff += 0.5f;
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
    public void ShowCardChoices()
    {
        Time.timeScale = 0f; // Pause game

        // Make a temporary copy of the list so we don't pick duplicates
        List<CardOption> pool = new List<CardOption>(allCards);

        for (int i = 0; i < cardButtons.Length; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            CardOption pickedCard = pool[randomIndex];
            pool.RemoveAt(randomIndex); // Remove so it can't be picked again this round

            // Update UI text
            cardTitles[i].text = pickedCard.cardName;
            cardDescriptions[i].text = pickedCard.description;

            // Clear old button clicks and assign the new card's effect
            cardButtons[i].onClick.RemoveAllListeners();
            cardButtons[i].onClick.AddListener(() => {
                pickedCard.onCardChosen.Invoke();
                CloseCardMenu();
            });
        }
    }

    void CloseCardMenu()
    {
        // Hide your UI panel here
        Time.timeScale = 1f; // Resume game
    }
}