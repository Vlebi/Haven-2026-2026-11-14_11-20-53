using UnityEngine;

public class BossHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 200;
    private int currentHealth;

    void Start()
    {

        currentHealth = maxHealth;
    }


    void TakeDamage(int damageAmount)
    {

    }

}
