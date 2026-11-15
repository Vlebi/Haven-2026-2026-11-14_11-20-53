using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100;
    public float currentHealth;
    public Movement movement;
    public float shieldHealth;
    public float regenSpeed = 0.5f;
    public float regenMult = 1;
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    Canvas deathCanvas;
    void Start()
    {
        
        currentHealth = maxHealth;
        movement = FindFirstObjectByType<Movement>();
        canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        deathCanvas = GameObject.FindGameObjectWithTag("DeathCanvas").GetComponent<Canvas>();
    }

    
    public void TakeDamage(float damageAmount)
    {
        if (movement.isInvincible == true) return;
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
        canvas.enabled = false;
        deathCanvas.enabled = true;
        // Törli magát az objektumot a pályáról
        Destroy(gameObject);
        Invoke("LoadMainMenu", 16f);
    }
    void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
    // public IEnumerator Regen()
    //{
    //  currentHealth += regenSpeed * regenMult;
    // yield return new WaitForSecondsRealtime(1);
    //}
}
