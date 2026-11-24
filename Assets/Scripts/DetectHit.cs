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

        GameObject enemyTarget = FindDamageable(other.transform);
        bool touchedEnemy = enemyTarget != null;
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
            IDamageable damageable = FindDamageableComponent(enemyTarget);
            int damage = _projectile.IsPerfectParry
                ? 999999
                : Mathf.CeilToInt(damageable.CurrentHealth * 0.5f);
            RequestDamage(enemyTarget, damage);
            _projectile.Despawn();
            return;
        }

        if (player != null)
        {
            if (isProjectile)
            {
                if (TryParry(player, out bool perfectParry))
                {
                    if (_projectile.IsDroneShot)
                        player.GetComponent<ParryHandler>().AllowQuickParry();
                    if (perfectParry)
                        _projectile.MarkPerfectParry();
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
                if (TryParry(player, out bool perfectParry))
                {
                    GameObject attacker = FindDamageable(transform);
                    if (attacker != null)
                    {
                        IDamageable damageable = FindDamageableComponent(attacker);
                        RequestDamage(attacker, perfectParry
                            ? 999999
                            : Mathf.CeilToInt(damageable.CurrentHealth * 0.5f));
                    }
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

        if (FindDamageable(transform) != null)
        {
            return;
        }

        if (touchedEnemy)
        {
            RequestDamage(enemyTarget, Damage);
        }
    }

    GameObject FindDamageable(Transform current)
    {
        while (current != null)
        {
            foreach (MonoBehaviour component in current.GetComponents<MonoBehaviour>())
                if (component is IDamageable)
                    return current.gameObject;

            current = current.parent;
        }

        return null;
    }

    IDamageable FindDamageableComponent(GameObject target)
    {
        foreach (MonoBehaviour component in target.GetComponents<MonoBehaviour>())
            if (component is IDamageable damageable)
                return damageable;

        return target.GetComponentInParent<IDamageable>();
    }

    void RequestDamage(GameObject target, int amount)
    {
        target.SendMessage("TakeDamage", amount, SendMessageOptions.DontRequireReceiver);
    }

    void RequestPlayerDamage(PlayerController player, int amount)
    {
        if (PlayerHitChannel != null) PlayerHitChannel.Raise(player.gameObject, amount);
    }

    bool TryParry(PlayerController player, out bool perfectParry)
    {
        perfectParry = false;
        ParryHandler parry = player.GetComponent<ParryHandler>();
        if (parry == null)
            return false;

        float parryElapsed = Time.time - parry.TimeLastParried;
        perfectParry = parry.isParrying
            && parryElapsed >= 0f
            && parryElapsed <= parry.ParryWindow;
        bool parried = parry.isParrying
            && parryElapsed >= 0f
            && parryElapsed <= parry.BlockWindow;

        if (parried)
        {
            if (perfectParry)
                parry.RegisterPerfectParrySuccess();
            else
                parry.RegisterParrySuccess();
        }

        if (perfectParry)
        {
            parry.RegisterPerfectParry(transform.root.gameObject);
            player.Heal(parry.ParryHealAmount);
        }
        else if (parried)
        {
            player.Heal(Mathf.CeilToInt(parry.ParryHealAmount / 3f), false);
        }

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
