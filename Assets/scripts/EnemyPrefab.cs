using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Mozgás Beállítások")]
    public float speed = 0.7f;
    public float speedMultiplayer = 1f;
    public float currentspeed => speed * speedMultiplayer;
    public string targetName = "char1";

    [Header("Forgás Beállítások")]
    public float rotationOffset = -90f;

    private Transform playerTransform;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.Find(targetName);

        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void FixedUpdate()
    {
        if (playerTransform != null)
        {
            // Lenullázzuk a sodródást
            rb.linearVelocity = Vector2.zero;

            // Fizikai alapú mozgás (nem teleportál bele a játékosba)
            Vector2 newPosition = Vector2.MoveTowards(rb.position, playerTransform.position, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            // Kard hegyének forgatása a játékos felé
            Vector2 direction = playerTransform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle + rotationOffset);
        }
    }
}