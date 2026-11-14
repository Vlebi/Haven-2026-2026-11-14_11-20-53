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

    [SerializeField] private List<WeaponDefinition> weapons = new List<WeaponDefinition>();
    [SerializeField] private int startingWeaponIndex;

    private Camera cam;
    private Transform aimPivot;
    private Transform weaponTransform;
    private SpriteRenderer weaponRenderer;
    private BoxCollider2D weaponHitbox;
    private int equippedWeaponIndex = -1;

    private WeaponDefinition EquippedWeapon =>
        equippedWeaponIndex >= 0 && equippedWeaponIndex < weapons.Count
            ? weapons[equippedWeaponIndex]
            : null;

    private void Start()
    {
        cam = Camera.main;
        CreateAimVisuals();
        EquipWeapon(startingWeaponIndex);
    }

    private void LateUpdate()
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        WeaponDefinition weapon = EquippedWeapon;
        if (weapon == null || !other.CompareTag("Enemy"))
            return;

        EnemyHealthManager enemyHealth = other.GetComponentInParent<EnemyHealthManager>();
        if (enemyHealth != null)
            enemyHealth.TakeSwordHit(weapon.damageInHits);
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
