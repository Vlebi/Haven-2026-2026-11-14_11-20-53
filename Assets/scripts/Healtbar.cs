using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private HealthManager health;
    [SerializeField] private Slider slider;

    private void Start()
    {
        if (health == null)
            health = GameObject.Find("char1").GetComponent<HealthManager>();

        if (slider == null)
            slider = GetComponent<Slider>();

        slider.value = health.currentHealth / health.maxHealth;
    }

    private void Update()
    {
        if (health != null && health.maxHealth > 0)
            slider.value = Mathf.Clamp01(health.currentHealth / health.maxHealth);
    }
}