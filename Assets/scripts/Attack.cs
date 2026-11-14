using UnityEngine;
using UnityEngine.Tilemaps;

public class Attack : MonoBehaviour
{
    [SerializeField] private TileBase startingWeaponTile;
    [SerializeField] private Vector2 weaponOffset = new Vector2(0.45f, 0f);
    [SerializeField] private float weaponSpriteAngleOffset;

    private Camera cam;
    private Transform aimPivot;

    private void Start()
    {
        cam = Camera.main;
        CreateAimVisuals();
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

    private void CreateAimVisuals()
    {
        SpriteRenderer originalRenderer = GetComponent<SpriteRenderer>();
        Tile weaponTile = startingWeaponTile as Tile;
        if (originalRenderer == null || weaponTile == null || weaponTile.sprite == null)
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

        GameObject weapon = new GameObject("ShortSword");
        weapon.transform.SetParent(aimPivot, false);
        weapon.transform.localPosition = weaponOffset;
        weapon.transform.localRotation = Quaternion.Euler(0f, 0f, weaponSpriteAngleOffset);

        SpriteRenderer weaponRenderer = weapon.AddComponent<SpriteRenderer>();
        weaponRenderer.sprite = weaponTile.sprite;
        weaponRenderer.color = Color.white;
        weaponRenderer.sharedMaterial = originalRenderer.sharedMaterial;
        weaponRenderer.sortingLayerID = originalRenderer.sortingLayerID;
        weaponRenderer.sortingOrder = originalRenderer.sortingOrder + 1;
    }
}
