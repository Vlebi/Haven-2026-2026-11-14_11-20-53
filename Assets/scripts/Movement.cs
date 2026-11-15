using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public score score;
    public EnemyHealthManager EHM;

    [Header("Movement & Combat")]
    public float speed = 5f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 70f;
    public float baseDamage = 10f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float maxDashSpeed = 40f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float bounceForce = 25f;
    [SerializeField] private float bounceDuration = 0.3f;
    [SerializeField] private float stunDuration = 1.5f;
    [SerializeField] private float dashBufferTime = 0.15f;
    [SerializeField] private GameObject stunMarker;
    public bool isInvincible = false;

    [Header("Combo")]
    [SerializeField] private float speedBonusPerHit = 2f;
    [SerializeField] private float damageBonusPerHit = 5f;

    private Rigidbody2D rb;
    private Camera cam;
    private Vector2 moveInput;

    private bool canDash = true;
    private bool isDashing;
    private bool isStunned;
    private bool hitEnemyThisDash;
    private float dashBufferCounter;

    private int comboStacks = 0;
    private Vector2 lastDashDirection;
    private Coroutine activeDashCoroutine;
    private float currentDashSpeed;

    public float CurrentDamage => baseDamage + (comboStacks * damageBonusPerHit);
    public int ComboDamageBonus => Mathf.RoundToInt(comboStacks * damageBonusPerHit);
    public bool IsDashing => isDashing;

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        // Ensures smooth movement between physics frames
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (score == null)
        {
            score = FindFirstObjectByType<score>();
        }

        if (stunMarker != null)
            stunMarker.SetActive(false);
    }

    void Update()
    {
        // Always capture movement input so there's no dead frame after a dash/stun
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Buffer the dash click so clicking slightly early still triggers it
        if (Input.GetMouseButtonDown(0))
        {
            dashBufferCounter = dashBufferTime;
        }
        else
        {
            dashBufferCounter -= Time.deltaTime;
        }

        if (isDashing || isStunned) return;

        if (dashBufferCounter > 0f && canDash)
        {
            dashBufferCounter = 0f;
            activeDashCoroutine = StartCoroutine(DashTowardsMouse());
        }
    }

    void FixedUpdate()
    {
        if (isDashing || isStunned) return;

        // Smooth acceleration and deceleration instead of instant velocity snapping
        Vector2 targetVelocity = moveInput * speed;
        float accelRate = (moveInput.magnitude > 0.01f) ? acceleration : deceleration;

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, accelRate * Time.fixedDeltaTime);
    }

    private void CalculateDashSpeed()
    {
        if (score != null && score.timePassedBetweenDash <= 1.5f)
        {
            // Fixed: Uses speedBonusPerHit instead of damageBonusPerHit, and caps at maxDashSpeed
            currentDashSpeed = Mathf.Min(dashSpeed + (comboStacks * speedBonusPerHit), maxDashSpeed);
        }
        else
        {
            comboStacks = 0; // Reset combo if too much time passed
            currentDashSpeed = dashSpeed;
        }
    }

    private IEnumerator DashTowardsMouse()
    {
        isInvincible = true;
        canDash = false;
        isDashing = true;
        hitEnemyThisDash = false;

        CalculateDashSpeed();

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        lastDashDirection = (mouseWorldPos - transform.position).normalized;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            // Slight ease-out curve during the dash feels much punchier than flat speed
            float speedMultiplier = Mathf.Lerp(1.2f, 0.8f, elapsed / dashDuration);
            rb.linearVelocity = lastDashDirection * (currentDashSpeed * speedMultiplier);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (!hitEnemyThisDash)
        {
            isDashing = false;
            isInvincible = false;
            comboStacks = 0;
            StartCoroutine(StunPlayer());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponentInParent<EnemyHealthManager>() == null)
            return;

        foreach (Collider2D playerCollider in GetComponentsInChildren<Collider2D>())
        {
            if (playerCollider.enabled && !playerCollider.isTrigger && !collision.collider.isTrigger)
                Physics2D.IgnoreCollision(playerCollider, collision.collider, true);
        }

        if (isDashing && !hitEnemyThisDash)
            RegisterDashHit();
    }

    public void RegisterDashHit()
    {
        if (!isDashing || hitEnemyThisDash)
            return;

        hitEnemyThisDash = true;
        if (score != null)
            score.timePassedBetweenDash = 0;
        if (activeDashCoroutine != null)
        {
            StopCoroutine(activeDashCoroutine);
            activeDashCoroutine = null;
        }

        comboStacks++;
        StartCoroutine(BounceBack());
    }

    private IEnumerator BounceBack()
    {
        float currentBounceForce = bounceForce;
        float elapsed = 0f;
        while (elapsed < bounceDuration)
        {
            // Smoothly decay the bounce velocity instead of abruptly stopping at zero
            float currentBounce = Mathf.Lerp(currentBounceForce, 0f, elapsed / bounceDuration);
            rb.linearVelocity = -lastDashDirection * currentBounce;

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
        canDash = true;
        isInvincible = false;
    }

    private IEnumerator StunPlayer()
    {
        comboStacks = 0;
        isStunned = true;
        if (stunMarker != null)
            stunMarker.SetActive(true);

        // Slide to a stop briefly rather than hitting an invisible brick wall
        while (rb.linearVelocity.magnitude > 0.1f)
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        if (stunMarker != null)
            stunMarker.SetActive(false);
        canDash = true;
        isInvincible = false;
    }
}