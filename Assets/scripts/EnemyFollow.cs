using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public string playerTag = "Player";
    public string playerObjectName = "char1";
    public float stoppingDistance = 1.2f;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public bool flipSprite = true;
    public bool rotateToTarget = false; // top-down only

    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private void Awake()
    {
        Debug.Log("EnemyFollow started", this);
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f; // remove if you want gravity
    }

    private void FixedUpdate()
    {
        if (playerTransform == null && !FindPlayer())
        {
            Debug.LogWarning("EnemyFollow: player not found", this);
            return;
        }

        Vector2 direction = (Vector2)playerTransform.position - rb.position;

        if (direction.magnitude > stoppingDistance)
        {
            Vector2 next = Vector2.MoveTowards(
                rb.position, playerTransform.position, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(next);
        }

        if (flipSprite && spriteRenderer != null)
        {
            if (direction.x > 0.1f) spriteRenderer.flipX = false;
            else if (direction.x < -0.1f) spriteRenderer.flipX = true;
        }

        if (rotateToTarget && direction.sqrMagnitude > 0.0001f)
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
            try { player = GameObject.FindGameObjectWithTag(playerTag); }
            catch (UnityException) { Debug.LogWarning($"Tag '{playerTag}' is not defined.", this); }
        }

        if (player == null && !string.IsNullOrEmpty(playerObjectName))
            player = GameObject.Find(playerObjectName);

        playerTransform = player != null ? player.transform : null;
        return playerTransform != null;
    }
}