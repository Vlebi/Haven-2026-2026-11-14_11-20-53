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
}

public class WaveSystemManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public Wave[] waves;
    public float timeBetweenWaves = 5f;

    [Header("Spawn Settings")]
    public string enemySceneName = "Enemy";
    public float spawnBuffer = 1f;

    // Hozzáadtunk egy Tag-et, ami alapján számoljuk az ellenfeleket
    public string enemyTag = "Enemy";

    private int currentWaveIndex = 0;

    // Ez a változó figyeli, hogy épp zajlik-e egy hullám
    private bool isWaveActive = false;

    void Start()
    {
        StartCoroutine(StartNextWave());
    }

    void Update()
    {
        // Ha egy hullám aktív, de már nincs egyetlen ellenfél sem a pályán
        if (isWaveActive && GameObject.FindGameObjectsWithTag(enemyTag).Length == 0)
        {
            // A hullám véget ért
            isWaveActive = false;

            // Jöhet a következő
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
            Debug.Log("All waves completed! Victory!");
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemyToScene(wave.enemyPrefab);
            yield return new WaitForSeconds(wave.spawnDelay);
        }

        // Amikor az utolsó ellenfél is lespawnolt a hullámban, "aktívvá" tesszük a hullámot, 
        // így az Update() elkezdi figyelni, hogy mikor halnak meg mind.
        isWaveActive = true;
    }

    private void SpawnEnemyToScene(GameObject prefab)
    {
        Scene targetScene = SceneManager.GetSceneByName(enemySceneName);

        if (targetScene.IsValid() && targetScene.isLoaded)
        {
            Vector3 spawnPos = GetOffScreenPosition();
            GameObject newEnemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(newEnemy, targetScene);
        }
    }

    private Vector3 GetOffScreenPosition()
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