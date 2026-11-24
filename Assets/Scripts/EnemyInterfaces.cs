public interface IDamageable
{
    int CurrentHealth { get; }
    void TakeDamage(int amount);
}

public interface IEnemyPlayerHit
{
    bool TryRegisterPlayerHit();
}

public interface IEnemyDeath
{
    void Die();
}
