using UnityEngine;

public sealed class EnemyEventListener : MonoBehaviour
{
    public EnemyEventChannel DamageRequestChannel;
    public EnemyEventChannel DamageChannel;
    public EnemyEventChannel PlayerHitChannel;
    public EnemyEventChannel DeathChannel;

    public void Configure(EnemyEventChannel damage, EnemyEventChannel playerHit, EnemyEventChannel death)
    {
        OnDisable();
        DamageChannel = damage;
        PlayerHitChannel = playerHit;
        DeathChannel = death;
        OnEnable();
    }

    public void ConfigureAll(EnemyEventChannel damageRequest, EnemyEventChannel damage, EnemyEventChannel playerHit, EnemyEventChannel death)
    {
        OnDisable();
        DamageRequestChannel = damageRequest;
        DamageChannel = damage;
        PlayerHitChannel = playerHit;
        DeathChannel = death;
        OnEnable();
    }

    void OnEnable()
    {
        if (DamageRequestChannel != null) DamageRequestChannel.Raised += OnDamageRequest;
        if (DamageChannel != null) DamageChannel.Raised += OnDamage;
        if (PlayerHitChannel != null) PlayerHitChannel.Raised += OnPlayerHit;
        if (DeathChannel != null) DeathChannel.Raised += OnDeath;
    }

    void OnDisable()
    {
        if (DamageRequestChannel != null) DamageRequestChannel.Raised -= OnDamageRequest;
        if (DamageChannel != null) DamageChannel.Raised -= OnDamage;
        if (PlayerHitChannel != null) PlayerHitChannel.Raised -= OnPlayerHit;
        if (DeathChannel != null) DeathChannel.Raised -= OnDeath;
    }

    void OnDamage(GameObject target, int amount)
    {
        if (target != gameObject && target.transform.root.gameObject != gameObject) return;
        SfxPlayer.Instance?.PlayEnemyHit();
        EnemyVisualEffects.FlashOnHit(gameObject);
    }

    void OnDamageRequest(GameObject target, int amount)
    {
        if (target != gameObject && target.transform.root.gameObject != gameObject) return;
        foreach (MonoBehaviour component in GetComponents<MonoBehaviour>())
            if (component is IDamageable damageable) { damageable.TakeDamage(amount); return; }
    }

    void OnPlayerHit(GameObject target, int amount)
    {
        if (target != gameObject && target.transform.root.gameObject != gameObject) return;
        PlayerController player = GetComponent<PlayerController>();
        if (player != null) player.TakeDamage(amount);
    }

    void OnDeath(GameObject target, int unused)
    {
        if (target != gameObject && target.transform.root.gameObject != gameObject) return;
        SfxPlayer.Instance?.PlayEnemyDeath();
        EnemyDeathFade fade = GetComponent<EnemyDeathFade>();
        if (fade == null) fade = gameObject.AddComponent<EnemyDeathFade>();
        if (fade is IEnemyDeath death) death.Die();
    }
}
