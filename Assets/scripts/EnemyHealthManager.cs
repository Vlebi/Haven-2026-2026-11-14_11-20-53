using System.Collections;
using UnityEngine;

public class EnemyHealthManager : MonoBehaviour
{
    [SerializeField] private int hitsToKill = 5;
    [SerializeField] private float hitStunDuration = 1f;
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField, Range(0f, 1f)] private float weaponDropChance = 0.04f;

    public float hitsRemaining;
    private Rigidbody2D rb;
    private EnemyMovement enemyMovement;
    private EnemyFollow enemyFollow;
    private score score;
    private Coroutine hitReactionCoroutine;
    private bool movementWasEnabled;
    private bool followWasEnabled;
    private bool isDead;

    private void Awake()
    {
        hitsRemaining = Mathf.Max(1, hitsToKill);
        rb = GetComponent<Rigidbody2D>();
        enemyMovement = GetComponent<EnemyMovement>();
        enemyFollow = GetComponent<EnemyFollow>();
    }

    public void TakeSwordHit(int hitDamage = 1)
    {
        if (isDead)
            return;

        hitsRemaining -= Mathf.Max(1, hitDamage);
        if (hitsRemaining <= 0)
        {
            isDead = true;
            gameObject.SetActive(false);

            TryDropWeapon();

            score = FindFirstObjectByType<score>();
            if (score != null)
                score.currentscore += score.getScore * score.currentMult * score.scoreStreak;

            Destroy(gameObject);
            return;
        }

        StartHitReaction();
        Debug.Log(hitsRemaining + " hits remaining for " + gameObject.name);
    }

    private void TryDropWeapon()
    {
        if (Random.value >= weaponDropChance)
            return;

        Attack playerAttack = FindFirstObjectByType<Attack>();
        if (playerAttack == null || !playerAttack.TryGetRandomWeaponDrop(out int weaponIndex))
            return;

        GameObject pickupObject = new GameObject("WeaponPickup");
        pickupObject.transform.position = transform.position;
        interact_pickup pickup = pickupObject.AddComponent<interact_pickup>();
        pickup.Initialize(playerAttack, weaponIndex);
    }

    private void StartHitReaction()
    {
        if (hitReactionCoroutine != null)
        {
            StopCoroutine(hitReactionCoroutine);
        }
        else
        {
            movementWasEnabled = enemyMovement != null && enemyMovement.enabled;
            followWasEnabled = enemyFollow != null && enemyFollow.enabled;
        }

        if (enemyMovement != null)
            enemyMovement.enabled = false;
        if (enemyFollow != null)
            enemyFollow.enabled = false;

        if (rb != null)
        {
            GameObject player = GameObject.Find("char1");
            Vector2 knockbackDirection = player != null
                ? (Vector2)(transform.position - player.transform.position).normalized
                : Vector2.up;
            rb.linearVelocity = knockbackDirection * knockbackForce;
        }

        hitReactionCoroutine = StartCoroutine(RecoverFromHit());
    }

    private IEnumerator RecoverFromHit()
    {
        yield return new WaitForSeconds(hitStunDuration);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        if (enemyMovement != null && movementWasEnabled)
            enemyMovement.enabled = true;
        if (enemyFollow != null && followWasEnabled)
            enemyFollow.enabled = true;

        hitReactionCoroutine = null;
    }
}
