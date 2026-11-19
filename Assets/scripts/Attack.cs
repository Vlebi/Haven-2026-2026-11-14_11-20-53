using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Attack : MonoBehaviour
{
    [System.Serializable]
    public class WeaponDefinition
    {
        public string weaponName;
        public TileBase tile;
        [Min(0)] public int tier = 1;
        [Min(1)] public float damageInHits = 1;
        public Vector2 offset = new Vector2(0.45f, 0f);
        public Vector3 scale = Vector3.one;
        public float spriteAngleOffset;
    }

    public float currentDamageBuff = 0;

    [SerializeField] public List<WeaponDefinition> weapons = new List<WeaponDefinition>();
    [SerializeField] private int startingWeaponIndex;
    [SerializeField, Min(0f)] private float behindEnemySearchDistance = 1.25f;
    [SerializeField, Min(0f)] private float behindEnemySideTolerance = 0.4f;
    [SerializeField, Min(0f)] private float hammerSplashRadius = 0.75f;
    [SerializeField] private LayerMask wallCollisionMask = ~0;
    [SerializeField, Min(0f)] private float wallClearance = 0.01f;

    private Camera cam;
    private Transform aimPivot;
    private Transform weaponTransform;
    private SpriteRenderer weaponRenderer;
    private BoxCollider2D weaponHitbox;
    private ContactFilter2D wallContactFilter;
    private readonly List<RaycastHit2D> wallCastHits = new List<RaycastHit2D>();
    private bool weaponBlockedByWall;
    public int equippedWeaponIndex = -1;

    public WeaponDefinition EquippedWeapon =>
        equippedWeaponIndex >= 0 && equippedWeaponIndex < weapons.Count
            ? weapons[equippedWeaponIndex]
            : null;

    private void Start()
    {
        cam = Camera.main;
        wallContactFilter.SetLayerMask(wallCollisionMask);
        wallContactFilter.useTriggers = false;
        CreateAimVisuals();
        EquipWeapon(startingWeaponIndex);
    }

    private void Update()
    {
        if (aimPivot == null)
            return;

        if (cam == null)
            cam = Camera.main;
        if (cam == null)
            return;

        Vector3 mouseScreenPosition = Input.mousePosition;
        float depth = Mathf.Abs(cam.transform.position.z - transform.position.z);
        if (float.IsNaN(mouseScreenPosition.x) || float.IsInfinity(mouseScreenPosition.x) ||
            float.IsNaN(mouseScreenPosition.y) || float.IsInfinity(mouseScreenPosition.y) ||
            float.IsNaN(depth) || float.IsInfinity(depth))
        {
            return;
        }

        mouseScreenPosition.z = depth;
        Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseScreenPosition);
        if (float.IsNaN(mouseWorldPosition.x) || float.IsInfinity(mouseWorldPosition.x) ||
            float.IsNaN(mouseWorldPosition.y) || float.IsInfinity(mouseWorldPosition.y))
        {
            return;
        }

        Vector2 direction = (Vector2)mouseWorldPosition - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        WeaponDefinition weapon = EquippedWeapon;
        if (weapon != null && weaponTransform != null && weaponHitbox != null)
        {
            weaponTransform.localPosition = weapon.offset;
            weaponHitbox.enabled = true;
            weaponBlockedByWall = ClampWeaponToWall();
            weaponHitbox.enabled = !weaponBlockedByWall;
            Physics2D.SyncTransforms();
        }
    }

    private bool ClampWeaponToWall()
    {
        Vector2 origin = transform.position;
        Vector2 desiredPosition = weaponTransform.position;
        Vector2 toWeapon = desiredPosition - origin;
        float weaponCenterDistance = toWeapon.magnitude;
        if (weaponCenterDistance <= 0.001f)
            return false;

        Vector2 direction = toWeapon / weaponCenterDistance;
        Collider2D characterCollider = GetComponent<Collider2D>();
        float characterExtent = 0f;
        if (characterCollider != null)
        {
            Bounds characterBounds = characterCollider.bounds;
            characterExtent = Mathf.Abs(direction.x) * characterBounds.extents.x +
                Mathf.Abs(direction.y) * characterBounds.extents.y;
        }

        Bounds weaponBounds = weaponHitbox.bounds;
        float weaponExtent = Mathf.Abs(direction.x) * weaponBounds.extents.x +
            Mathf.Abs(direction.y) * weaponBounds.extents.y;
        Vector2 castOrigin = origin + direction * characterExtent;
        float castDistance = weaponCenterDistance + weaponExtent - characterExtent;
        if (castDistance <= 0f)
            return false;

        Physics2D.SyncTransforms();
        RaycastHit2D hit = Physics2D.Raycast(castOrigin, direction, castDistance, wallCollisionMask);
        if (hit.collider == null || hit.collider.isTrigger || hit.collider.CompareTag("Enemy") ||
            hit.transform == transform || hit.transform.IsChildOf(transform))
            return false;

        float allowedCenterDistance = Mathf.Max(
            characterExtent,
            characterExtent + hit.distance - weaponExtent - (wallClearance * 6f));
        weaponTransform.position = origin + direction * allowedCenterDistance;
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (weaponBlockedByWall || weaponHitbox == null || !weaponHitbox.enabled)
            return;

        Movement movement = GetComponent<Movement>();
        if (movement == null || !movement.IsDamageDash)
            return;

        WeaponDefinition weapon = EquippedWeapon;
        if (weapon == null)
            return;

        EnemyHealthManager target = GetEnemyHealth(other);
        if (target == null)
            return;

        movement.RegisterDashHit();

        int damageMultiplier = movement.IsDamageBoostedDash ? 2 : 1;
        float directDamage = weapon.weaponName == "hammer"
            ? 1 + currentDamageBuff
            : weapon.damageInHits + currentDamageBuff + movement.ComboDamageBonus;
        target.TakeSwordHit((int)directDamage * damageMultiplier);

        switch (weapon.weaponName)
        {
            case "flimsy_spr":
                DamageEnemyBehind(target, 1 * damageMultiplier);
                break;
            case "spear":
                DamageEnemyBehind(target, 3 * damageMultiplier);
                break;
            case "hammer":
                DamageNearbyEnemies(target, 1 * damageMultiplier);
                break;
        }
    }

    private EnemyHealthManager GetEnemyHealth(Collider2D collider)
    {
        EnemyHealthManager enemyHealth = collider.GetComponentInParent<EnemyHealthManager>();
        return enemyHealth != null && enemyHealth.gameObject.CompareTag("Enemy") ? enemyHealth : null;
    }

    private void DamageEnemyBehind(EnemyHealthManager target, int damageInHits)
    {
        Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f)
            return;
        direction.Normalize();

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
            target.transform.position,
            behindEnemySearchDistance + behindEnemySideTolerance);

        EnemyHealthManager nearestBehind = null;
        float nearestDistance = float.MaxValue;
        HashSet<EnemyHealthManager> checkedEnemies = new HashSet<EnemyHealthManager>();

        foreach (Collider2D collider in nearbyColliders)
        {
            EnemyHealthManager candidate = GetEnemyHealth(collider);
            if (candidate == null || candidate == target || !checkedEnemies.Add(candidate))
                continue;

            Vector2 offset = (Vector2)candidate.transform.position - (Vector2)target.transform.position;
            float forwardDistance = Vector2.Dot(offset, direction);
            float sideDistance = Mathf.Abs(direction.x * offset.y - direction.y * offset.x);
            if (forwardDistance <= 0.05f || forwardDistance > behindEnemySearchDistance ||
                sideDistance > behindEnemySideTolerance)
                continue;

            if (forwardDistance < nearestDistance)
            {
                nearestDistance = forwardDistance;
                nearestBehind = candidate;
            }
        }

        if (nearestBehind != null)
            nearestBehind.TakeSwordHit(damageInHits);
    }

    private void DamageNearbyEnemies(EnemyHealthManager target, int damageInHits)
    {
        Vector2 attackDirection = (Vector2)target.transform.position - (Vector2)transform.position;
        if (attackDirection.sqrMagnitude < 0.0001f)
            return;
        attackDirection.Normalize();

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(target.transform.position, hammerSplashRadius * 0.4f);
        HashSet<EnemyHealthManager> damagedEnemies = new HashSet<EnemyHealthManager>();

        foreach (Collider2D collider in nearbyColliders)
        {
            EnemyHealthManager candidate = GetEnemyHealth(collider);
            if (candidate == null || candidate == target || !damagedEnemies.Add(candidate))
                continue;

            Vector2 offsetFromPlayer = (Vector2)candidate.transform.position - (Vector2)transform.position;
            if (Vector2.Dot(offsetFromPlayer, attackDirection) <= 0f)
                continue;

            candidate.TakeSwordHit(damageInHits);
        }
    }

    private void CreateAimVisuals()
    {
        SpriteRenderer originalRenderer = GetComponent<SpriteRenderer>();
        if (originalRenderer == null)
            return;

        aimPivot = new GameObject("AimPivot").transform;
        aimPivot.SetParent(transform, false);

        GameObject characterVisual = new GameObject("CharacterVisual");
        characterVisual.transform.SetParent(aimPivot, false);
        SpriteRenderer characterRenderer = characterVisual.AddComponent<SpriteRenderer>();
        characterRenderer.sprite = originalRenderer.sprite;
        characterRenderer.color = GameSettings.PlayerColor;
        characterRenderer.sharedMaterial = originalRenderer.sharedMaterial;
        characterRenderer.sortingLayerID = originalRenderer.sortingLayerID;
        characterRenderer.sortingOrder = originalRenderer.sortingOrder;
        characterRenderer.flipX = originalRenderer.flipX;
        characterRenderer.flipY = originalRenderer.flipY;
        characterRenderer.maskInteraction = originalRenderer.maskInteraction;
        characterRenderer.spriteSortPoint = originalRenderer.spriteSortPoint;
        originalRenderer.enabled = false;

        GameObject weapon = new GameObject("Weapon");
        weaponTransform = weapon.transform;
        weaponTransform.SetParent(aimPivot, false);

        weaponRenderer = weapon.AddComponent<SpriteRenderer>();
        weaponRenderer.color = Color.white;
        weaponRenderer.sharedMaterial = originalRenderer.sharedMaterial;
        weaponRenderer.sortingLayerID = originalRenderer.sortingLayerID;
        weaponRenderer.sortingOrder = originalRenderer.sortingOrder + 1;

        weaponHitbox = weapon.AddComponent<BoxCollider2D>();
        weaponHitbox.isTrigger = true;
    }

    public bool EquipWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weapons.Count || weaponRenderer == null)
            return false;

        WeaponDefinition weapon = weapons[weaponIndex];
        Tile tile = weapon.tile as Tile;
        if (tile == null || tile.sprite == null)
            return false;

        equippedWeaponIndex = weaponIndex;
        weaponRenderer.sprite = tile.sprite;
        weaponTransform.localPosition = weapon.offset;
        weaponTransform.localScale = weapon.scale;
        weaponTransform.localRotation = Quaternion.Euler(0f, 0f, weapon.spriteAngleOffset);
        weaponHitbox.size = tile.sprite.bounds.size;
        return true;
    }

    public bool TryGetRandomWeaponDrop(out int weaponIndex)
    {
        weaponIndex = -1;
        WeaponDefinition equippedWeapon = EquippedWeapon;
        if (equippedWeapon == null)
            return false;

        List<int> eligibleWeapons = new List<int>();
        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponDefinition candidate = weapons[i];
            if (candidate == null || candidate.tile == null ||
                Mathf.Abs(candidate.tier - equippedWeapon.tier) > 1)
                continue;

            eligibleWeapons.Add(i);
        }

        if (eligibleWeapons.Count == 0)
            return false;

        weaponIndex = eligibleWeapons[Random.Range(0, eligibleWeapons.Count)];
        return true;
    }
}
