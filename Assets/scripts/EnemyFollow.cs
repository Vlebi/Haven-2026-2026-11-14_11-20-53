using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public string playerTag = "Player";
    public string playerObjectName = "char1";
    public float stoppingDistance = 1.2f;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public bool useNavMeshPathfinding;
    public bool flipSprite = true;
    public bool rotateToTarget = false; // top-down

    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private NavMeshAgent navMeshAgent;
    private Vector3 lastDestination;
    private bool destinationSet;
    private bool warnedAboutNavMesh;
    private bool warnedAboutInvalidPath;
    private bool warnedAboutMissingPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb.gravityScale = 0f;

        if (!useNavMeshPathfinding)
            return;

        if (navMeshAgent == null)
        {
            Debug.LogError("EnemyFollow needs a NavMeshAgent when NavMesh pathfinding is enabled.", this);
            return;
        }

        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.updatePosition = false;
        navMeshAgent.baseOffset = 0f;
        navMeshAgent.speed = Mathf.Max(0f, moveSpeed);
        navMeshAgent.stoppingDistance = Mathf.Max(0f, stoppingDistance);
    }

    private void Update()
    {
        if (!useNavMeshPathfinding || navMeshAgent == null)
            return;

        if (playerTransform == null && !FindPlayer())
            return;

        if (!TryPlaceAgentOnNavMesh())
            return;

        Vector3 destination = playerTransform.position;
        destination.z = transform.position.z;
        if (destinationSet && (destination - lastDestination).sqrMagnitude < 0.01f)
            return;

        if (!navMeshAgent.SetDestination(destination))
        {
            Debug.LogWarning("EnemyFollow could not set a NavMesh destination for the player.", this);
            return;
        }

        lastDestination = destination;
        destinationSet = true;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null && !FindPlayer())
            return;

        Vector2 direction = (Vector2)playerTransform.position - rb.position;

        if (useNavMeshPathfinding)
        {
            if (navMeshAgent == null || !TryPlaceAgentOnNavMesh())
                return;

            navMeshAgent.nextPosition = rb.position;
            if (destinationSet && !navMeshAgent.pathPending
                && navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                if (!warnedAboutInvalidPath)
                {
                    Debug.LogWarning("EnemyFollow has no valid NavMesh path to the player.", this);
                    warnedAboutInvalidPath = true;
                }
            }
            else if (!navMeshAgent.pathPending && navMeshAgent.hasPath)
            {
                warnedAboutInvalidPath = false;
                Vector3 desiredVelocity = navMeshAgent.desiredVelocity;
                Vector2 planarVelocity = Vector2.ClampMagnitude(
                    new Vector2(desiredVelocity.x, desiredVelocity.y), Mathf.Max(0f, moveSpeed));
                rb.MovePosition(rb.position + planarVelocity * Time.fixedDeltaTime);
            }
        }
        else if (direction.magnitude > stoppingDistance)
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

    private bool TryPlaceAgentOnNavMesh()
    {
        if (navMeshAgent.isOnNavMesh)
        {
            warnedAboutNavMesh = false;
            return true;
        }

        if (NavMesh.SamplePosition(rb.position, out NavMeshHit hit, 0.5f, navMeshAgent.areaMask)
            && navMeshAgent.Warp(hit.position))
        {
            rb.position = hit.position;
            warnedAboutNavMesh = false;
            return true;
        }

        if (!warnedAboutNavMesh)
        {
            Debug.LogWarning("EnemyFollow could not place the enemy on the NavMesh.", this);
            warnedAboutNavMesh = true;
        }

        return false;
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
        if (playerTransform != null)
        {
            warnedAboutMissingPlayer = false;
            return true;
        }

        if (!warnedAboutMissingPlayer)
        {
            Debug.LogWarning("EnemyFollow: player not found", this);
            warnedAboutMissingPlayer = true;
        }

        return false;
    }
}
