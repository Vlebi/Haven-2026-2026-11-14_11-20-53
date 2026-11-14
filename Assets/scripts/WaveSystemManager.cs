using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Ez az osztály írja le, hogy mit tartalmaz egy hullám
[System.Serializable]
public class Wave
{
    public string waveName; // Pl. "1. Hullám"
    public GameObject enemyPrefab; // Milyen ellenfél jöjjön
    public int enemyCount; // Hány darab jöjjön összesen
    public float spawnDelay; // Hány másodperc teljen el két spawnolás között
}

public class WaveSystemManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public Wave[] waves; // A hullámok listája
    public float timeBetweenWaves = 5f; // Szünet a hullámok között másodpercben

    [Header("Spawn Settings")]
    public string enemySceneName = "Enemy"; // Ide tesszük be őket (maradhat az eddigi)
    public float spawnBuffer = 1f;

    private int currentWaveIndex = 0;

    void Start()
    {
        // Elindítjuk a hullámok kezelését
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        // Ha van még hátra hullám
        if (currentWaveIndex < waves.Length)
        {
            Debug.Log($"Next wave starting: {waves[currentWaveIndex].waveName}");

            // Várunk egy kicsit a hullám indulása előtt
            yield return new WaitForSeconds(timeBetweenWaves);

            // Elindítjuk az aktuális hullám spawnolását
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));

            // Ha lement a hullám, növeljük az indexet, és jöhet a következő
            currentWaveIndex++;
            StartCoroutine(StartNextWave());
        }
        else
        {
            Debug.Log("All waves completed! Victory!");
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        // Ledaráljuk annyiszor, ahány ellenfél van a hullámban
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemyToScene(wave.enemyPrefab);

            // Várunk a megadott ideig a következő ellenfél lerakása előtt
            yield return new WaitForSeconds(wave.spawnDelay);
        }
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