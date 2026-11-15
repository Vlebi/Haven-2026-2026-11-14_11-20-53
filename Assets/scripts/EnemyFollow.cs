using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public string playerTag = "Player";
    public float stoppingDistance = 1.2f;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public bool flipSprite = true;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
            }
            return;
        }

        agent.SetDestination(playerTransform.position);

        if (flipSprite && spriteRenderer != null)
        {
            if (agent.velocity.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (agent.velocity.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }
}