using UnityEngine;

public class DetectHit : MonoBehaviour
{
    public int Damage = 25;

    private Projectile _projectile;

    void Awake()
    {
        _projectile = GetComponent<Projectile>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root == transform.root)
        {
            return;
        }

        bool touchedEnemy = other.transform.root.CompareTag("Enemy");
        PlayerController player = other.GetComponentInParent<PlayerController>();
        LungeBehavior lungeAttacker = GetComponentInParent<LungeBehavior>();
        PunchingBehavior punchingAttacker = GetComponentInParent<PunchingBehavior>();
        bool isProjectile = _projectile != null;

        if (isProjectile && touchedEnemy)
        {
            return;
        }

        if (player != null)
        {
            if (isProjectile)
            {
                player.Health = Mathf.Max(0, player.Health - _projectile.Damage);
                _projectile.Despawn();
            }
            else if (TryRegisterPlayerHit(lungeAttacker, punchingAttacker))
            {
                player.Health = Mathf.Max(0, player.Health - Damage);
            }

            return;
        }

        if (isProjectile)
        {
            _projectile.Despawn();
            return;
        }

        if (transform.root.CompareTag("Enemy"))
        {
            return;
        }

        if (touchedEnemy)
        {
            other.transform.root.SendMessage("TakeDamage", Damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    bool TryRegisterPlayerHit(LungeBehavior lungeEnemy, PunchingBehavior punchingEnemy)
    {
        if (lungeEnemy != null)
        {
            return lungeEnemy.TryRegisterPlayerHit();
        }

        if (punchingEnemy != null)
        {
            return punchingEnemy.TryRegisterPlayerHit();
        }

        return true;
    }

    public static void IgnoreEnemyCollisions(GameObject enemy)
    {
        Collider2D[] enemyColliders = enemy.GetComponentsInChildren<Collider2D>();

        foreach (LungeBehavior otherEnemy in FindObjectsOfType<LungeBehavior>())
        {
            IgnoreCollisions(enemy, enemyColliders, otherEnemy.gameObject);
        }

        foreach (PunchingBehavior otherEnemy in FindObjectsOfType<PunchingBehavior>())
        {
            IgnoreCollisions(enemy, enemyColliders, otherEnemy.gameObject);
        }

        foreach (RangedBehavior otherEnemy in FindObjectsOfType<RangedBehavior>())
        {
            IgnoreCollisions(enemy, enemyColliders, otherEnemy.gameObject);
        }
    }

    static void IgnoreCollisions(GameObject enemy, Collider2D[] enemyColliders, GameObject otherEnemy)
    {
        if (enemy == otherEnemy)
        {
            return;
        }

        Collider2D[] otherColliders = otherEnemy.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D enemyCollider in enemyColliders)
        {
            foreach (Collider2D otherCollider in otherColliders)
            {
                Physics2D.IgnoreCollision(enemyCollider, otherCollider);
            }
        }
    }
}
