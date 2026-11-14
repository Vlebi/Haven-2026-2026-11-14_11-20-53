using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawn : MonoBehaviour
{
    private const int SpawnPositionAttempts = 30;

    [Header("Settings")]
    public GameObject enemyPrefab;
    public string enemySceneName = "map";

    [Header("Spawn Parameters")]
    public float spawnInterval = 3f;
    public float spawnBuffer = 1f;

    [Header("Limit")]
    public int maxEnemies = 10; // Hány darab enemy jöhet létre összesen
    private int spawnedCount = 0; // Belső számláló

    private float timer = 0f;

    void Update()
    {
        // Ha elértük a limitet, a spawner nem csinál semmit
        if (spawnedCount >= maxEnemies)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            if (SpawnEnemyToScene())
                spawnedCount++;
            timer = 0f;
        }
    }
    
    public void LowerSpawnedCount()
    {
        spawnedCount --;
    }

    private bool SpawnEnemyToScene()
    {
        Scene targetScene = SceneManager.GetSceneByName(enemySceneName);

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            Debug.LogWarning($"The {enemySceneName} Scene is not loaded!");
            return false;
        }

        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawn has no enemy prefab assigned.", this);
            return false;
        }

        if (!TryGetSpawnPosition(targetScene, out Vector3 spawnPosition))
        {
            Debug.LogWarning("Could not find a clear enemy spawn position inside the camera bounds.", this);
            return false;
        }

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(newEnemy, targetScene);
        return true;
    }

    private bool TryGetSpawnPosition(Scene targetScene, out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;
        Camera cam = Camera.main;
        if (cam == null || enemyPrefab == null)
            return false;

        if (!TryGetMapBounds(targetScene, out Bounds mapBounds))
        {
            Debug.LogWarning("Could not find wall collider bounds in the enemy scene.", this);
            return false;
        }

        float cameraDepth = Mathf.Abs(cam.transform.position.z);
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cameraDepth));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cameraDepth));

        Collider2D enemyCollider = enemyPrefab.GetComponent<Collider2D>();
        Vector2 footprintSize = enemyCollider != null
            ? enemyCollider.bounds.size
            : Vector2.one * 0.5f;
        Vector2 colliderOffset = enemyCollider != null
            ? (Vector2)(enemyCollider.bounds.center - enemyPrefab.transform.position)
            : Vector2.zero;

        float marginX = Mathf.Max(spawnBuffer, footprintSize.x * 0.5f);
        float marginY = Mathf.Max(spawnBuffer, footprintSize.y * 0.5f);
        float minX = mapBounds.min.x + footprintSize.x * 0.5f - colliderOffset.x;
        float maxX = mapBounds.max.x - footprintSize.x * 0.5f - colliderOffset.x;
        float minY = mapBounds.min.y + footprintSize.y * 0.5f - colliderOffset.y;
        float maxY = mapBounds.max.y - footprintSize.y * 0.5f - colliderOffset.y;

        for (int attempt = 0; attempt < SpawnPositionAttempts; attempt++)
        {
            if (!TryGetCandidatePosition(
                    Random.Range(0, 4), bottomLeft, topRight,
                    minX, maxX, minY, maxY, marginX, marginY,
                    colliderOffset, out Vector2 candidate))
                continue;

            Vector2 colliderCenter = candidate + colliderOffset;
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(colliderCenter, footprintSize, 0f);
            bool isBlocked = false;

            foreach (Collider2D overlap in overlaps)
            {
                if (overlap != null && overlap.enabled && !overlap.isTrigger)
                {
                    isBlocked = true;
                    break;
                }
            }

            if (isBlocked)
                continue;

            spawnPosition = new Vector3(candidate.x, candidate.y, 0f);
            return true;
        }

        return false;
    }

    private bool TryGetMapBounds(Scene targetScene, out Bounds mapBounds)
    {
        mapBounds = default;
        int wallLayer = LayerMask.NameToLayer("wall");
        if (wallLayer < 0)
            return false;

        bool foundBounds = false;
        Renderer[] mapRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer mapRenderer in mapRenderers)
        {
            if (!mapRenderer.enabled || !mapRenderer.gameObject.activeInHierarchy ||
                mapRenderer.gameObject.scene != targetScene || mapRenderer.gameObject.layer != wallLayer)
                continue;

            if (!foundBounds)
            {
                mapBounds = mapRenderer.bounds;
                foundBounds = true;
            }
            else
            {
                mapBounds.Encapsulate(mapRenderer.bounds);
            }
        }

        return foundBounds && mapBounds.size.x > 0f && mapBounds.size.y > 0f;
    }

    private bool TryGetCandidatePosition(
        int edge,
        Vector3 cameraBottomLeft,
        Vector3 cameraTopRight,
        float mapMinX,
        float mapMaxX,
        float mapMinY,
        float mapMaxY,
        float marginX,
        float marginY,
        Vector2 colliderOffset,
        out Vector2 candidate)
    {
        candidate = Vector2.zero;
        float minX;
        float maxX;
        float minY;
        float maxY;

        switch (edge)
        {
            case 0:
                minX = Mathf.Max(mapMinX, cameraBottomLeft.x - colliderOffset.x);
                maxX = Mathf.Min(mapMaxX, cameraTopRight.x - colliderOffset.x);
                minY = cameraTopRight.y + marginY - colliderOffset.y;
                maxY = mapMaxY;
                break;
            case 1:
                minX = cameraTopRight.x + marginX - colliderOffset.x;
                maxX = mapMaxX;
                minY = Mathf.Max(mapMinY, cameraBottomLeft.y - colliderOffset.y);
                maxY = Mathf.Min(mapMaxY, cameraTopRight.y - colliderOffset.y);
                break;
            case 2:
                minX = Mathf.Max(mapMinX, cameraBottomLeft.x - colliderOffset.x);
                maxX = Mathf.Min(mapMaxX, cameraTopRight.x - colliderOffset.x);
                minY = mapMinY;
                maxY = cameraBottomLeft.y - marginY - colliderOffset.y;
                break;
            default:
                minX = mapMinX;
                maxX = cameraBottomLeft.x - marginX - colliderOffset.x;
                minY = Mathf.Max(mapMinY, cameraBottomLeft.y - colliderOffset.y);
                maxY = Mathf.Min(mapMaxY, cameraTopRight.y - colliderOffset.y);
                break;
        }

        if (minX > maxX || minY > maxY)
            return false;

        candidate = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        return true;
    }
}