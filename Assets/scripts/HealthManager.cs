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
    public float regenSpeed = 2f;
    public float regenMult = 1;
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    Canvas deathCanvas;
    private bool isDead;
    private WaitForSeconds _regenDelay = new WaitForSeconds(1f);
    void Start()
    {

        currentHealth = maxHealth;
        movement = FindFirstObjectByType<Movement>();
        foreach (Canvas foundCanvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (foundCanvas.CompareTag("Canvas"))
            {
                canvas = foundCanvas;
            }
            else if (foundCanvas.CompareTag("DeathCanvas"))
            {
                deathCanvas = foundCanvas;
            }
        }

        if (canvas != null) canvas.gameObject.SetActive(true);
        if (deathCanvas != null) deathCanvas.gameObject.SetActive(false);
        StartCoroutine(Regen());
    }
    private void LateUpdate()
    {
        if (!isDead)
            return;

        if (canvas != null) canvas.gameObject.SetActive(false);
        if (deathCanvas != null)
        {
            deathCanvas.transform.localScale = Vector3.one;
            deathCanvas.gameObject.SetActive(true);
        }
    }


    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        if (movement.isInvincible == true) return;
        if (shieldHealth > 0)
        {
            shieldHealth -= damageAmount;
        }
        else if (shieldHealth > 0 && shieldHealth < damageAmount)
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
        if (isDead) return;

        isDead = true;
        funfacter funfacter;

        funfacter = gameObject.GetComponent<funfacter>();
        funfacter.updateFunFact();
        Debug.Log($"{gameObject.name} died!");
        if (canvas != null) canvas.gameObject.SetActive(false);
        if (deathCanvas != null)
        {
            deathCanvas.transform.localScale = Vector3.one;
            deathCanvas.gameObject.SetActive(true);
        }
        // Törli magát az objektumot a pályáról
        if (movement != null) movement.enabled = false;
        Attack attack = GetComponent<Attack>();
        if (attack != null) attack.enabled = false;
        foreach (Collider2D playerCollider in GetComponentsInChildren<Collider2D>())
            playerCollider.enabled = false;
        foreach (SpriteRenderer playerRenderer in GetComponentsInChildren<SpriteRenderer>())
            playerRenderer.enabled = false;
        Invoke("LoadMainMenu", 16f);
    }
    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public IEnumerator Regen()
    {
        while (true)
        {
            if (!isDead && currentHealth < maxHealth)
            {
                currentHealth = Mathf.Min(currentHealth + (regenSpeed * regenMult), maxHealth);
            }
            yield return _regenDelay;
        }
    }
}
