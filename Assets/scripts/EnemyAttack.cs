using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damageAmount = 10; // Mennyi HP-t vegyen le
    public float attackCooldown = 1f; // Hány másodpercenként tudjon sebezni, amíg hozzáér
    public string targetName = "char1"; // A játékos neve

    private float lastAttackTime = 0f;

    // Ez a beépített függvény folyamatosan fut, amíg a két fizikai test összeér
    void OnCollisionStay2D(Collision2D collision)
    {
        // Ellenőrizzük, hogy a játékoshoz értünk-e hozzá
        if (collision.gameObject.name == targetName)
        {
            // Ellenőrizzük, hogy eltelt-e elég idő az előző sebzés óta
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                // Megpróbáljuk lekérni a játékosról a HealthManager scriptet
                HealthManager playerHealth = collision.gameObject.GetComponent<HealthManager>();

                if (playerHealth != null)
                {
                    // Ha megvan, levonjuk a HP-t
                    playerHealth.TakeDamage(damageAmount);

                    // Frissítjük az utolsó támadás idejét
                    lastAttackTime = Time.time;
                }
            }
        }
    }
}