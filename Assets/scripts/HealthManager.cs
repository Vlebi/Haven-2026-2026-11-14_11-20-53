using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        // Játék indulásakor az aktuális életerő a maximumon van
        currentHealth = maxHealth;
    }

    // Ezt a függvényt fogja meghívni a barátod kódja (pl. a lövedék vagy a kard)
    public void TakeDamage(int damageAmount)
    {
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
