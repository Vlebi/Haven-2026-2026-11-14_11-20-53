using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using System.Collections;
using TMPro;

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
    private const int SpawnPositionAttempts = 80;

    [Header("Wave Settings")]
    public Wave[] waves;
    public float timeBetweenWaves = 5f;

    [Header("Spawn Settings")]
    public float spawnBuffer = 1f;
    public string enemyTag = "Enemy";
    [Min(0)] public int remainingEnemyGrace = 3;

    [Header("Testing")]
    [SerializeField] private bool useInspectorWaveForTesting;
    public int currentWaveIndex = 7;
    private bool isWaveActive = false;
    private bool isSpawningWave;
    private int completedWaveCycles;
    private int enemyCountMultiplier = 1;
    public int CompletedWaveCycles => completedWaveCycles;
    [SerializeField] private TMP_Text currentWaveText;
    [SerializeField] private TMP_Text enemyRemainingText;

    void Start()
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("No waves are configured; the wave system cannot start.", this);
            enabled = false;
            return;
        }

        int startingWaveIndex = useInspectorWaveForTesting
            ? currentWaveIndex
            : BedScript.LoadSavedWave();
        currentWaveIndex = Mathf.Clamp(startingWaveIndex, 0, waves.Length - 1);
        completedWaveCycles = useInspectorWaveForTesting
            ? 0
            : Mathf.Max(0, BedScript.LoadSavedWaveCycle());
        for (int cycle = 0; cycle < completedWaveCycles && enemyCountMultiplier <= int.MaxValue / 2; cycle++)
            enemyCountMultiplier *= 2;
        enemyRemainingText = RuntimeHudText.GetOrCreate(
            "EnemyRemainingText", "Enemies remaining: 0", new Vector2(20f, -55f));
        TMP_FontAsset pijamasFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Pijamas-ejPO SDF");
        if (enemyRemainingText != null)
        {
            if (pijamasFont != null)
                enemyRemainingText.font = pijamasFont;
            enemyRemainingText.fontSize = 36f;
            enemyRemainingText.fontStyle = FontStyles.Bold;
        }
        UpdateHud(0);
        StartCoroutine(StartNextWave());
    }

    void Update()
    {
        int remainingEnemies = CountTopLevelEnemies();
        UpdateHud(remainingEnemies);

        if (isWaveActive && !isSpawningWave)
        {
            bool shouldAdvance = remainingEnemies == 0;
            Wave currentWave = waves[currentWaveIndex];
            int waveEnemyCount = GetWaveEnemyCount(currentWave);
            if (!shouldAdvance && CanUseRemainingEnemyGrace(currentWave, waveEnemyCount))
                shouldAdvance = remainingEnemies <= remainingEnemyGrace;

            if (shouldAdvance)
            {
                isWaveActive = false;
                currentWaveIndex++;
                if (currentWaveIndex >= waves.Length)
                {
                    currentWaveIndex = 0;
                    completedWaveCycles++;
                    enemyCountMultiplier = enemyCountMultiplier <= int.MaxValue / 2
                        ? enemyCountMultiplier * 2
                        : int.MaxValue;
                    Debug.Log($"All waves cleared! Starting cycle {completedWaveCycles + 1} with {enemyCountMultiplier}x enemy counts.", this);
                }
                StartCoroutine(StartNextWave());
            }
        }
    }

    private bool CanUseRemainingEnemyGrace(Wave wave, int waveEnemyCount)
    {
        return remainingEnemyGrace > 0
            && wave.enemyCount > remainingEnemyGrace
            && waveEnemyCount > remainingEnemyGrace;
    }

    private int GetWaveEnemyCount(Wave wave)
    {
        if (wave.enemyCount <= 0)
            return wave.enemyCount;

        return wave.enemyCount > int.MaxValue / enemyCountMultiplier
            ? int.MaxValue
            : wave.enemyCount * enemyCountMultiplier;
    }

    IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= 0 && currentWaveIndex < waves.Length)
        {
            Wave wave = waves[currentWaveIndex];
            int waveEnemyCount = GetWaveEnemyCount(wave);
            Debug.Log($"Next wave starting: {wave.waveName} ({waveEnemyCount} enemies)");
            UpdateCurrentWaveText();
            yield return new WaitForSeconds(timeBetweenWaves);
            yield return StartCoroutine(SpawnWave(wave, waveEnemyCount));
        }
    }

    private void UpdateCurrentWaveText()
    {
        if (currentWaveText == null)
            return;

        currentWaveText.text = completedWaveCycles == 0
            ? $"Wave: {currentWaveIndex + 1}"
            : $"Cycle {completedWaveCycles + 1} - Wave: {currentWaveIndex + 1}";
    }

    private void UpdateHud(int remainingEnemies)
    {
        UpdateCurrentWaveText();
        if (enemyRemainingText != null)
            enemyRemainingText.text = $"ENEMIES REMAINING: {remainingEnemies}";
    }

    private int CountTopLevelEnemies()
    {
        int count = 0;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag(enemyTag))
        {
            Transform parent = enemy.transform.parent;
            if (parent == null || !parent.CompareTag(enemyTag))
                count++;
        }

        return count;
    }

    IEnumerator SpawnWave(Wave wave, int targetEnemyCount)
    {
        isSpawningWave = true;
        int spawnedCount = 0;
        for (int i = 0; i < targetEnemyCount; i++)
        {
            if (SpawnEnemyToScene(wave.enemyPrefab, wave.targetSceneName))
                spawnedCount++;
            yield return new WaitForSeconds(wave.spawnDelay);
        }

        isSpawningWave = false;
        if (spawnedCount < targetEnemyCount)
        {
            Debug.LogError($"Wave '{wave.waveName}' only spawned {spawnedCount}/{targetEnemyCount} enemies. It will not advance; check NavMesh coverage and clear spawn space.", this);
            isWaveActive = false;
            yield break;
        }

        Debug.Log($"Wave '{wave.waveName}' spawned {spawnedCount}/{targetEnemyCount} enemy(ies).", this);
        isWaveActive = true;
    }

    private bool SpawnEnemyToScene(GameObject prefab, string sceneName)
    {
        Scene targetScene = SceneManager.GetSceneByName(sceneName);

        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            Debug.LogError($"The '{sceneName}' scene is not loaded; cannot spawn this wave.", this);
            return false;
        }

        if (prefab == null)
        {
            Debug.LogError("No Enemy Prefab assigned for this wave!", this);
            return false;
        }

        if (!TryGetSpawnPosition(targetScene, prefab, out Vector3 spawnPosition))
        {
            Debug.LogError($"No safe, reachable spawn position found for '{prefab.name}'. Enemy was not spawned.", this);
            return false;
        }

        GameObject newEnemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(newEnemy, targetScene);
        Debug.Log($"Spawned '{newEnemy.name}' for scene '{sceneName}' at {spawnPosition}.", this);
        return true;
    }

    private bool TryGetSpawnPosition(Scene targetScene, GameObject prefab, out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;
        Camera cam = Camera.main;
        if (cam == null || prefab == null || !TryGetMapBounds(targetScene, out Bounds mapBounds))
            return false;

        float cameraDepth = Mathf.Abs(cam.transform.position.z);
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cameraDepth));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cameraDepth));

        Collider2D enemyCollider = prefab.GetComponent<Collider2D>();
        Vector2 footprintSize = enemyCollider != null ? enemyCollider.bounds.size : Vector2.one * 0.5f;
        Vector2 colliderOffset = enemyCollider != null
            ? (Vector2)(enemyCollider.bounds.center - prefab.transform.position)
            : Vector2.zero;
        float marginX = Mathf.Max(spawnBuffer, footprintSize.x * 0.5f);
        float marginY = Mathf.Max(spawnBuffer, footprintSize.y * 0.5f);
        float minX = mapBounds.min.x + footprintSize.x * 0.5f - colliderOffset.x;
        float maxX = mapBounds.max.x - footprintSize.x * 0.5f - colliderOffset.x;
        float minY = mapBounds.min.y + footprintSize.y * 0.5f - colliderOffset.y;
        float maxY = mapBounds.max.y - footprintSize.y * 0.5f - colliderOffset.y;
        if (minX > maxX || minY > maxY)
            return false;

        EnemyFollow enemyFollow = prefab.GetComponent<EnemyFollow>();
        bool requiresNavMesh = enemyFollow != null && enemyFollow.useNavMeshPathfinding;
        NavMeshAgent prefabAgent = prefab.GetComponent<NavMeshAgent>();
        int areaMask = prefabAgent != null ? prefabAgent.areaMask : NavMesh.AllAreas;
        NavMeshPath path = null;
        Vector3 playerDestination = Vector3.zero;
        if (requiresNavMesh)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null || !NavMesh.SamplePosition(player.transform.position, out NavMeshHit playerHit, 2f, areaMask))
                return false;

            playerDestination = playerHit.position;
            path = new NavMeshPath();
        }

        int offscreenAttempts = SpawnPositionAttempts / 2;
        for (int attempt = 0; attempt < SpawnPositionAttempts; attempt++)
        {
            Vector2 candidate;
            if (attempt < offscreenAttempts)
            {
                if (!TryGetCandidatePosition(
                        Random.Range(0, 4), bottomLeft, topRight,
                        minX, maxX, minY, maxY, marginX, marginY,
                        colliderOffset, out candidate))
                    continue;
            }
            else
            {
                candidate = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            }

            if (requiresNavMesh)
            {
                float sampleDistance = Mathf.Max(0.75f, Mathf.Max(footprintSize.x, footprintSize.y));
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit spawnHit, sampleDistance, areaMask))
                    continue;

                candidate = spawnHit.position;
                if (!mapBounds.Contains(candidate))
                    continue;
                if (attempt < offscreenAttempts && !IsOutsideCamera(candidate, bottomLeft, topRight, marginX, marginY))
                    continue;
                if (!NavMesh.CalculatePath(spawnHit.position, playerDestination, areaMask, path)
                    || path.status != NavMeshPathStatus.PathComplete)
                    continue;
            }

            Vector2 colliderCenter = candidate + colliderOffset;
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(colliderCenter, footprintSize, 0f);
            bool isBlocked = false;
            foreach (Collider2D overlap in overlaps)
            {
                if (overlap != null && overlap.enabled && (!overlap.isTrigger || overlap.CompareTag("spikes")))
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

    private bool IsOutsideCamera(Vector2 position, Vector3 bottomLeft, Vector3 topRight, float marginX, float marginY)
    {
        return position.x < bottomLeft.x - marginX || position.x > topRight.x + marginX
            || position.y < bottomLeft.y - marginY || position.y > topRight.y + marginY;
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

}