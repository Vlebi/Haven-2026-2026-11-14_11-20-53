using System.Collections.Generic;
using UnityEngine;

public class spikes : MonoBehaviour
{
    [SerializeField, Min(0)] private int damageAmount = 10;
    [SerializeField, Min(0.1f)] private float damageCooldown = 1f;
    private readonly Dictionary<HealthManager, float> nextDamageTimes = new Dictionary<HealthManager, float>();

    private void OnTriggerEnter2D(Collider2D other) => TryDamagePlayer(other);
    private void OnTriggerStay2D(Collider2D other) => TryDamagePlayer(other);

    private void OnCollisionEnter2D(Collision2D collision) => TryDamagePlayer(collision.collider);
    private void OnCollisionStay2D(Collision2D collision) => TryDamagePlayer(collision.collider);

    private void TryDamagePlayer(Collider2D other)
    {
        HealthManager healthManager = other.GetComponentInParent<HealthManager>();
        if (healthManager == null || !healthManager.CompareTag("Player") || damageAmount <= 0)
            return;

        if (nextDamageTimes.TryGetValue(healthManager, out float nextDamageTime) && Time.time < nextDamageTime)
            return;

        nextDamageTimes[healthManager] = Time.time + damageCooldown;
        healthManager.TakeDamage(damageAmount);
    }
}
