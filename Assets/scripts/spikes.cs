using UnityEngine;

public class spikes : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        HealthManager healthManager = other.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            healthManager.TakeDamage(damageAmount);
        }
    }
}