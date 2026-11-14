using UnityEngine;

public class EnemyHealthManager : MonoBehaviour
{
    [SerializeField] private int hitsToKill = 5;

    private int hitsRemaining;

    private void Awake()
    {
        hitsRemaining = Mathf.Max(1, hitsToKill);
    }

    public void TakeSwordHit(int hitDamage = 1)
    {
        hitsRemaining -= Mathf.Max(1, hitDamage);
        if (hitsRemaining <= 0)
            Destroy(gameObject);
    }
}
