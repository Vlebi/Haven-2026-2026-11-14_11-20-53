using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damageAmount = 5;
    public float attackCooldown = 1f;
    public string targetName = "char1";

    private float lastAttackTime = 0f;

    // OnTriggerStay2D akkor fut le, ha egy "Is Trigger" collider belép a játékosba
    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("trigger stay");
        // Megnézzük, hogy maga az objektum vagy a szülője a "char1"-e
        if (other.gameObject.name == targetName || (other.transform.parent != null && other.transform.parent.name == targetName))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                // Megkeressük a HealthManager-t az objektumon vagy a szülőjén
                HealthManager playerHealth = other.GetComponentInParent<HealthManager>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);
                    lastAttackTime = Time.time;
                }
                else
                {
                    Debug.LogWarning("I found the char1, but there's no HealthManager script on it!");
                }
            }
        }
    }
}