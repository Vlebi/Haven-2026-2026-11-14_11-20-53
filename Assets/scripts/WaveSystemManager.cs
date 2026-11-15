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
    public string targetSceneName = "Enemy"; // Alapból az Enemy Scene-be teszi, a 10. körnél átírhatod a Boss Scene nevére!
}

public class WaveSystemManager : MonoBehaviour
{
    [Header("Hullámok Beállítása")]
    public Wave[] waves;
    public float timeBetweenWaves = 5f;

    [Header("Spawn Beállítások")]
    public float spawnBuffer = 1f;
    public string enemyTag = "Enemy";

    public int currentWaveIndex = 0;
    private bool isWaveActive = false;

    void Start()
    {
        currentWaveIndex = BedScript.LoadSavedWave();
           
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
            Debug.Log($"Következő hullám indul: {waves[currentWaveIndex].waveName}");
            yield return new WaitForSeconds(timeBetweenWaves);
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }
        else
        {
            Debug.Log("Legyőzted a Bosst! Győzelem!");
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

        if (targetScene.IsValid() && targetScene.isLoaded)
        {
            Vector3 spawnPos = GetOffScreenPosition();
            GameObject newEnemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(newEnemy, targetScene);
        }
        else
        {
            Debug.LogWarning($"A '{sceneName}' nevű Scene nincs betöltve a Hierarchy-ban!");
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