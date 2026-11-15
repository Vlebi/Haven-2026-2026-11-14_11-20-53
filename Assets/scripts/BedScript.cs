using UnityEngine;

public class BedScript : MonoBehaviour
{
    [Header("Settings")]
    public string interactTag = "Interactable";
    public float interactRange = 2.5f;
    public KeyCode interactKey = KeyCode.F;

    // A kulcs neve, ami alatt a Unity tárolja a mentést
    private const string SAVE_KEY = "SavedWave";

    void Update()
    {
        // Ha lenyomja az F gombot és egy mentőpont (Interactable) közelében van
        if (Input.GetKeyDown(interactKey) && IsNearInteractable())
        {
            SaveCurrentWave();
        }
    }

    // Ellenőrzi, van-e "Interactable" taggel ellátott tárgy a közelben
    bool IsNearInteractable()
    {
        GameObject[] interactables = GameObject.FindGameObjectsWithTag(interactTag);
        foreach (GameObject obj in interactables)
        {
            if (Vector2.Distance(transform.position, obj.transform.position) <= interactRange)
            {
                return true;
            }
        }
        return false;
    }

    // 1. MENTÉS: Ezt hívja meg az interactálás
    public void SaveCurrentWave()
    {
        // IDE ÍRD BE a saját WaveSystemManager változód nevét!
        // Példa: int currentWave = WaveSystemManager.instance.currentWave;
        int currentWave = FindFirstObjectByType<WaveSystemManager>().currentWaveIndex;

        PlayerPrefs.SetInt(SAVE_KEY, currentWave);
        PlayerPrefs.Save(); // Azonnal kiírja a lemezre

        Debug.Log("Game saved! Wave: " + currentWave);
    }

    // 2. BETÖLTÉS: Ezt hívd meg a WaveSystemManager Start() függvényében!
    public static int LoadSavedWave()
    {
        // Ha nincs mentés, az 1-es hullámmal tér vissza
        return PlayerPrefs.GetInt(SAVE_KEY, 1);
    }

    // 3. TÖRLÉS: Ezt hívd meg, amikor a játékos meghal!
    public static void DeleteSaveOnDeath()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Debug.Log("Game over – the saved wave has been deleted!");
        }
    }
}