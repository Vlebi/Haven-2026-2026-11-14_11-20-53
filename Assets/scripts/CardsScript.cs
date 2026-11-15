using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsScript : MonoBehaviour
{
    [Header("Game Script References")]
    public HealthManager hm;
    public Attack atk;
    public EnemyMovement enemyMove;
    public Movement mvmnt;
    public score score;

    [Header("Card UI Setup")]
    public GameObject cardPanel;          // Drag your CardSelectionPanel here
    public List<Button> allCardButtons;   // Drag ALL your pre-made card buttons here
    public int cardsToShow = 2;           // Number of cards to display (set to 2)

    // --- CARD EFFECTS (Link these to your buttons' OnClick events) ---

    public void LifeSteal() { }
    public void PLS() { }
    public void TierPlus() { }
    public void Explosion() { }

    public void Regen()
    {
        hm.regenMult += 1;
    }

    public void HalfHealthBack()
    {
        if (hm.currentHealth <= 10)
        {
            hm.currentHealth = hm.maxHealth / 2;
        }
    }

    public void Jackpot()
    {
        atk.currentDamageBuff += 0.5f;
        hm.maxHealth = hm.maxHealth + 20;
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

    // --- MENU & SELECTION LOGIC ---

    public void ShowCardChoices()
    {
        cardPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game

        // 1. Hide every button first
        foreach (Button btn in allCardButtons)
        {
            btn.gameObject.SetActive(false);
        }

        // 2. Make a temporary pool so we don't pick duplicates
        List<Button> pool = new List<Button>(allCardButtons);

        // 3. Pick random buttons, turn them on, and randomize their order
        for (int i = 0; i < cardsToShow; i++)
        {
            if (pool.Count == 0) break;

            int randomIndex = Random.Range(0, pool.Count);
            Button pickedButton = pool[randomIndex];
            pool.RemoveAt(randomIndex);

            pickedButton.gameObject.SetActive(true);
            pickedButton.transform.SetAsLastSibling(); // Randomizes left/right position
        }
    }

    public void CloseCardMenu()
    {
        cardPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }
}
