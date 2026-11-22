using UnityEngine;
using UnityEngine.InputSystem;

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

        if (isProjectile && touchedEnemy && !_projectile.IsReflected)
        {
            return;
        }

        if (isProjectile && touchedEnemy && _projectile.IsReflected)
        {
            other.transform.root.SendMessage("TakeDamage", Mathf.Max(_projectile.Damage, 100), SendMessageOptions.DontRequireReceiver);
            _projectile.Despawn();
            return;
        }

        if (player != null)
        {
            if (isProjectile)
            {
                if (TryParry(player))
                {
                    Vector2 aim = Camera.main != null
                        ? (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - (Vector2)player.transform.position
                        : (Vector2)transform.position - (Vector2)player.transform.position;
                    _projectile.Reflect(aim);
                    return;
                }
                player.Health = Mathf.Max(0, player.Health - _projectile.Damage);
                _projectile.Despawn();
            }
            else if (TryRegisterPlayerHit(lungeAttacker, punchingAttacker))
            {
                if (TryParry(player))
                {
                    transform.root.SendMessage("TakeDamage", 999999, SendMessageOptions.DontRequireReceiver);
                }
                else player.Health = Mathf.Max(0, player.Health - Damage);
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

    bool TryParry(PlayerController player)
    {
        ParryHandler parry = player.GetComponent<ParryHandler>();
        return parry != null && Time.time - parry.TimeLastParried <= parry.ParryWindow;
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
