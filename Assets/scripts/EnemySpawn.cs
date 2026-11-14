using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawn : MonoBehaviour
{
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
            SpawnEnemyToScene();
            spawnedCount++; // Hozzáadunk egyet a számlálóhoz
            timer = 0f;
        }
    }
    
    public void LowerSpawnedCount()
    {
        spawnedCount --;
    }

    private void SpawnEnemyToScene()
    {
        Scene targetScene = SceneManager.GetSceneByName(enemySceneName);

        if (targetScene.IsValid() && targetScene.isLoaded)
        {
            Vector3 spawnPos = GetOffScreenPosition();

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(newEnemy, targetScene);
        }
        else
        {
            Debug.LogWarning($"The {enemySceneName} Scene is not loaded!");
        }
    }

    private Vector3 GetOffScreenPosition()
    {
        if (Camera.main == null) return Vector3.zero;

        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float spawnX = 0f;
        float spawnY = 0f;

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