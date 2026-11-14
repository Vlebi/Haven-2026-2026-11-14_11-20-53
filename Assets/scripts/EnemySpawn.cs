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

        if (!TryGetSpawnPosition(out Vector3 spawnPosition))
        {
            Debug.LogWarning("Could not find a clear enemy spawn position inside the camera bounds.", this);
            return false;
        }

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(newEnemy, targetScene);
        return true;
    }

    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;
        Camera cam = Camera.main;
        if (cam == null || enemyPrefab == null)
            return false;

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
        float minX = bottomLeft.x + marginX;
        float maxX = topRight.x - marginX;
        float minY = bottomLeft.y + marginY;
        float maxY = topRight.y - marginY;
        if (minX > maxX || minY > maxY)
            return false;

        for (int attempt = 0; attempt < SpawnPositionAttempts; attempt++)
        {
            Vector2 candidate = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
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
}