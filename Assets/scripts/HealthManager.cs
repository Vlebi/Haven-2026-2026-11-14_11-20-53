using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;
    public Movement movementcs;

    void Start()
    {
        
        currentHealth = maxHealth;
    }

    
    public void TakeDamage(int damageAmount)
    {
        if (movementcs.isInvincible == true) return;
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
}
