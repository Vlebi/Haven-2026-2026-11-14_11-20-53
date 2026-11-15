using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public string playerTag = "Player";
    public string playerObjectName = "char1";
    public float stoppingDistance = 1.2f;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public bool flipSprite = true;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (agent != null)
            agent.enabled = false;

        FindPlayer();
    }

    private void FixedUpdate()
    {
        if (playerTransform == null && !FindPlayer())
            return;

        Vector2 direction = (Vector2)playerTransform.position - rb.position;
        if (direction.magnitude > stoppingDistance)
        {
            Vector2 nextPosition = Vector2.MoveTowards(
                rb.position,
                playerTransform.position,
                moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(nextPosition);
        }

        if (flipSprite && spriteRenderer != null)
        {
            if (direction.x > 0.1f)
                spriteRenderer.flipX = false;
            else if (direction.x < -0.1f)
                spriteRenderer.flipX = true;
        }

        if (rb != null && direction.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle - 90f);
        }
    }

    private bool FindPlayer()
    {
        GameObject player = null;
        if (!string.IsNullOrEmpty(playerTag))
        {
            try
            {
                player = GameObject.FindGameObjectWithTag(playerTag);
            }
            catch (UnityException)
            {
                Debug.LogWarning($"EnemyFollow has an undefined player tag '{playerTag}'.", this);
            }
        }

        if (player == null && !string.IsNullOrEmpty(playerObjectName))
            player = GameObject.Find(playerObjectName);

        playerTransform = player != null ? player.transform : null;
        return playerTransform != null;
    }
}