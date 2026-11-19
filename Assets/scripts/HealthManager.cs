using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100;
    public float currentHealth;
    [SerializeField, Range(0f, 1f)] private float lowHealthColorPreservation = 0f;
    [SerializeField, Range(0f, 1f)] private float noColorHealthThreshold = 0.1f;
    [SerializeField] private Material graphiteMaterial;
    [SerializeField] private Movement movement;
    private float originalColorPreservation;
    public float shieldHealth;
    [SerializeField]
    Canvas canvas;
    [SerializeField]
    Canvas deathCanvas;
    [SerializeField] private GameObject hitMarker;
    [SerializeField, Min(0f)] private float hitMarkerDuration = 0.2f;
    private bool isDead;
    private Coroutine hitMarkerCoroutine;
    private MusicManager musicManager;
    private SpriteRenderer[] playerRenderers;
    private Color[] baseRendererColors;
    private int lastLoggedHealthBand = -1;
    private bool loggedMissingMusicManager;
    private bool isPlayer;

    void Start()
    {
        isPlayer = CompareTag("Player");
        currentHealth = maxHealth;
        musicManager = MusicManager.Instance;
        playerRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        baseRendererColors = new Color[playerRenderers.Length];
        for (int i = 0; i < playerRenderers.Length; i++)
            baseRendererColors[i] = playerRenderers[i].color;
        if (graphiteMaterial != null)
            originalColorPreservation = graphiteMaterial.GetFloat("_ColorPreservation");
        UpdateGraphiteColorPreservation();
        Debug.Log($"HealthManager '{gameObject.name}' started. HP: {currentHealth}/{maxHealth}, MusicManager: {(musicManager != null ? musicManager.name : "NOT FOUND")}, GraphiteMaterial: {(graphiteMaterial != null ? graphiteMaterial.name : "NOT ASSIGNED")}", this);

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
        if (hitMarker != null) hitMarker.SetActive(false);
    }

    private void Update()
    {
        UpdateGraphiteColorPreservation();
        if (!isPlayer)
            return;

        if (musicManager == null)
            musicManager = MusicManager.Instance;
        float healthPercent = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
        int healthBand = Mathf.Clamp(Mathf.FloorToInt(healthPercent * 10f), 0, 10);
        if (healthBand != lastLoggedHealthBand)
        {
            lastLoggedHealthBand = healthBand;
            Debug.Log($"HealthManager '{gameObject.name}' health: {healthPercent:P0} ({currentHealth:F1}/{maxHealth:F1}), MusicManager: {(musicManager != null ? musicManager.name : "NOT FOUND")}", this);
        }
        if (musicManager != null)
            musicManager.ApplyHealthMood(healthPercent);
        else if (!loggedMissingMusicManager)
        {
            loggedMissingMusicManager = true;
            Debug.LogWarning($"HealthManager '{gameObject.name}' cannot apply music mood: MusicManager.Instance is null.", this);
        }
        UpdateLowHealthPulse(healthPercent);
    }

    private void UpdateLowHealthPulse(float healthPercent)
    {
        float danger = 1f - healthPercent;
        float pulse = 1f - (danger * 0.35f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f)));
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            SpriteRenderer renderer = playerRenderers[i];
            if (renderer != null && renderer.enabled)
            {
                Color baseColor = baseRendererColors[i];
                renderer.color = new Color(
                    baseColor.r * pulse,
                    baseColor.g * pulse,
                    baseColor.b * pulse,
                    baseColor.a);
            }
        }
    }

    private void UpdateGraphiteColorPreservation()
    {
        float healthPercent = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
        float healthColorRange = Mathf.Max(0.001f, 1f - noColorHealthThreshold);
        float colorPreservationFactor = Mathf.Clamp01((healthPercent - noColorHealthThreshold) / healthColorRange);
        float preservation = Mathf.Lerp(lowHealthColorPreservation, 1f, colorPreservationFactor);
        if (graphiteMaterial != null)
            graphiteMaterial.SetFloat("_ColorPreservation", preservation);
    }

    private void OnDestroy()
    {
        if (graphiteMaterial != null)
            graphiteMaterial.SetFloat("_ColorPreservation", originalColorPreservation);
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
        if (movement != null && movement.isInvincible) return;

        damageAmount = Mathf.Max(0f, damageAmount);
        if (damageAmount <= 0f) return;

        ShowHitMarker();
        float absorbedDamage = Mathf.Min(shieldHealth, damageAmount);
        if (absorbedDamage > 0f)
        {
            shieldHealth = Mathf.Max(0f, shieldHealth - absorbedDamage);
            damageAmount -= absorbedDamage;
        }
        if (damageAmount > 0f)
            currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"HealthManager '{gameObject.name}' took {damageAmount} damage. Remaining HP: {currentHealth}/{maxHealth} ({(maxHealth > 0f ? currentHealth / maxHealth : 0f):P0}).", this);

        // Ha az életerő eléri a nullát (vagy az alá esik), meghal
        if (currentHealth <= 0)
        {
            Die();

        }
    }

    public void HealPercent(float percentOfMaxHealth)
    {
        if (isDead || percentOfMaxHealth <= 0f)
            return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + (maxHealth * percentOfMaxHealth), 0f, maxHealth);
        Debug.Log($"HealthManager '{gameObject.name}' healed {percentOfMaxHealth:P0}. HP: {previousHealth:F1} -> {currentHealth:F1}/{maxHealth:F1}.", this);
    }

    private void ShowHitMarker()
    {
        if (hitMarker == null)
            return;

        hitMarker.SetActive(true);
        if (hitMarkerCoroutine != null)
            StopCoroutine(hitMarkerCoroutine);
        hitMarkerCoroutine = StartCoroutine(HideHitMarkerAfterDelay());
    }

    private IEnumerator HideHitMarkerAfterDelay()
    {
        yield return new WaitForSeconds(hitMarkerDuration);
        if (hitMarker != null)
            hitMarker.SetActive(false);
        hitMarkerCoroutine = null;
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
}
