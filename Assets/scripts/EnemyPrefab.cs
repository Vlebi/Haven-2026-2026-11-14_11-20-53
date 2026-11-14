using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    public string targetName = "char1"; // Ide beírhatod az Inspectorban is a célobjektum nevét

    private Transform playerTransform;

    void Start()
    {
        // Megkeressük a célobjektumot a pontos neve alapján az összes betöltött Scene-ben
        GameObject player = GameObject.Find(targetName);

        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"Could not find an object named '{targetName}' in the scene!");
        }
    }

    void Update()
    {
        // Ha megvan a célpont, az ellenség folyamatosan mozog felé
        if (playerTransform != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
        }
    }
}