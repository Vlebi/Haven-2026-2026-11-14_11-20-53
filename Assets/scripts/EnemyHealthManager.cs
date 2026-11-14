using System.Collections;
using UnityEngine;

public class EnemyHealthManager : MonoBehaviour
{
    [SerializeField] private int hitsToKill = 5;
    [SerializeField] private float hitStunDuration = 1f;
    [SerializeField] private float knockbackForce = 3f;

    public int hitsRemaining;
    private Rigidbody2D rb;
    private EnemyMovement enemyMovement;
    private score score;
    private Coroutine hitReactionCoroutine;
    private bool movementWasEnabled;

   public EnemySpawn enemySpawn;
    private void Awake()
    {
        hitsRemaining = Mathf.Max(1, hitsToKill);
        rb = GetComponent<Rigidbody2D>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    public void TakeSwordHit(int hitDamage = 1)
    {
        hitsRemaining -= Mathf.Max(1, hitDamage);
        if (hitsRemaining <= 0)
        {

            score = FindFirstObjectByType<score>();
            score.currentscore =+ score.getScore * score.currentMult * score.scoreStreak;
            if (enemySpawn == null)
                enemySpawn = FindFirstObjectByType<EnemySpawn>();

            if (enemySpawn != null)
                enemySpawn.LowerSpawnedCount();
            else
                Debug.LogWarning("No EnemySpawn was found to update the spawned enemy count.", this);

            Destroy(gameObject);
            return;
        }

        StartHitReaction();
        Debug.Log(hitsRemaining + " hits remaining for " + gameObject.name);
    }

    private void StartHitReaction()
    {
        if (hitReactionCoroutine != null)
        {
            StopCoroutine(hitReactionCoroutine);
        }
        else if (enemyMovement != null)
        {
            movementWasEnabled = enemyMovement.enabled;
        }

        if (enemyMovement != null)
            enemyMovement.enabled = false;

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

        hitReactionCoroutine = null;
    }
}
