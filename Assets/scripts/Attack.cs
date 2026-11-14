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
        [Min(1)] public int damageInHits = 1;
        public Vector2 offset = new Vector2(0.45f, 0f);
        public Vector3 scale = Vector3.one;
        public float spriteAngleOffset;
    }

    public int currentDamageBuff = 0;

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
    public int equippedWeaponIndex = -1;

    public WeaponDefinition EquippedWeapon =>
        equippedWeaponIndex >= 0 && equippedWeaponIndex < weapons.Count
            ? weapons[equippedWeaponIndex]
            : null;

    private void Start()
    {
        cam = Camera.main;
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
        mouseScreenPosition.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 direction = (Vector2)mouseWorldPosition - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        WeaponDefinition weapon = EquippedWeapon;
        if (weapon != null && weaponTransform != null && weaponHitbox != null)
        {
            weaponTransform.localPosition = weapon.offset;
            ClampWeaponToWall();
            Physics2D.SyncTransforms();
        }
    }

    private void ClampWeaponToWall()
    {
        Vector2 origin = transform.position;
        Vector2 desiredPosition = weaponTransform.position;
        Vector2 toWeapon = desiredPosition - origin;
        float distance = toWeapon.magnitude;
        if (distance <= 0.001f)
            return;

        Vector2 direction = toWeapon / distance;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        Bounds hitboxBounds = weaponHitbox.bounds;
        float halfWidth = Mathf.Abs(perpendicular.x) * hitboxBounds.extents.x +
            Mathf.Abs(perpendicular.y) * hitboxBounds.extents.y;

        float allowedDistance = distance;
        for (int rayIndex = -2; rayIndex <= 2; rayIndex++)
        {
            Vector2 rayOrigin = origin + perpendicular * (halfWidth * rayIndex / 2f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                rayOrigin,
                direction,
                distance,
                wallCollisionMask);

            foreach (RaycastHit2D hit in hits)
            {
                Collider2D obstacle = hit.collider;
                if (obstacle == null || obstacle.isTrigger ||
                    obstacle.transform == transform || obstacle.transform.IsChildOf(transform) ||
                    obstacle.CompareTag("Enemy"))
                    continue;

                float hitDistance = Vector2.Dot(hit.point - origin, direction);
                if (hitDistance <= 0f)
                    continue;

                allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, hitDistance - wallClearance));
            }
        }

        weaponTransform.position = origin + direction * allowedDistance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        WeaponDefinition weapon = EquippedWeapon;
        if (weapon == null)
            return;

        EnemyHealthManager target = GetEnemyHealth(other);
        if (target == null)
            return;

        target.TakeSwordHit(weapon.damageInHits + currentDamageBuff);

        switch (weapon.weaponName)
        {
            case "flimsy_spr":
                DamageEnemyBehind(target, 1);
                break;
            case "spear":
                DamageEnemyBehind(target, 3);
                break;
            case "hammer":
                DamageNearbyEnemies(target, 3);
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
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(target.transform.position, hammerSplashRadius);
        HashSet<EnemyHealthManager> damagedEnemies = new HashSet<EnemyHealthManager>();

        foreach (Collider2D collider in nearbyColliders)
        {
            EnemyHealthManager candidate = GetEnemyHealth(collider);
            if (candidate == null || candidate == target || !damagedEnemies.Add(candidate))
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
        characterRenderer.color = originalRenderer.color;
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
}
