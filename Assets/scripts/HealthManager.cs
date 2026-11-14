using System.Collections;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100;
    public float currentHealth;
    public Movement movementcs;
    public float shieldHealth;
    public float regenSpeed = 0.5f;
    public float regenMult;

    void Start()
    {
        
        currentHealth = maxHealth;
    }

    
    public void TakeDamage(float damageAmount)
    {
        if (movementcs.isInvincible == true) return;
        if(shieldHealth > 0)
        {
            shieldHealth -= damageAmount;
        }
        else if(shieldHealth > 0 && shieldHealth < damageAmount)
        {
            damageAmount -= shieldHealth;
            currentHealth -= shieldHealth;
        }
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} dealt {damageAmount} damage! Remaining HP: {currentHealth}");

        // Ha az életerő eléri a nullát (vagy az alá esik), meghal
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        // Törli magát az objektumot a pályáról
        Destroy(gameObject);
    }
    public IEnumerator Regen()
    {
        currentHealth += regenSpeed * regenMult;
        yield return new WaitForSecondsRealtime(1);
    }
}
