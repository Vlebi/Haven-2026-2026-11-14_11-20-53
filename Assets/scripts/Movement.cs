using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
//balls, greasy ones in fact
public class Movement : MonoBehaviour
{
    public score score;
    [Header("Movement & Combat")]
    public float speed = 5f;
    public float baseDamage = 10f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float bounceForce = 12f;
    [SerializeField] private float bounceDuration = 0.15f;
    [SerializeField] private float stunDuration = 3f;
    public bool isInvincible = false;

    [Header("Combo")]
    [SerializeField] private float speedBonusPerHit = 2f;
    [SerializeField] private float damageBonusPerHit = 5f;

    private Rigidbody2D rb;
    private Camera cam;
    private Vector2 movement;
    private float horizontal;
    private float vertical;

    private bool canDash = true;
    private bool isDashing;
    private bool isStunned;
    private bool hitEnemyThisDash;
    private int comboStacks = 0;

    private Vector2 lastDashDirection;
    private Coroutine activeDashCoroutine;

    public float CurrentDSpeed => dashSpeed + (comboStacks * speedBonusPerHit);
    public float CurrentDamage => baseDamage + (comboStacks * damageBonusPerHit);

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDashing || isStunned) return;

        if (Input.GetMouseButtonDown(0) && canDash)
        {
            activeDashCoroutine = StartCoroutine(DashTowardsMouse());
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        if (score.timePassedBetweenDash >= 1.5f)
        {
            dashSpeed = 10;
        }
    }
    

    void FixedUpdate()
    {
        if (isDashing || isStunned) return;

        movement = new Vector2(horizontal, vertical).normalized;
        rb.linearVelocity = movement * speed;
    }

    private IEnumerator DashTowardsMouse()
    {
        isInvincible = true;
        canDash = false;
        isDashing = true;
        hitEnemyThisDash = false;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        lastDashDirection = (mouseWorldPos - transform.position).normalized;

        rb.linearVelocity = lastDashDirection * CurrentDSpeed;

        yield return new WaitForSeconds(dashDuration);

        if (!hitEnemyThisDash)
        {
            rb.linearVelocity = Vector2.zero;
            isDashing = false;
            isInvincible = false;
            comboStacks = 0;
            StartCoroutine(StunPlayer());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing && !hitEnemyThisDash && collision.gameObject.CompareTag("Enemy"))
        {
            hitEnemyThisDash = true;
            score.timePassedBetweenDash = 0;

            if (activeDashCoroutine != null)
            {
                StopCoroutine(activeDashCoroutine);
            }

            comboStacks++;
            StartCoroutine(BounceBack());
        }
    }

    private IEnumerator BounceBack()
    {
        rb.linearVelocity = -lastDashDirection * bounceForce;

        yield return new WaitForSeconds(bounceDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        canDash = true;
        isInvincible = false;
    }

    private IEnumerator StunPlayer()
    {
        comboStacks = 0;
        isStunned = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        canDash = true;
        isInvincible = false;
    }
}