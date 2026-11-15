using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[System.Serializable]
public class Wave
{
    public string waveName;
    public GameObject enemyPrefab;
    public int enemyCount;
    public float spawnDelay;
    public string targetSceneName = "map";
}

public class WaveSystemManager : MonoBehaviour
{
    private const int SpawnPositionAttempts = 30;

    [Header("Wave Settings")]
    public Wave[] waves;
    public float timeBetweenWaves = 5f;

    [Header("Spawn Settings")]
    public float spawnBuffer = 1f;
    public string enemyTag = "Enemy";

    public int currentWaveIndex = 0;
    private bool isWaveActive = false;

    void Start()
    {
        currentWaveIndex = 8;
        StartCoroutine(StartNextWave());
    }

    void Update()
    {
        if (isWaveActive && GameObject.FindGameObjectsWithTag(enemyTag).Length == 0)
        {
            isWaveActive = false;
            currentWaveIndex++;
            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator StartNextWave()
    {
        if (currentWaveIndex < waves.Length)
        {
            Debug.Log($"Next wave starting: {waves[currentWaveIndex].waveName}");
            yield return new WaitForSeconds(timeBetweenWaves);
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }
        else
        {
            Debug.Log("You defeated the Boss! Victory!");
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemyToScene(wave.enemyPrefab, wave.targetSceneName);
            yield return new WaitForSeconds(wave.spawnDelay);
        }

        isWaveActive = true;
    }

    private void SpawnEnemyToScene(GameObject prefab, string sceneName)
    {
        Scene targetScene = SceneManager.GetSceneByName(sceneName);

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            Debug.LogWarning($"The '{sceneName}' scene is not loaded in the Hierarchy!");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning("No Enemy Prefab assigned for this wave!", this);
            return;
        }

        if (!TryGetSpawnPosition(targetScene, prefab, out Vector3 spawnPosition))
        {
            Debug.LogWarning("Could not find a free spawn position on the map (Grid), using fallback position.", this);
            spawnPosition = GetFallbackOffScreenPosition();
        }

        GameObject newEnemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(newEnemy, targetScene);
    }

    private bool TryGetSpawnPosition(Scene targetScene, GameObject prefab, out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;
        Camera cam = Camera.main;
        if (cam == null || prefab == null)
            return false;

        if (!TryGetMapBounds(targetScene, out Bounds mapBounds))
            return false;

        float cameraDepth = Mathf.Abs(cam.transform.position.z);
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cameraDepth));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cameraDepth));

        Collider2D enemyCollider = prefab.GetComponent<Collider2D>();
        Vector2 footprintSize = enemyCollider != null
            ? enemyCollider.bounds.size
            : Vector2.one * 0.5f;
        Vector2 colliderOffset = enemyCollider != null
            ? (Vector2)(enemyCollider.bounds.center - prefab.transform.position)
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
        GameObject grid = null;
        GameObject[] sceneRoots = targetScene.GetRootGameObjects();
        foreach (GameObject root in sceneRoots)
        {
            if (root.name == "Grid")
            {
                grid = root;
                break;
            }
        }

        if (grid == null)
            return false;

        int cornerCount = 0;
        foreach (Transform child in grid.transform)
        {
            if (!child.name.StartsWith("corner", System.StringComparison.OrdinalIgnoreCase))
                continue;

            Vector3 cornerPosition = child.position;
            if (cornerCount == 0)
                mapBounds = new Bounds(cornerPosition, Vector3.zero);
            else
                mapBounds.Encapsulate(cornerPosition);

            cornerCount++;
        }

        return cornerCount >= 4 && mapBounds.size.x > 0f && mapBounds.size.y > 0f;
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

    private Vector3 GetFallbackOffScreenPosition()
    {
        if (Camera.main == null) return Vector3.zero;

        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        float spawnX = 0f, spawnY = 0f;

        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: spawnX = Random.Range(-camWidth, camWidth); spawnY = camHeight + spawnBuffer; break;
            case 1: spawnX = camWidth + spawnBuffer; spawnY = Random.Range(-camHeight, camHeight); break;
            case 2: spawnX = Random.Range(-camWidth, camWidth); spawnY = -(camHeight + spawnBuffer); break;
            case 3: spawnX = -(camWidth + spawnBuffer); spawnY = Random.Range(-camHeight, camHeight); break;
        }

        return new Vector3(cam.transform.position.x + spawnX, cam.transform.position.y + spawnY, 0f);
    }
}