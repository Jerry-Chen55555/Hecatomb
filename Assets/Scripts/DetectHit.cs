using UnityEngine;
using UnityEngine.InputSystem;

public class DetectHit : MonoBehaviour
{
    public int Damage = 25;
    public EnemyEventChannel DamageRequestChannel;
    public EnemyEventChannel PlayerHitChannel;

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
        Projectile otherProjectile = other.GetComponentInParent<Projectile>();
        LungeBehavior lungeAttacker = GetComponentInParent<LungeBehavior>();
        PunchingBehavior punchingAttacker = GetComponentInParent<PunchingBehavior>();
        bool isProjectile = _projectile != null;

        if (isProjectile && otherProjectile != null)
        {
            return;
        }

        if (isProjectile && touchedEnemy && !_projectile.IsReflected)
        {
            return;
        }

        if (isProjectile && touchedEnemy && _projectile.IsReflected)
        {
            RequestDamage(other.transform.root.gameObject, Mathf.Max(_projectile.Damage, 100));
            _projectile.Despawn();
            return;
        }

        if (player != null)
        {
            if (isProjectile)
            {
                if (TryParry(player))
                {
                    if (_projectile.IsDroneShot)
                        player.GetComponent<ParryHandler>().AllowQuickParry();
                    Vector2 aim = Camera.main != null
                        ? (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - (Vector2)player.transform.position
                        : (Vector2)transform.position - (Vector2)player.transform.position;
                    _projectile.Reflect(aim);
                    return;
                }
                RequestPlayerDamage(player, _projectile.Damage);
                _projectile.Despawn();
            }
            else if (TryRegisterPlayerHit(lungeAttacker, punchingAttacker))
            {
                if (TryParry(player))
                {
                    RequestDamage(transform.root.gameObject, 999999);
                }
                else RequestPlayerDamage(player, Damage);
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
            RequestDamage(other.transform.root.gameObject, Damage);
        }
    }

    void RequestDamage(GameObject target, int amount)
    {
        if (DamageRequestChannel != null) DamageRequestChannel.Raise(target, amount);
    }

    void RequestPlayerDamage(PlayerController player, int amount)
    {
        if (PlayerHitChannel != null) PlayerHitChannel.Raise(player.gameObject, amount);
    }

    bool TryParry(PlayerController player)
    {
        ParryHandler parry = player.GetComponent<ParryHandler>();
        if (parry == null)
            return false;

        bool parried = parry.isParrying;
        bool perfectParry = parried
            && Time.time - parry.TimeLastParried >= 0f
            && Time.time - parry.TimeLastParried <= parry.ParryWindow;

        if (parried)
            parry.RegisterParrySuccess();

        if (perfectParry)
            player.Heal(parry.ParryHealAmount);

        return parried;
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
