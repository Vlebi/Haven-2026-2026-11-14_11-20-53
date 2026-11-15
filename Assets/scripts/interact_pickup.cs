using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class interact_pickup : MonoBehaviour
{
    private static readonly List<interact_pickup> nearbyPickups = new List<interact_pickup>();
    private static GameObject interactionPrompt;
    private static int lastProcessedFrame = -1;

    [SerializeField, Min(0.1f)] private float pickupRange = 1.5f;

    private Attack playerAttack;
    private int weaponIndex = -1;
    private bool playerIsNearby;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D rangeCollider;

    public void Initialize(Attack attack, int index)
    {
        playerAttack = attack;
        weaponIndex = index;

        if (playerAttack == null || weaponIndex < 0 || weaponIndex >= playerAttack.weapons.Count)
        {
            Destroy(gameObject);
            return;
        }

        Tile weaponTile = playerAttack.weapons[weaponIndex].tile as Tile;
        if (weaponTile == null || weaponTile.sprite == null)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = weaponTile.sprite;
        spriteRenderer.sortingOrder = 20;
        transform.localScale = Vector3.one * 0.5f;

        rangeCollider = gameObject.AddComponent<CircleCollider2D>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = pickupRange;

        FindInteractionPrompt();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Attack attack = other.GetComponentInParent<Attack>();
        if (attack == null)
            return;

        playerAttack = attack;
        playerIsNearby = true;
        if (!nearbyPickups.Contains(this))
            nearbyPickups.Add(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Attack attack = other.GetComponentInParent<Attack>();
        if (attack == null || attack != playerAttack)
            return;

        playerIsNearby = false;
        nearbyPickups.Remove(this);
    }

    private void Update()
    {
        if (lastProcessedFrame == Time.frameCount)
            return;
        lastProcessedFrame = Time.frameCount;

        nearbyPickups.RemoveAll(pickup => pickup == null || !pickup.playerIsNearby);
        interact_pickup closestPickup = null;
        float closestDistance = float.MaxValue;

        foreach (interact_pickup pickup in nearbyPickups)
        {
            if (pickup.playerAttack == null)
                continue;

            float distance = (pickup.playerAttack.transform.position - pickup.transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestPickup = pickup;
                closestDistance = distance;
            }
        }

        FindInteractionPrompt();
        if (interactionPrompt != null && interactionPrompt.activeSelf != (closestPickup != null))
            interactionPrompt.SetActive(closestPickup != null);

        if (closestPickup != null && Input.GetKeyDown(KeyCode.F))
            closestPickup.PickUp();
    }

    private void PickUp()
    {
        if (playerAttack != null && playerAttack.EquipWeapon(weaponIndex))
            Destroy(gameObject);
    }

    private static void FindInteractionPrompt()
    {
        if (interactionPrompt != null)
            return;

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            Transform[] children = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == "Interact")
                {
                    interactionPrompt = child.gameObject;
                    return;
                }
            }
        }
    }
}
